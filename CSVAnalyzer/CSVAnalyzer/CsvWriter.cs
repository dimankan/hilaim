using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CSVAnalyzer
{
    public class OutputModel
    {
        public DateTime TimeStamp { get; set; } // Время
        public string FileName { get; set; } // Название файла
        public string ObjectName { get; set; } // Название объекта
        public int Wavelength { get; set; } // Значение объекта
    }

    public class CsvWriter
    {
        private readonly string _filePath;

        public CsvWriter(string filePath)
        {
            _filePath = filePath;

            // Если файл не существует, создаем его и записываем заголовок
            if (!File.Exists(_filePath))
            {
                using (var writer = new StreamWriter(_filePath))
                {
                    writer.WriteLine("Time,FileName,ObjectName,ObjectValue");
                }
            }
        }

        public void SaveModel(OutputModel model)
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
                             $"{model.FileName}," +
                             $"{model.ObjectName}," +
                             $"{model.Wavelength}";

            // Добавляем новую строку в начало
            existingLines.Insert(1, newLine); // На место после заголовка

            // Записываем обновленный контент обратно в файл
            File.WriteAllLines(_filePath, existingLines);
        }

        public List<OutputModel> ReadModels()
        {
            var models = new List<OutputModel>();

            // Чтение файла построчно, пропуская заголовок
            var lines = File.ReadAllLines(_filePath).Skip(1);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue; // Пропуск пустых строк

                var fields = line.Split(',');

                if (fields.Length != 4) continue; // Проверяем корректность строки

                try
                {
                    var model = new OutputModel
                    {
                        TimeStamp = DateTime.Parse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        FileName = fields[1],
                        ObjectName = fields[2],
                        Wavelength = int.Parse(fields[3])
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
