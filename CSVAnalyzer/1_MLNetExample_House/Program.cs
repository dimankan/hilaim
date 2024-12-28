using System; // Подключаем основную библиотеку для работы с консольным приложением
using Microsoft.ML; // Подключаем библиотеку ML.NET для работы с машинным обучением
using Microsoft.ML.Data; // Подключаем дополнительные компоненты ML.NET для работы с данными

namespace MLNetExample // Объявляем пространство имён для программы
{
    class Program // Главный класс программы
    {
        // Описание структуры данных для обучения
        public class HouseData
        {
            public float Size { get; set; } // Поле для площади дома (входные данные)
            public float Price { get; set; } // Поле для цены дома (целевая переменная)
        }

        // Описание структуры данных для результата прогнозирования
        public class PricePrediction
        {
            [ColumnName("Score")] // Указываем, что это поле содержит результат прогноза
            public float Price { get; set; } // Поле для прогнозируемой цены дома
        }

        static void Main(string[] args) // Точка входа в программу
        {
            // Шаг 1: Создаём контекст для работы с ML.NET
            // MLContext — это главный объект, который управляет процессом обучения и прогнозирования
            var mlContext = new MLContext();

            // Шаг 2: Подготавливаем данные для обучения
            // Создаём массив с примерными данными: площадь дома и его цена
            var trainingData = new[]
            {
                new HouseData { Size = 50, Price = 150 }, // Дом площадью 50 м² стоит 150 тыс. у.е.
                new HouseData { Size = 60, Price = 200 }, // Дом площадью 60 м² стоит 200 тыс. у.е.
                new HouseData { Size = 60, Price = 200 }, // Дом площадью 60 м² стоит 200 тыс. у.е.
                new HouseData { Size = 70, Price = 250 }  // Дом площадью 70 м² стоит 250 тыс. у.е.
            };

            // Загружаем эти данные в формат, понятный ML.NET
            var trainData = mlContext.Data.LoadFromEnumerable(trainingData);

            // Шаг 3: Создаём конвейер обработки данных и обучения модели
            // Конвейер определяет последовательность операций для подготовки данных и обучения
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "Size" }) // Объединяем колонку "Size" в новую колонку "Features"
                .Append(mlContext.Regression.Trainers.Sdca( // Добавляем алгоритм обучения SDCA для задачи регрессии
                    labelColumnName: "Price", // Указываем, что целевая переменная — это колонка "Price"
                    maximumNumberOfIterations: 100)); // Ограничиваем количество итераций обучения до 100

            // Обучаем модель на наших данных
            var model = pipeline.Fit(trainData); // Fit обучает модель на тренировочных данных

            // Шаг 4: Используем модель для прогнозирования
            // Создаём PredictionEngine, чтобы использовать обученную модель
            var predictionEngine = mlContext.Model.CreatePredictionEngine<HouseData, PricePrediction>(model);

            // Подготавливаем данные для прогноза: дом площадью 65 м²
            var sizeToPredict = new HouseData { Size = 65 };

            // Получаем прогноз с использованием PredictionEngine
            var prediction = predictionEngine.Predict(sizeToPredict);

            // Шаг 5: Выводим результат
            // Выводим прогнозируемую цену дома в консоль
            Console.WriteLine($"Прогнозируемая цена для дома площадью {sizeToPredict.Size} м²: {prediction.Price} тыс. у.е.");
        }
    }
}
