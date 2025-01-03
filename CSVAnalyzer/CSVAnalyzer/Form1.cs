using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CSVAnalyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            tBarSmooth.Value = 21;
            tbSmooth.Text = "21";

            tbThreshold.Text = "0.05";
            tBarThreshold.Value = 50;
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

        private void button2_Click(object sender, EventArgs e)
        {
            List<SubstanceData> dataCsv = CsvProcessor.LoadFromFile(@"D:\Языки программирования\GitHub_Hilaim\ABS_DMSO_10uM.csv");

            dataGridView1.DataSource = dataCsv;

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
                }

                DisplayChart1();
                DisplayChart2();

                // Обработка и отображение данных на графике

                GetMaxPoints();
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
            chart.ChartAreas[0].AxisX.Interval = 50; // Интервал между метками по оси X

            // Дополнительные настройки оси X (например, начало и конец)
            chart.ChartAreas[0].AxisX.Minimum = xAxisMin ?? measurements.Min(m => m.Wavelength);
            chart.ChartAreas[0].AxisX.Maximum = xAxisMax ?? measurements.Max(m => m.Wavelength);
        }

        private void AddVerticalLine(Chart chart, List<double> xAxisValueCollection)
        {

            chart.Annotations.Clear();

            foreach (var xAxisValue in xAxisValueCollection)
            {
                var lineAnnotation = new VerticalLineAnnotation();
                lineAnnotation.AxisXName = "ChartArea1\\rX";
                lineAnnotation.Height = 121D;
                lineAnnotation.LineColor = Color.Green;
                lineAnnotation.X = xAxisValue; lineAnnotation.Y = 0;

                // Добавляем аннотацию на график
                chart.Annotations.Add(lineAnnotation);
            }
        }


        #endregion

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
        }


        private void GetMaxPoints()
        {
            if (SmoothedMeasurments == null)
                return;

            var treshold = Convert.ToDouble(tbThreshold.Text, CultureInfo.InvariantCulture);
            var maxPoints = GetAbsorptionPeaks(SmoothedMeasurments, treshold).ToList();

            var maxPointsRound = maxPoints.Select(x => new { Wavelength = Math.Round(x.Wavelength) }).ToList();

            dataGridView3.DataSource = maxPointsRound;

            AddVerticalLine(chart1, maxPointsRound.Select(x => x.Wavelength).ToList());
            AddVerticalLine(chart2, maxPointsRound.Select(x => x.Wavelength).ToList());
        }

        private void tBarThreshold_Scroll(object sender, EventArgs e)
        {
            tbThreshold.Text = (Convert.ToDouble(tBarThreshold.Value, CultureInfo.InvariantCulture) / 1000).ToString().Replace(',', '.');
        }

        private void tbThreshold_TextChanged(object sender, EventArgs e)
        {
            //var valuePoint = 
            //// Проверяем, является ли ввод числом
            if (!Double.TryParse(tbThreshold.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out Double value) || value < (double)0)
            {
                //// Если не число или вне диапазона, удаляем последний символ
                MessageBox.Show("Введите число не меньше 0.", "Неверный ввод", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbThreshold.Text = ((double)tBarThreshold.Value / 1000).ToString().Replace(',', '.'); // Очищаем текстовое поле
                return;
            }
            GetMaxPoints();
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

    }
}
