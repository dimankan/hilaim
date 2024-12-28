using System;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace SentimentAnalysisExample
{
    class Program
    {
        // Входные данные (отзывы) для обучения модели
        public class SentimentData
        {
            // Текст отзыва
            public string Text { get; set; }

            // Метка (Label): 
            // true — положительный отзыв, false — отрицательный
            public bool Label { get; set; }
        }

        // Прогнозируемые данные
        public class SentimentPrediction
        {
            // Атрибут [ColumnName("PredictedLabel")]:
            // Указывает, что этот столбец будет содержать прогнозируемый класс (true/false)
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }

            // Вероятность того, что отзыв положительный
            public float Probability { get; set; }

            // Оценка модели (Score):
            // Внутренний показатель модели, показывающий уверенность в предсказании
            public float Score { get; set; }
        }

        static void Main(string[] args)
        {
            // Шаг 1: Создание контекста машинного обучения (MLContext)
            // MLContext — это базовый объект для работы с ML.NET.
            // Он управляет всеми операциями, такими как загрузка данных, обучение моделей, прогнозирование и т. д.
            var mlContext = new MLContext();

            // Шаг 2: Подготовка тренировочных данных
            // Данные для обучения: массив объектов SentimentData
            // Text — это текст отзыва, Label — истинная метка отзыва.
            var trainingData = new[]
             {
                new SentimentData { Text = "Это потрясающе!", Label = true },
                new SentimentData { Text = "Мне это не понравилось.", Label = false },
                new SentimentData { Text = "Просто великолепно!", Label = true },
                new SentimentData { Text = "Это худшее, что я видел.", Label = false },
                new SentimentData { Text = "Мне очень понравился этот продукт!", Label = true },
                new SentimentData { Text = "Отвратительное качество, не рекомендую.", Label = false },
                new SentimentData { Text = "Было лучше, чем я ожидал!", Label = true },
                new SentimentData { Text = "Разочарован. Деньги на ветер.", Label = false },
                new SentimentData { Text = "Очень доволен покупкой, работает прекрасно.", Label = true },
                new SentimentData { Text = "Никогда не куплю это снова.", Label = false },
                new SentimentData { Text = "Прекрасное соотношение цены и качества!", Label = true },
                new SentimentData { Text = "Совершенно бесполезная вещь.", Label = false },
                new SentimentData { Text = "Превзошло все мои ожидания!", Label = true },
                new SentimentData { Text = "Не соответствует описанию, я в ярости.", Label = false },
                new SentimentData { Text = "Очень хороший продукт. Рекомендую.", Label = true },
                new SentimentData { Text = "Качество на нуле, не стоит своих денег.", Label = false },
                new SentimentData { Text = "Это был лучший подарок, который я получал.", Label = true },
                new SentimentData { Text = "Сломалось на следующий день.", Label = false },
                new SentimentData { Text = "Функционирует идеально, я доволен.", Label = true },
                new SentimentData { Text = "Ужасный сервис и плохое качество.", Label = false },
                new SentimentData { Text = "Мой лучший опыт покупки!", Label = true },
                new SentimentData { Text = "Отвратительное обслуживание, не рекомендую.", Label = false },
                new SentimentData { Text = "Я абсолютно счастлив с этой покупкой!", Label = true },
                new SentimentData { Text = "Это была ошибка, не покупайте.", Label = false },
                new SentimentData { Text = "Отличный продукт, выше всяких похвал.", Label = true },
                new SentimentData { Text = "Деньги потрачены зря, не стоит.", Label = false },
                new SentimentData { Text = "Лучшее, что я покупал за последнее время.", Label = true },
                new SentimentData { Text = "Это просто катастрофа, ужасно.", Label = false },
                new SentimentData { Text = "Прекрасный дизайн и отличное качество.", Label = true },
                new SentimentData { Text = "Я был шокирован таким плохим качеством.", Label = false },
                new SentimentData { Text = "Очень рад, что купил это!", Label = true },
                new SentimentData { Text = "Не оправдало никаких ожиданий.", Label = false },
                new SentimentData { Text = "Лучшее соотношение цены и качества.", Label = true },
                new SentimentData { Text = "Отвратительная упаковка, всё повреждено.", Label = false },
                new SentimentData { Text = "Очень доволен, буду советовать друзьям.", Label = true },
                new SentimentData { Text = "Не советую никому, ужасный опыт.", Label = false },
                new SentimentData { Text = "Качество на высоте, молодцы!", Label = true },
                new SentimentData { Text = "Всё сломалось после двух дней использования.", Label = false },
                new SentimentData { Text = "Было очень приятно получить это.", Label = true },
                new SentimentData { Text = "Полностью разочарован.", Label = false },
                new SentimentData { Text = "Это был лучший выбор, который я мог сделать.", Label = true },
                new SentimentData { Text = "Плохой продукт, не рекомендую.", Label = false },
                new SentimentData { Text = "Обслуживание отличное, товар тоже.", Label = true },
                new SentimentData { Text = "Всё оказалось хуже, чем я ожидал.", Label = false },
                new SentimentData { Text = "Сделано на совесть, выглядит отлично.", Label = true },
                new SentimentData { Text = "Не рекомендую, деньги на ветер.", Label = false },
                new SentimentData { Text = "Очень хорошо работает, полностью доволен.", Label = true },
                new SentimentData { Text = "Товар пришёл с дефектами.", Label = false },
                new SentimentData { Text = "Это мой любимый продукт теперь!", Label = true },
                new SentimentData { Text = "Просто ужасно, жалею о покупке.", Label = false },
                new SentimentData { Text = "Очень крутой дизайн, мне нравится.", Label = true },
                new SentimentData { Text = "Отвратительный сервис, никому не советую.", Label = false },
                new SentimentData { Text = "Этот товар стоит каждой потраченной копейки.", Label = true },
                new SentimentData { Text = "Купил, и сразу сломалось. Просто кошмар.", Label = false },
                new SentimentData { Text = "Пользуюсь уже несколько месяцев, всё отлично.", Label = true },
                new SentimentData { Text = "Никак не оправдывает цену, жаль денег.", Label = false },
                new SentimentData { Text = "Лучше, чем я мог ожидать.", Label = true },
                new SentimentData { Text = "Такого отвратительного качества я ещё не видел.", Label = false },
                new SentimentData { Text = "Очень понравилось, буду заказывать ещё.", Label = true },
                new SentimentData { Text = "Никому не рекомендую, потратите время зря.", Label = false },
                new SentimentData { Text = "Всё, что я хотел, получил в этом продукте.", Label = true },
                new SentimentData { Text = "Ужасный продукт, никогда не куплю снова.", Label = false },
                new SentimentData { Text = "Сервис на высшем уровне, всем доволен.", Label = true },
                new SentimentData { Text = "Продукт не соответствует описанию.", Label = false },
                new SentimentData { Text = "Очень рад, что выбрал это.", Label = true },
                new SentimentData { Text = "Качество оставляет желать лучшего.", Label = false },
                new SentimentData { Text = "Рекомендую всем, кто ищет что-то качественное.", Label = true },
                new SentimentData { Text = "Очень плохой опыт, больше не куплю.", Label = false },
                new SentimentData { Text = "Продукт радует каждый день.", Label = true },
                new SentimentData { Text = "Сломалось через неделю. Ужас.", Label = false },
                new SentimentData { Text = "Это лучшая покупка в моей жизни.", Label = true },
                new SentimentData { Text = "Упаковка была повреждена, товар сломан.", Label = false },
                new SentimentData { Text = "Очень хороший продукт, закажу ещё раз.", Label = true },
                new SentimentData { Text = "Качество хуже некуда.", Label = false },
                new SentimentData { Text = "Я счастлив, что нашёл это.", Label = true },
                new SentimentData { Text = "Не стоило этих денег, жалею.", Label = false },
                new SentimentData { Text = "Полностью соответствует описанию, доволен.", Label = true },
                new SentimentData { Text = "Это был ужасный опыт, не советую.", Label = false }
            };


            // Загружаем данные в формат, который понимает ML.NET
            var trainData = mlContext.Data.LoadFromEnumerable(trainingData);

            // Шаг 3: Создание конвейера обработки данных (Pipeline)
            // Конвейер включает в себя шаги:
            // 1. Преобразование текста в числовые признаки с помощью FeaturizeText.
            //    "Features" — это имя нового столбца, который будет содержать числовые признаки текста.
            //    "Text" — это имя столбца, который содержит текстовые данные.
            // 2. Добавление алгоритма обучения модели (SdcaLogisticRegression):
            //    - LabelColumnName: имя столбца с метками (Label).
            //    - FeatureColumnName: имя столбца с признаками (Features).
            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", "Text")
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));

            // Шаг 4: Обучение модели
            // Метод Fit обучает модель на тренировочных данных (trainData).
            // Результатом является объект модели (model), который готов к прогнозированию.
            var model = pipeline.Fit(trainData);

            // Шаг 5: Создание PredictionEngine для прогнозирования
            // PredictionEngine используется для прогнозирования на отдельных объектах.
            // Первый тип (SentimentData) — это тип входных данных.
            // Второй тип (SentimentPrediction) — это тип выходных данных (результатов прогноза).
            var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);

            // Подготовка тестовых данных
            // Мы создаём новый отзыв, чтобы проверить модель.
            var testInput = new SentimentData { Text = "Мне это действительно понравилось!" };

            // Прогнозирование:
            // Метод Predict принимает объект типа SentimentData и возвращает результат прогноза.
            var prediction = predictionEngine.Predict(testInput);

            // Вывод результатов прогноза
            // Prediction: прогнозируемый класс (положительный или отрицательный).
            // Probability: вероятность, с которой модель уверена в предсказании.
            Console.WriteLine($"Текст: {testInput.Text}");
            Console.WriteLine($"Прогноз: {(prediction.Prediction ? "Положительный" : "Отрицательный")}");
            Console.WriteLine($"Вероятность: {prediction.Probability:P2}");
        }
    }
}
