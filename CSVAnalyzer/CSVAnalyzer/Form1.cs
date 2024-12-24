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

                DisplayPoint(790, 1, "ХУЙ");
           }
        }

     

        private void DisplayChart(SubstanceData substanceData)
        {

            // Очистим старые серии, если они есть
            chart1.Series.Clear();

            // Создадим новую серию для отображения данных
            var series = new Series
            {
                Name = "Absorption Spectrum",
                IsVisibleInLegend = false,
                ChartType = SeriesChartType.Line,  // Тип графика (линия)
                BorderWidth = 2
            };

            // Добавляем данные из Measurements в серию
            foreach (var measurement in substanceData.Measurements)
            {
                // measurement.Wavelength - X, measurement.Abs - Y
                series.Points.AddXY(measurement.Wavelength, measurement.Abs);
            }

            // Добавляем серию на график
            chart1.Series.Add(series);

            // Настроим оси графика
            chart1.ChartAreas[0].AxisX.Title = "Wavelength (nm)";
            chart1.ChartAreas[0].AxisY.Title = "Absorption (AU)";
        }

        private void DisplayPoint(double x, double y, string comment)
        {
            // Очищаем старые аннотации, если они есть
            chart1.Annotations.Clear();

            // Создаем серию для отображения одной точки
            var series = new Series
            {
                Name = "Point",
                IsVisibleInLegend = false,
                ChartType = SeriesChartType.Point, // Тип графика (точка)
                MarkerSize = 8,
                MarkerStyle = MarkerStyle.Star6,
                MarkerColor = Color.Red // Цвет точки
            };

            // Добавляем точку на график
            series.Points.AddXY(x, y);

            // Добавляем серию на график
            chart1.Series.Add(series);

            // Создаем аннотацию с комментарием
            var annotation = new TextAnnotation
            {
                Text = comment, // Текст комментария
                X = x, // Позиция X
                Y = y, // Позиция Y
                ForeColor = Color.Black, // Цвет текста
                Font = new Font("Arial", 8, FontStyle.Regular),
                Alignment = ContentAlignment.MiddleLeft // Размещение текста
            };

            // Добавляем аннотацию на график
            chart1.Annotations.Add(annotation);

            // Настроим оси графика, если необходимо
            chart1.ChartAreas[0].AxisX.Title = "Wavelength (nm)";
            chart1.ChartAreas[0].AxisY.Title = "Absorption (AU)";
        }

    }
}
