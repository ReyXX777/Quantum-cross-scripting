using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.IO.Compression;
using QuantumCrossScripting.Data;

namespace QuantumCrossScripting.ML
{
    // Input data class for the model
    public class XssDetectionModelInput
    {
        [ColumnName("InputText")]
        public string InputText { get; set; }
    }

    // Output data class for the model
    public class XssDetectionModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool IsMalicious { get; set; }
    }

    public class PredictionEngineService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string _modelZipPath = "ML/Model/XssDetectionModel.zip"; // Path to the zipped model
        private readonly string _extractedModelPath = "ML/Model/extracted_model"; // Directory to extract the model

        // PredictionEngine should be initialized once and reused.
        private PredictionEngine<XssDetectionModelInput, XssDetectionModelOutput> _predictionEngine;

        public PredictionEngineService()
        {
            _mlContext = new MLContext();
            InitializeModel();
        }

        // Initialize the model by loading it
        private void InitializeModel()
        {
            try
            {
                // Check if the model file exists
                if (!File.Exists(_modelZipPath))
                {
                    throw new FileNotFoundException("Model file not found: " + _modelZipPath);
                }

                // Check if the model is already extracted or if we need to re-extract
                if (!Directory.Exists(_extractedModelPath) || !File.Exists(Path.Combine(_extractedModelPath, "XssDetectionModel.zip")))
                {
                    Directory.CreateDirectory(_extractedModelPath); // Ensure directory exists
                    ZipFile.ExtractToDirectory(_modelZipPath, _extractedModelPath);
                }

                // Load the model from the extracted directory
                string modelFilePath = Path.Combine(_extractedModelPath, "XssDetectionModel.zip");
                if (!File.Exists(modelFilePath))
                {
                    throw new InvalidOperationException("Model file not found in extracted directory.");
                }

                _model = _mlContext.Model.Load(modelFilePath, out var modelInputSchema);

                // Create a PredictionEngine once during initialization for reuse
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<XssDetectionModelInput, XssDetectionModelOutput>(_model);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error initializing the model.", ex);
            }
        }

        // Use the loaded model to predict if the input text is malicious (XSS)
        public bool PredictXss(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                throw new ArgumentException("Input text cannot be null or empty.");
            }

            try
            {
                // Ensure prediction engine is initialized
                if (_predictionEngine == null)
                {
                    throw new InvalidOperationException("Prediction engine not initialized.");
                }

                // Predict the result
                var prediction = _predictionEngine.Predict(new XssDetectionModelInput { InputText = inputText });
                return prediction.IsMalicious;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error during prediction.", ex);
            }
        }

        // Method to retrain the model with new data
        public void RetrainModel(XssDetectionModelInput[] trainingData)
        {
            if (trainingData == null || trainingData.Length == 0)
            {
                throw new ArgumentException("Training data cannot be null or empty.");
            }

            try
            {
                // Load the training data into an IDataView
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                // Define the pipeline for retraining
                var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(XssDetectionModelInput.InputText))
                    .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(labelColumnName: "IsMalicious", featureColumnName: "Features"));

                // Retrain the model
                var retrainedModel = pipeline.Fit(dataView);

                // Save the retrained model
                string retrainedModelPath = Path.Combine(Path.GetDirectoryName(_modelZipPath), "RetrainedModel.zip");
                _mlContext.Model.Save(retrainedModel, dataView.Schema, retrainedModelPath);

                // Update the model and prediction engine
                _model = retrainedModel;
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<XssDetectionModelInput, XssDetectionModelOutput>(_model);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error during model retraining.", ex);
            }
        }

        // Method to evaluate the model's performance
        public void EvaluateModel(XssDetectionModelInput[] testData)
        {
            if (testData == null || testData.Length == 0)
            {
                throw new ArgumentException("Test data cannot be null or empty.");
            }

            try
            {
                // Load the test data into an IDataView
                var dataView = _mlContext.Data.LoadFromEnumerable(testData);

                // Evaluate the model
                var predictions = _model.Transform(dataView);
                var metrics = _mlContext.BinaryClassification.Evaluate(predictions, "IsMalicious");

                // Log or return the evaluation metrics
                Console.WriteLine($"Accuracy: {metrics.Accuracy}");
                Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve}");
                Console.WriteLine($"F1 Score: {metrics.F1Score}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error during model evaluation.", ex);
            }
        }
    }
}
