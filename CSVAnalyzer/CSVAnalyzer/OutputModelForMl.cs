using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CSVAnalyzer
{
    public class OutputModelForMl
    {
        public DateTime TimeStamp { get; set; } // Время
        public string ObjectName { get; set; } // Название объекта
        public double Wavelength { get; set; } // Значение объекта
        public double Abs { get; set; } // Значение объекта
        public bool IsMaxWavelength { get; set; }
    }

    public class CsvWriterForMl
    {
        private readonly string _filePath;

        public CsvWriterForMl(string filePath)
        {
            _filePath = filePath;

            // Если файл не существует, создаем его и записываем заголовок
            if (!File.Exists(_filePath))
            {
                using (var writer = new StreamWriter(_filePath))
                {
                    writer.WriteLine("Time,ObjectName,Wavelength,Abs,IsMaxWavelength");
                }
            }
        }

        public void SaveModel(OutputModelForMl model)
        {
            List<string> existingLines;
            try
            {
                // Чтение существующего содержимого файла
                existingLines = File.ReadAllLines(_filePath).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Если файл {_filePath} открыт, то закройте его и нажмите ОК" + Environment.NewLine + ex.Message, "Проблема с файлом");
                existingLines = File.ReadAllLines(_filePath).ToList();
            }

            // Форматируем строку для новой записи
            string newLine = $"{model.TimeStamp.ToString("o", CultureInfo.InvariantCulture)}," +
                             $"{model.ObjectName}," +
                             $"{model.Wavelength}," +
                             $"{model.Abs}," +
                             $"{model.IsMaxWavelength}";

            // Добавляем новую строку в начало
            existingLines.Insert(1, newLine); // На место после заголовка

            // Записываем обновленный контент обратно в файл
            File.WriteAllLines(_filePath, existingLines);
        }

        public List<OutputModelForMl> ReadModels()
        {
            var models = new List<OutputModelForMl>();

            // Чтение файла построчно, пропуская заголовок
            var lines = File.ReadAllLines(_filePath).Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue; // Пропуск пустых строк

                var fields = line.Split(',');

                if (fields.Length != 5) continue; // Проверяем корректность строки

                try
                {
                    var model = new OutputModelForMl
                    {
                        TimeStamp = DateTime.Parse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ObjectName = fields[1],
                        Wavelength = double.Parse(fields[2], CultureInfo.InvariantCulture),
                        Abs = double.Parse(fields[3], CultureInfo.InvariantCulture),
                        IsMaxWavelength = bool.Parse(fields[4])
                    };

                    models.Add(model);
                }
                catch
                {
                    // Игнорируем строки, которые не удалось разобрать
                    continue;
                }
            }

            return models;
        }
    }

}
