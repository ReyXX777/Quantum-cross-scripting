using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.Linq;
using QuantumCrossScripting.Data;
using System.IO.Compression;

namespace QuantumCrossScripting.ML
{
    // Input data class
    public class XssDetectionModelInput
    {
        [ColumnName("InputText")]
        public string InputText { get; set; }
    }

    // Output data class
    public class XssDetectionModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool IsMalicious { get; set; }
    }

    public class PredictionEngine
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string _modelZipPath = "ML/Model/XssDetectionModel.zip";

        public PredictionEngine()
        {
            _mlContext = new MLContext();
            InitializeModel();
        }

        // Load and initialize the model
        private void InitializeModel()
        {
            if (!File.Exists(_modelZipPath))
            {
                throw new FileNotFoundException("Model file not found: " + _modelZipPath);
            }

            // Unzip model if it doesn't already exist as extracted
            string extractedModelPath = Path.Combine(Path.GetDirectoryName(_modelZipPath), "extracted_model");
            if (!Directory.Exists(extractedModelPath))
            {
                ZipFile.ExtractToDirectory(_modelZipPath, extractedModelPath);
            }

            // Load the model from the unzipped file
            string modelFilePath = Path.Combine(extractedModelPath, "XssDetectionModel.zip");
            _model = _mlContext.Model.Load(modelFilePath, out var modelInputSchema);
        }

        // Use the loaded model to predict if the input is XSS
        public bool PredictXss(string inputText)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<XssDetectionModelInput, XssDetectionModelOutput>(_model);
            var prediction = predictionEngine.Predict(new XssDetectionModelInput { InputText = inputText });
            return prediction.IsMalicious;
        }
    }
}
