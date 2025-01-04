using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CSVAnalyzer
{
    public static class CsvProcessor
    {
        public static List<SubstanceData> LoadFromFile(string filePath)
        {
            List<SubstanceData> SubstancesData = new List<SubstanceData>();

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
                throw new Exception("Invalid file format.");

            // Первая строка - названия веществ
            string[] headers = lines[0].Split(',');
            int substanceCount = 0;

            // Инициализация веществ по заголовкам
            for (int i = 0; i < headers.Length; i += 2)
            {
                if (!string.IsNullOrWhiteSpace(headers[i]))
                {
                    SubstancesData.Add(new SubstanceData { Name = headers[i].Trim() });
                    substanceCount++;
                }
            }

            // Обработка строк данных
            for (int i = 2; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    break; // Остановка на пустой строке

                string[] values = line.Split(',');

                // Проверяем, чтобы данных хватило для всех веществ
                if (values.Length < substanceCount * 2)
                    continue;

                // Итерируемся по веществам
                for (int j = 0; j < substanceCount; j++)
                {
                    // Локальные переменные для текущей пары Wavelength и Abs
                    string wavelengthRaw = values[j * 2];
                    string absRaw = values[j * 2 + 1];

                    // Используем CultureInfo.InvariantCulture для парсинга
                    if (double.TryParse(wavelengthRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out double wavelength) &&
                        double.TryParse(absRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out double abs))
                    {
                        // Если получилось, добавляем к текущему веществу
                        SubstancesData[j].Measurements.Add((wavelength, abs));
                    }
                    else
                    {
                        // Логируем или игнорируем ошибочные строки
                        Console.WriteLine($"Невозможно распознать значения: '{wavelengthRaw}' и '{absRaw}' на строке {i + 1}");
                    }
                }
            }

            return SubstancesData;
        }

    }
    public class SubstanceData
    {
        public string Name { get; set; }
        public List<(double Wavelength, double Abs)> Measurements { get; private set; } = new List<(double, double)>();

        public override string ToString()       
        {
            return Name;
        }

        /// <summary>
        /// Сглаживает данные методом скользящего среднего.
        /// </summary>
        /// <param name="data">Список точек данных (длина волны и значение).</param>
        /// <param name="windowSize">Размер окна скользящего среднего.</param>
        /// <returns>Список сглаженных точек.</returns>
        public static List<(double Wavelength, double Abs)> Smooth(
            List<(double Wavelength, double Abs)> data,
            int windowSize)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Данные не должны быть пустыми.", nameof(data));

            if (windowSize < 1)
                throw new ArgumentException("Размер окна должен быть положительным числом.", nameof(windowSize));

            // Округляем windowSize до ближайшего нечетного числа в большую сторону
            if (windowSize % 2 == 0)
                windowSize++;

            int halfWindow = windowSize / 2;
            var smoothed = new List<(double Wavelength, double Abs)>();

            for (int i = 0; i < data.Count; i++)
            {
                int start = Math.Max(0, i - halfWindow);
                int end = Math.Min(data.Count - 1, i + halfWindow);
                int count = end - start + 1;

                double avgWavelength = data.Skip(start).Take(count).Average(x => x.Wavelength);
                double avgAbs = data.Skip(start).Take(count).Average(x => x.Abs);

                smoothed.Add((avgWavelength, avgAbs));
            }

            return smoothed;
        }

    }
}
