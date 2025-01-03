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

namespace Winforms_Chart_Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            PopulateChart(chart1);
        }
        public void PopulateChart(Chart chart)
        {
            // Очистим график, если есть данные
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Annotations.Clear();

            // Добавим область графика
            var chartArea = new ChartArea("MainArea");
            chart.ChartAreas.Add(chartArea);

            // Создадим серию для данных
            var series = new Series("Absorption")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Blue
            };

            // Сгенерируем данные для графика
            Random random = new Random();
            int maxIndex = 0;
            double maxAbsorption = double.MinValue;

            for (int i = 0; i <= 100; i++)
            {
                double absorption = Math.Sin(i * Math.PI / 50) + random.NextDouble() * 0.2; // Пример данных
                series.Points.AddXY(i, absorption);

                if (absorption > maxAbsorption)
                {
                    maxAbsorption = absorption;
                    maxIndex = i;
                }
            }

            chart.Series.Add(series);

            // Добавим точку для обозначения максимума
            var maxPointSeries = new Series("MaxPoint")
            {
                ChartType = SeriesChartType.Point,
                Color = Color.Red,
                MarkerSize = 10
            };
            maxPointSeries.Points.AddXY(maxIndex, maxAbsorption);
            chart.Series.Add(maxPointSeries);

            // Добавим текстовое описание для максимума
            var maxLabel = new TextAnnotation
            {
                Text = $"Max Absorption\nX: {maxIndex}\nY: {maxAbsorption:F2}",
                ForeColor = Color.Red,
                Font = new Font("Arial", 10, FontStyle.Bold),
                AnchorX = maxIndex,
                AnchorY = maxAbsorption,
                Alignment = ContentAlignment.TopLeft
            };
            chart.Annotations.Add(maxLabel);

            // Настроим оси
            chartArea.AxisX.Title = "Wavelength (nm)";
            chartArea.AxisY.Title = "Absorption";
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart1.Annotations.Clear();
            var x = new VerticalLineAnnotation();
            ChartArea chartArea4 = new ChartArea();
            x.AxisXName = "ChartArea1\\rX";
            x.Height = 121D;
            x.LineColor = Color.Green;
            x.X = 4; x.Y = 0;
            chart1.Annotations.Add(x);
            //chartArea4.Name = "ChartArea4";
            //this.chart1.ChartAreas.Add(chartArea4);
            //verticalLineAnnotation1.AxisXName = "ChartArea1\\rX";
            //verticalLineAnnotation1.Height = 121D;
            //verticalLineAnnotation1.LineColor = System.Drawing.Color.Red;
            //verticalLineAnnotation1.Name = "VerticalLineAnnotation2";
            //verticalLineAnnotation1.Width = 1D;
            //verticalLineAnnotation1.X = 3D;
            //verticalLineAnnotation1.Y = 1D;
            //  this.chart1.Annotations.Add(verticalLineAnnotation2);

        }
    }
}
