using CSVAnalyzer.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CSVAnalyzer
{
    public partial class Form1 : Form
    {
        private readonly JsonFileManager<AppSettings> _jsonManager;
        private readonly CsvWriter _csvWriter;
        private AppSettings _settings;


        public Form1()
        {
            InitializeComponent();
            _csvWriter = new CsvWriter("data.csv");

            var models = _csvWriter.ReadModels();
            dgvCsvResult.DataSource = models;

            // Указываем путь к файлу настроек
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            // Создаем менеджер для работы с JSON
            _jsonManager = new JsonFileManager<AppSettings>(filePath);
            // Считываем настройки из файла
            _settings = _jsonManager.Read();


            tBarSmooth.Value = Convert.ToInt32(_settings.SmoothSize);
            tbSmooth.Text = _settings.SmoothSize;

            tbThreshold.Text = _settings.Threshold;
            tBarThreshold.Value = Convert.ToInt32(Convert.ToDouble(_settings.Threshold.Replace(".", ",")) * 1000);

            numericUpDown1.Value = _settings.ChartInterval;

        }

        public SubstanceData SelectedSubstanceData { get; set; }
        public int SmoothSize
        {
            get
            {
                try
                {
                    int smoothSize = Convert.ToInt32(tbSmooth.Text);
                    return smoothSize;
                }
                catch (Exception ex)
                {
                    return 5;
                }
            }
        }
        public List<(double Wavelength, double Abs)> SmoothedMeasurments
        {
            get
            {
                if (SelectedSubstanceData == null)
                    return null;

                try
                {
                    var smoothedMeasurments = SubstanceData.Smooth(SelectedSubstanceData.Measurements, SmoothSize);
                    return smoothedMeasurments;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public string PathFile { get; set; } = @"D:\Языки программирования\GitHub_Hilaim\ABS_DMSO_10uM.csv";

        private void button2_Click(object sender, EventArgs e)
        {
            // Выбор файла из проводника
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PathFile = dialog.FileName;
                textBox1.Text = PathFile;
            }
            try
            {

                List<SubstanceData> dataCsv = CsvProcessor.LoadFromFile(PathFile);

                dataGridView1.DataSource = dataCsv;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не смог прочитать файл CSV. Кидай файл t.me/dv0888.{Environment.NewLine}{ex.Message}", "ТРАГЕДИЯ!");
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Проверяем, что выделена хотя бы одна строка
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Получаем первую выделенную строку
                var selectedRow = dataGridView1.SelectedRows[0];

                // Извлекаем привязанный объект
                SelectedSubstanceData = (SubstanceData)selectedRow.DataBoundItem;

                if (SelectedSubstanceData != null)
                {
                    // Преобразуем Measurements для отображения в DataGridView2
                    var measurements = SelectedSubstanceData.Measurements
                        .Select(m => new { m.Wavelength, m.Abs })
                        .ToList();

                    // Привязываем данные ко второму DataGridView
                    dataGridView2.DataSource = measurements;
                    dataGridView2.ClearSelection();
                }

                DisplayChart1();
                DisplayChart2();

                // Обработка и отображение данных на графике

                GetMaxPoints();

                ClearSelectionRows();
            }
        }


        #region DisplayChart
        private void DisplayChart1()
        {
            DisplayChart(chart1, SelectedSubstanceData.Measurements);
        }
        private void DisplayChart2()
        {
            if (SelectedSubstanceData == null)
                return;

            try
            {
                var startPoint = SelectedSubstanceData.Measurements[SelectedSubstanceData.Measurements.Count - 1].Wavelength;

                var endPoint = SelectedSubstanceData.Measurements[0].Wavelength;

                DisplayChart(chart2, SmoothedMeasurments, startPoint, endPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DisplayChart(Chart chart, List<(double Wavelength, double Abs)> measurements, double? xAxisMin = null, double? xAxisMax = null)
        {
            chart.Series.Clear();

            // Если серия не найдена, создаём новую
            var series = new Series
            {
                Name = "Absorption Spectrum",
                ChartType = SeriesChartType.Line,  // Тип графика (линия)
                BorderWidth = 4,
                IsVisibleInLegend = false,
            };

            // Добавляем данные из Measurements в серию
            foreach (var measurement in measurements)
            {
                series.Points.AddXY(measurement.Wavelength, measurement.Abs);
            }

            // Добавляем серию на график
            chart.Series.Add(series);

            // Настроим оси графика (если нужно изменить их титулы)
            chart.ChartAreas[0].AxisX.Title = "Wavelength (nm)";
            chart.ChartAreas[0].AxisY.Title = "Absorption (AU)";

            // Настройка частоты отображения меток на осях
            chart.ChartAreas[0].AxisX.Interval = _settings.ChartInterval; // Интервал между метками по оси X

            // Дополнительные настройки оси X (например, начало и конец)
            chart.ChartAreas[0].AxisX.Minimum = xAxisMin ?? measurements.Min(m => m.Wavelength);
            chart.ChartAreas[0].AxisX.Maximum = xAxisMax ?? measurements.Max(m => m.Wavelength);
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "0";
        }

        private void ChangeChartInterval(Chart chart, int interval)
        {
            chart.ChartAreas[0].AxisX.Interval = interval; // Интервал между метками по оси X
        }

        private void AddVerticalLine(Chart chart, List<double> xAxisValueCollection, Color color)
        {
            // Находим все существующие аннотации с указанным цветом
            var annotationsToRemove = chart.Annotations.OfType<VerticalLineAnnotation>()
                                       .Where(a => a.LineColor == color)
                                       .ToList();

            // Удаляем найденные аннотации
            foreach (var annotation in annotationsToRemove)
            {
                chart.Annotations.Remove(annotation);
            }

            if (xAxisValueCollection == null)
                return;

            foreach (var xAxisValue in xAxisValueCollection)
            {
                var lineAnnotation = new VerticalLineAnnotation();
                lineAnnotation.AxisXName = "ChartArea1\\rX";
                lineAnnotation.Height = 121D;
                lineAnnotation.LineColor = color;
                lineAnnotation.X = xAxisValue; lineAnnotation.Y = 0;
                lineAnnotation.LineWidth = 3;

                // Добавляем аннотацию на график
                chart.Annotations.Add(lineAnnotation);
            }
        }


        #endregion
        #region Smooth

        private void tBarSmooth_Scroll(object sender, EventArgs e)
        {
            tbSmooth.Text = tBarSmooth.Value.ToString();
        }

        private void tbSmooth_TextChanged(object sender, EventArgs e)
        {
            // Проверяем, является ли ввод числом
            if (!int.TryParse(tbSmooth.Text, out int value) || value < 0)
            {
                // Если не число или вне диапазона, удаляем последний символ
                MessageBox.Show("Введите число от 1.", "Неверный ввод", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbSmooth.Text = tBarSmooth.Value.ToString(); // Очищаем текстовое поле
                return;
            }

            DisplayChart2();
            GetMaxPoints();

            _settings.SmoothSize = tbSmooth.Text;
            _jsonManager.Write(_settings);
        }

        #endregion
        #region Threshold
        private void tBarThreshold_Scroll(object sender, EventArgs e)
        {
            tbThreshold.Text = (Convert.ToDouble(tBarThreshold.Value, CultureInfo.InvariantCulture) / 1000).ToString().Replace(',', '.');
        }

        private void tbThreshold_TextChanged(object sender, EventArgs e)
        {
            //// Проверяем, является ли ввод числом
            if (!Double.TryParse(tbThreshold.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out Double value) || value < (double)0)
            {
                //// Если не число или вне диапазона, удаляем последний символ
                MessageBox.Show("Введите число не меньше 0.", "Неверный ввод", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbThreshold.Text = ((double)tBarThreshold.Value / 1000).ToString().Replace(',', '.'); // Очищаем текстовое поле
                return;
            }
            GetMaxPoints();

            _settings.Threshold = tbThreshold.Text;
            _jsonManager.Write(_settings);
        }
        #endregion

        private void GetMaxPoints()
        {
            if (SmoothedMeasurments == null)
                return;

            var treshold = Convert.ToDouble(tbThreshold.Text, CultureInfo.InvariantCulture);
            var maxPoints = GetAbsorptionPeaks(SmoothedMeasurments, treshold).ToList();

            var maxPointsRound = maxPoints.Select(x => new { Wavelength = Math.Round(x.Wavelength) }).ToList();

            dataGridView3.DataSource = maxPointsRound;
            dataGridView3.ClearSelection();

            AddVerticalLine(chart1, maxPointsRound.Select(x => x.Wavelength).ToList(), Color.Indigo);
            AddVerticalLine(chart2, maxPointsRound.Select(x => x.Wavelength).ToList(), Color.Indigo);
        }
        private List<(double Wavelength, double Abs)> GetAbsorptionPeaks(List<(double Wavelength, double Abs)> measurements, double threshold = 0.1)
        {
            var peaks = new List<(double Wavelength, double Abs)>();

            // Ищем пики по соседним точкам
            for (int i = 1; i < measurements.Count - 1; i++)
            {
                double prevAbs = measurements[i - 1].Abs;
                double currentAbs = measurements[i].Abs;
                double nextAbs = measurements[i + 1].Abs;

                // Проверяем, что текущая точка больше соседей (локальный максимум)
                if (currentAbs > prevAbs && currentAbs > nextAbs)
                {
                    // Если максимум превышает порог, добавляем его в список пиков
                    if (currentAbs > threshold)
                    {
                        peaks.Add(measurements[i]);
                    }
                }
            }

            return peaks;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ChangeChartInterval(chart1, Convert.ToInt32(numericUpDown1.Value));
            ChangeChartInterval(chart2, Convert.ToInt32(numericUpDown1.Value));

            _settings.ChartInterval = Convert.ToInt32(numericUpDown1.Value);
            _jsonManager.Write(_settings);
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            // Проверяем, что была выбрана хотя бы одна строка
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Получаем значение из столбца "Wavelength" первой выделенной строки
                object heyValue = dataGridView2.SelectedRows[0].Cells["Wavelength"].Value;

                try
                {
                    // Проверяем, что значение является числовым
                    if (double.TryParse(heyValue?.ToString(), out double wavelength))
                    {
                        // Добавляем вертикальные линии на графики
                        AddVerticalLine(chart1, new List<double> { wavelength }, Color.Red);
                        AddVerticalLine(chart2, new List<double> { wavelength }, Color.Red);
                    }
                }
                catch (Exception)
                {
                    // Игнорируем ошибки
                }
            }
        }

        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            // Проверяем, что была выбрана хотя бы одна строка
            if (dataGridView3.SelectedCells.Count > 0)
            {
                // Получаем значение из столбца "Wavelength" первой выделенной строки
                object heyValue = dataGridView3.SelectedCells[0].Value;

                try
                {
                    // Проверяем, что значение является числовым
                    if (double.TryParse(heyValue?.ToString(), out double wavelength))
                    {
                        // Добавляем вертикальные линии на графики
                        AddVerticalLine(chart1, new List<double> { wavelength }, Color.Blue);
                        AddVerticalLine(chart2, new List<double> { wavelength }, Color.Blue);
                    }
                }
                catch (Exception)
                {
                    // Игнорируем ошибки
                }
            }
        }

        List<string> _selectedMaxPoints = new List<string>();
        private void AddMaxPoints(string maxPoints)
        {
            var maxPointsRound = Math.Round(Convert.ToDouble(maxPoints.Replace(".", ","))).ToString();

            if (_selectedMaxPoints.Contains(maxPointsRound))
                return;

            _selectedMaxPoints.Add(maxPointsRound);

            dgvSelected.DataSource = _selectedMaxPoints.Select(x => new { Wavelength = x }).Distinct().ToList();
        }

        private void btAddAllMaxPoints_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                AddMaxPoints(row.Cells[0].Value.ToString());
            }
        }

        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что была выбрана ячейка, а не заголовок или пустое пространство
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Получаем значение ячейки
                object cellValue = dataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                AddMaxPoints(cellValue.ToString());
            }
        }

        private void dataGridView3_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, что была нажата клавиша Enter
            if (e.KeyCode == Keys.Enter)
            {
                // Получаем текущую выбранную ячейку
                object cellValue = dataGridView3.SelectedCells[0].Value;

                AddMaxPoints(cellValue.ToString());

                // Отменяем стандартное поведение при нажатии Enter
                e.Handled = true;
            }
        }
        private void dataGridView2_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, что была нажата клавиша Enter
            if (e.KeyCode == Keys.Enter)
            {
                // Получаем текущую выбранную ячейку
                object cellValue = dataGridView2.SelectedCells[0].Value;

                // Вызываем метод AddMaxPoints, передавая значение ячейки
                AddMaxPoints(cellValue.ToString());

                // Отменяем стандартное поведение при нажатии Enter
                e.Handled = true;
            }
        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Проверяем, что была выбрана хотя бы одна строка
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Получаем значение из столбца "Wavelength" первой выделенной строки
                object heyValue = dataGridView2.SelectedRows[0].Cells["Wavelength"].Value;

                // Вызываем метод AddMaxPoints, передавая значение ячейки
                AddMaxPoints(heyValue.ToString());

            }
        }

        private void btClearSelectedRows_Click(object sender, EventArgs e)
        {
            ClearSelectionRows();
        }

        private void ClearSelectionRows()
        {
            _selectedMaxPoints.Clear();

            dgvSelected.DataSource = _selectedMaxPoints.Select(x => new { Wavelength = Math.Round(Convert.ToDouble(x.Replace(".", ","))) }).Distinct().ToList();


            AddVerticalLine(chart1, null, Color.YellowGreen);
            AddVerticalLine(chart2, null, Color.YellowGreen);

            AddVerticalLine(chart1, null, Color.DarkGreen);
            AddVerticalLine(chart2, null, Color.DarkGreen);
        }

        private void ClearSingleRows(string maxPoints)
        {
            _selectedMaxPoints.Remove(maxPoints);

            dgvSelected.DataSource = _selectedMaxPoints.Select(x => new { Wavelength = Math.Round(Convert.ToDouble(x.Replace(".", ","))) }).Distinct().ToList();
        }

        private void dgvSelected_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            AddVerticalLine(chart1, _selectedMaxPoints.Select(x => Convert.ToDouble(x)).ToList(), Color.YellowGreen);
            AddVerticalLine(chart2, _selectedMaxPoints.Select(x => Convert.ToDouble(x)).ToList(), Color.YellowGreen);
        }

        private void dgvSelected_SelectionChanged(object sender, EventArgs e)
        {
            // Проверяем, что была выбрана хотя бы одна строка
            if (dgvSelected.SelectedCells.Count > 0)
            {
                // Получаем значение из столбца "Wavelength" первой выделенной строки
                object heyValue = dgvSelected.SelectedCells[0].Value;

                try
                {
                    // Проверяем, что значение является числовым
                    if (double.TryParse(heyValue?.ToString(), out double wavelength))
                    {
                        // Добавляем вертикальные линии на графики
                        AddVerticalLine(chart1, new List<double> { wavelength }, Color.DarkGreen);
                        AddVerticalLine(chart2, new List<double> { wavelength }, Color.DarkGreen);
                    }
                }
                catch (Exception)
                {
                    // Игнорируем ошибки
                }
            }
        }

        private void dgvSelected_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что была выбрана ячейка, а не заголовок или пустое пространство
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Получаем значение ячейки
                object cellValue = dgvSelected.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                ClearSingleRows(cellValue.ToString());
            }
        }

        private async void btSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Запускаем SaveDataForMl и SaveData параллельно
                var task1 = Task.Run(() => SaveDataForMl(SelectedSubstanceData, _selectedMaxPoints));
                var task2 = SaveData();

                // Ждем завершения обеих задач
                await Task.WhenAll(task1, task2);
            }
            catch (Exception)
            {
            }
        }

        private readonly object _saveLock = new object();

        private async void SaveDataForMl(SubstanceData substanceData, List<string> selectedMaxPoints) 
        {
            lock (_saveLock)
            {
                try
                {
                    if (selectedMaxPoints.Count == 0 || selectedMaxPoints.Count == 0) return;

                    var csvWriterForMl = new CsvWriterForMl("data_for_ml.csv");

                    var time = DateTime.Now;
                    foreach ((double Wavelength, double Abs) item in substanceData.Measurements)
                    {
                        bool isMaxWavelength = false;

                        string roundWave = Math.Round(item.Wavelength).ToString();

                        if (selectedMaxPoints.Contains(roundWave))
                            isMaxWavelength = true;

                        var data = new OutputModelForMl()
                        {
                            TimeStamp = time,
                            ObjectName = SelectedSubstanceData.Name,
                            Abs = item.Abs,
                            Wavelength = item.Wavelength,
                            IsMaxWavelength = isMaxWavelength,
                        };

                        csvWriterForMl.SaveModel(data);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private async Task SaveData()
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения");
                return;
            }
            var objectName = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();


            if (dgvSelected.Rows.Count == 0)
            {
                MessageBox.Show("Не выбраны точки максимумов");
                return;
            }

            var time = DateTime.Now;
            foreach (var maxPoint in _selectedMaxPoints)
            {
                // Сохранение новой модели
                var model = new OutputModel
                {
                    TimeStamp = time,
                    FileName = Path.GetFileName(PathFile),
                    ObjectName = objectName,
                    Wavelength = Convert.ToInt32(maxPoint)
                };
                _csvWriter.SaveModel(model);
            }

            // Чтение моделей из файла
            var models = _csvWriter.ReadModels();

            dgvCsvResult.DataSource = models;
        }

        private void dgvCsvResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что текущая ячейка содержит значение типа DateTime
            if (e.ColumnIndex == 0 && e.Value is DateTime)
            {
                // Преобразуем значение ячейки в строку в нужном формате
                e.Value = ((DateTime)e.Value).ToString("yyyy.MM.dd HH:mm:ss");
                // Указываем, что значение ячейки было изменено
                e.FormattingApplied = true;
            }
        }

        private void btShowFile_Click(object sender, EventArgs e)
        {
            string filePath = @"data.csv";

            // Открываем папку, содержащую файл
            System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath);
        }
    }
}
