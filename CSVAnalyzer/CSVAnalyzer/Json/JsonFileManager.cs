using Newtonsoft.Json;
using System;
using System.IO;

namespace CSVAnalyzer.Json
{

    public class JsonFileManager<T> where T : new()
    {
        private readonly string _filePath;

        public JsonFileManager(string filePath)
        {
            _filePath = filePath;

            // Проверяем, существует ли файл, если нет — создаем его с пустым объектом
            if (!File.Exists(_filePath))
            {
                CreateDefaultFile();
            }
        }

        /// <summary>
        /// Считывает данные из JSON-файла.
        /// </summary>
        /// <returns>Десериализованный объект.</returns>
        public T Read()
        {
            try
            {
                var jsonData = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
            catch (Exception ex)
            {
                // Обработка ошибок чтения файла
                Console.WriteLine($"Ошибка при чтении JSON-файла: {ex.Message}");
                return new T();
            }
        }

        /// <summary>
        /// Записывает данные в JSON-файл.
        /// </summary>
        /// <param name="data">Объект для записи.</param>
        public void Write(T data)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(_filePath, jsonData);
            }
            catch (Exception ex)
            {
                // Обработка ошибок записи файла
                Console.WriteLine($"Ошибка при записи JSON-файла: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает JSON-файл с пустым объектом по умолчанию.
        /// </summary>
        private void CreateDefaultFile()
        {
            Write(new T());
        }
    }
}
