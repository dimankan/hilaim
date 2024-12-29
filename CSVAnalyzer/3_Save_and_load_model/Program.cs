using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

CreateAndSaveModel();
MakePrediction();

/// <summary>
/// Функция для создания, обучения и сохранения модели машинного обучения.
/// </summary>
static void CreateAndSaveModel()
{
    // Создаем массив данных для обучения модели
    HousingData[] housingData = new HousingData[]
    {
        new HousingData
        {
            Size = 600f, // Размер дома в квадратных футах
            HistoricalPrices = new float[] { 100000f, 125000f, 122000f }, // Исторические цены
            CurrentPrice = 170000f // Текущая цена (целевая переменная)
        },
        new HousingData
        {
            Size = 1000f,
            HistoricalPrices = new float[] { 200000f, 250000f, 230000f },
            CurrentPrice = 225000f
        },
        new HousingData
        {
            Size = 1000f,
            HistoricalPrices = new float[] { 126000f, 130000f, 200000f },
            CurrentPrice = 195000f
        }
    };

    // Создаем объект MLContext для управления процессом машинного обучения
    MLContext mlContext = new MLContext();

    // Загружаем данные в IDataView, который используется в ML.NET как базовый формат данных
    IDataView data = mlContext.Data.LoadFromEnumerable<HousingData>(housingData);

    // Определяем этапы подготовки данных и выбора алгоритма модели
    EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>> pipelineEstimator =
        mlContext.Transforms.Concatenate("Features", new string[] { "Size", "HistoricalPrices" }) // Объединяем признаки в один вектор
            .Append(mlContext.Transforms.NormalizeMinMax("Features")) // Нормализуем значения признаков
            .Append(mlContext.Regression.Trainers.Sdca()); // Используем алгоритм SDCA для регрессии

    // Обучаем модель на основе подготовленного пайплайна
    ITransformer trainedModel = pipelineEstimator.Fit(data);

    // Сохраняем обученную модель в файл
    mlContext.Model.Save(trainedModel, data.Schema, "model.zip");
}

/// <summary>
/// Функция для демонстрации предсказания на основе загруженной модели.
/// </summary>
static void MakePrediction()
{
    // Создаем новый MLContext для работы с предсказаниями
    MLContext mlContext = new MLContext();

    // Загружаем ранее сохраненную модель
    ITransformer trainedModel = mlContext.Model.Load("model.zip", out DataViewSchema modelSchema);

    // Создаем PredictionEngine для выполнения предсказаний
    var predictionEngine = mlContext.Model.CreatePredictionEngine<HousingData, HousingPrediction>(trainedModel);

    // Создаем пример данных для предсказания
    HousingData newHouse = new HousingData
    {
        Size = 800f, // Размер дома в квадратных футах
        HistoricalPrices = new float[] { 150000f, 160000f, 155000f } // Исторические цены
    };

    // Выполняем предсказание цены дома
    HousingPrediction prediction = predictionEngine.Predict(newHouse);

    // Выводим результат на экран
    System.Console.WriteLine($"Предсказанная цена для дома размером {newHouse.Size} кв. футов: {prediction.PredictedPrice:C}");
}

/// <summary>
/// Класс для представления данных о доме.
/// </summary>
public class HousingData
{
    // Размер дома в квадратных футах
    [LoadColumn(0)]
    public float Size { get; set; }

    // Исторические цены на дом
    [LoadColumn(1, 3)]
    [VectorType(3)]
    public float[] HistoricalPrices { get; set; }

    // Текущая цена (целевая переменная для обучения)
    [LoadColumn(4)]
    [ColumnName("Label")]
    public float CurrentPrice { get; set; }
}

/// <summary>
/// Класс для представления предсказания цены дома.
/// </summary>
class HousingPrediction
{
    // Предсказанная цена
    [ColumnName("Score")]
    public float PredictedPrice { get; set; }
}
