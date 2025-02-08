using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.IO.Compression;
using QuantumCrossScripting.Data;
using Microsoft.Extensions.Logging; // Add logging
using System.Collections.Generic; // For evaluation metrics

namespace QuantumCrossScripting.ML
{
    // ... (XssDetectionModelInput and XssDetectionModelOutput remain the same)

    public class PredictionEngineService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string _modelZipPath = "ML/Model/XssDetectionModel.zip";
        private readonly string _extractedModelPath = "ML/Model/extracted_model";
        private PredictionEngine<XssDetectionModelInput, XssDetectionModelOutput> _predictionEngine;
        private readonly ILogger<PredictionEngineService> _logger; // Logger

        public PredictionEngineService(ILogger<PredictionEngineService> logger) // Inject ILogger
        {
            _mlContext = new MLContext();
            _logger = logger;
            InitializeModel();
        }

        private void InitializeModel()
        {
            try
            {
                // ... (Model loading logic remains the same)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing the model."); // Log error
                throw; // Re-throw the exception after logging
            }
        }

        public bool PredictXss(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                throw new ArgumentException("Input text cannot be null or empty.");
            }

            try
            {
                if (_predictionEngine == null)
                {
                    throw new InvalidOperationException("Prediction engine not initialized.");
                }

                var prediction = _predictionEngine.Predict(new XssDetectionModelInput { InputText = inputText });
                _logger.LogInformation($"XSS Prediction: Input='{inputText}', Result={prediction.IsMalicious}"); // Log prediction
                return prediction.IsMalicious;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during prediction."); // Log error
                throw;
            }
        }

        public void RetrainModel(XssDetectionModelInput[] trainingData)
        {
            if (trainingData == null || trainingData.Length == 0)
            {
                throw new ArgumentException("Training data cannot be null or empty.");
            }

            try
            {
                // ... (Retraining logic remains the same)
                _logger.LogInformation("Model retrained successfully."); // Log retraining
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during model retraining."); // Log error
                throw;
            }
        }

        public Dictionary<string, double> EvaluateModel(XssDetectionModelInput[] testData) // Return metrics
        {
            if (testData == null || testData.Length == 0)
            {
                throw new ArgumentException("Test data cannot be null or empty.");
            }

            try
            {
                // ... (Evaluation logic remains the same)

                var predictions = _model.Transform(dataView);
                var metrics = _mlContext.BinaryClassification.Evaluate(predictions, "IsMalicious");

                var evaluationMetrics = new Dictionary<string, double>
                {
                    { "Accuracy", metrics.Accuracy },
                    { "AUC", metrics.AreaUnderRocCurve },
                    { "F1Score", metrics.F1Score },
                    { "Precision", metrics.Precision },
                    { "Recall", metrics.Recall }
                };
                _logger.LogInformation("Model Evaluation Metrics: {@Metrics}", evaluationMetrics); // Log metrics

                return evaluationMetrics; // Return metrics
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during model evaluation."); // Log error
                throw;
            }
        }


        // New method to get schema information
        public string GetModelSchema()
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Model not initialized.");
            }

            // Get the input schema
            var inputSchema = _model.GetInputSchema();

            // Format the schema information (you can customize this)
            var schemaInfo = $"Input Schema:\n";
            foreach (var column in inputSchema)
            {
                schemaInfo += $"{column.Name}: {column.Type}\n";
            }

            return schemaInfo;
        }

        // New method to get feature importance (if applicable)
        public string GetFeatureImportance()
        {
            if (_model is IPredictorWithFeatureWeights featureWeightsPredictor)
            {
                var featureWeights = featureWeightsPredictor.GetFeatureWeights();

                // Format feature importance information (you can customize this)
                var featureImportanceInfo = "Feature Importance:\n";
                for (int i = 0; i < featureWeights.Length; i++)
                {
                    featureImportanceInfo += $"Feature {i + 1}: {featureWeights[i]}\n";
                }
                return featureImportanceInfo;
            }
            else
            {
                return "Feature importance is not available for this model type.";
            }
        }
    }
}

