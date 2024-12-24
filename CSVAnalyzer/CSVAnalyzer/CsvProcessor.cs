using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
    }
}
