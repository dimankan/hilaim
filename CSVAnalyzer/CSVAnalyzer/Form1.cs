using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CSVAnalyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
                SubstanceData selectedSubstance = (SubstanceData)selectedRow.DataBoundItem;

                if (selectedSubstance != null)
                {
                    // Преобразуем Measurements для отображения в DataGridView2
                    var measurements = selectedSubstance.Measurements
                        .Select(m => new { Wavelength = m.Wavelength, Abs = m.Abs })
                        .ToList();

                    // Привязываем данные ко второму DataGridView
                    dataGridView2.DataSource = measurements;
                }

                DisplayChart(selectedSubstance);
                // Обработка и отображение данных на графике

                var maxPoints = GetAbsorptionPeaks(selectedSubstance.Measurements,0.05).ToList();

                dataGridView3.DataSource = maxPoints.Select(x=>new { Wavelength = x.Wavelength , Abs = x.Abs}).ToList();
            }
        }



        private void DisplayChart(SubstanceData substanceData)
        {
            chart1.Series.Clear();

            // Если серия не найдена, создаём новую
            var series = new Series
            {
                Name = "Absorption Spectrum",
                ChartType = SeriesChartType.Line,  // Тип графика (линия)
                BorderWidth = 4,
                IsVisibleInLegend = false,
            };

            // Добавляем данные из Measurements в серию
            foreach (var measurement in substanceData.Measurements)
            {
                series.Points.AddXY(measurement.Wavelength, measurement.Abs);
            }

            // Добавляем серию на график
            chart1.Series.Add(series);

            // Настроим оси графика (если нужно изменить их титулы)
            chart1.ChartAreas[0].AxisX.Title = "Wavelength (nm)";
            chart1.ChartAreas[0].AxisY.Title = "Absorption (AU)";

            // Настройка частоты отображения меток на осях
            chart1.ChartAreas[0].AxisX.Interval = 50; // Интервал между метками по оси X
            //chart1.ChartAreas[0].AxisY.Interval = 0.1; // Интервал между метками по оси Y

            // Дополнительные настройки оси X (например, начало и конец)
            chart1.ChartAreas[0].AxisX.Minimum = substanceData.Measurements.Min(m => m.Wavelength);
            chart1.ChartAreas[0].AxisX.Maximum = substanceData.Measurements.Max(m => m.Wavelength);

            //// Дополнительные настройки оси Y
            //chart1.ChartAreas[0].AxisY.Minimum = substanceData.Measurements.Min(m => m.Abs) - 0.1;
            //chart1.ChartAreas[0].AxisY.Maximum = substanceData.Measurements.Max(m => m.Abs) + 0.1;
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
