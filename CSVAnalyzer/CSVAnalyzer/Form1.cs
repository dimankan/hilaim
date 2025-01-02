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

        public SubstanceData SelectedSubstanceData { get; set; }
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


                DisplayChart(chart1, SelectedSubstanceData.Measurements);

                var smoothedMeasurments = SubstanceData.Smooth(SelectedSubstanceData.Measurements, 14);
                DisplayChart(chart2, smoothedMeasurments, SelectedSubstanceData.Measurements[SelectedSubstanceData.Measurements.Count-1].Wavelength  );
                // Обработка и отображение данных на графике

                var maxPoints = GetAbsorptionPeaks(SelectedSubstanceData.Measurements, 0.05).ToList();

                dataGridView3.DataSource = maxPoints.Select(x => new {  x.Wavelength, x.Abs }).ToList();
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
            chart.ChartAreas[0].AxisX.Maximum = xAxisMax ??  measurements.Max(m => m.Wavelength);
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
