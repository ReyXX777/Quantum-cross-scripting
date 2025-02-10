using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DetectionController : ControllerBase
    {
        private static readonly string XssPattern = @"<[^>]*script[^>]*>|<[^>]*on\w+=|javascript:|data:text/|<[^>]+style\s*=\s*['""][^'""]*expression\s*\([^'""]*\)[^'""]*['""]";

        private readonly ILogger<DetectionController> _logger;

        public DetectionController(ILogger<DetectionController> logger)
        {
            _logger = logger;
        }

        [HttpPost("detect")]
        public IActionResult Detect([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            bool isMalicious = DetectXss(model.InputData);

            _logger.LogInformation($"XSS detection attempt for input | Malicious: {isMalicious}");

            if (isMalicious)
            {
                return BadRequest("Potential XSS attack detected.");
            }

            return Ok(new { IsMalicious = isMalicious });
        }

        [HttpPost("sanitize")]
        public IActionResult Sanitize([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            string sanitizedData = SanitizeInput(model.InputData);

            return Ok(new { SanitizedData = sanitizedData });
        }

        [HttpPost("analyze")]
        public IActionResult Analyze([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            var analysisResult = AnalyzeInput(model.InputData);

            return Ok(new { AnalysisResult = analysisResult });
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            bool isValid = ValidateInput(model.InputData);

            return Ok(new { IsValid = isValid });
        }

        [HttpPost("log")]
        public IActionResult Log([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            _logger.LogInformation($"Input data logged securely: {model.InputData}");

            return Ok(new { Message = "Input data logged successfully." });
        }

        [HttpPost("batch-detect")]
        public IActionResult BatchDetect([FromBody] List<DetectionModel> models)
        {
            if (models == null || !models.Any())
            {
                return BadRequest("Input data is required.");
            }

            var results = models.Select(m => new { Input = m.InputData, IsMalicious = DetectXss(m.InputData) }).ToList();

            return Ok(new { BatchResults = results });
        }

        [HttpPost("export-analysis")]
        public IActionResult ExportAnalysis([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            var analysisResult = AnalyzeInput(model.InputData);
            var jsonResult = JsonSerializer.Serialize(analysisResult);

            return Ok(new { ExportedData = jsonResult });
        }

        [HttpPost("custom-pattern")]
        public IActionResult CustomPatternDetection([FromBody] CustomPatternModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData) || string.IsNullOrWhiteSpace(model.CustomPattern))
            {
                return BadRequest("Input data and custom pattern are required.");
            }

            bool isMalicious = Regex.IsMatch(model.InputData, model.CustomPattern, RegexOptions.IgnoreCase);

            return Ok(new { IsMalicious = isMalicious });
        }

        [HttpPost("performance-test")]
        public IActionResult PerformanceTest([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            var startTime = DateTime.UtcNow;
            var isMalicious = DetectXss(model.InputData);
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            return Ok(new { IsMalicious = isMalicious, DurationMilliseconds = duration.TotalMilliseconds });
        }

        [HttpPost("history")]
        public IActionResult History([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            _logger.LogInformation($"Historical input data logged: {model.InputData}");

            return Ok(new { Message = "Historical input data logged successfully." });
        }

        private bool DetectXss(string input)
        {
            var regex = new Regex(XssPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }

        private string SanitizeInput(string input)
        {
            var sanitizedInput = Regex.Replace(input, XssPattern, string.Empty, RegexOptions.IgnoreCase);
            return sanitizedInput;
        }

        private AnalysisResult AnalyzeInput(string input)
        {
            var regex = new Regex(XssPattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(input);

            var analysisResult = new AnalysisResult
            {
                TotalMatches = matches.Count,
                Matches = matches.Select(m => new MatchDetails { Value = m.Value, Index = m.Index }).ToList()
            };

            return analysisResult;
        }

        private bool ValidateInput(string input)
        {
            var allowedPattern = @"^[a-zA-Z0-9\s.,!?@#\$%\^&\*\(\)\-_\+=\[\]\{\}\|\\;:'""<>\?\/`~]*$";
            var regex = new Regex(allowedPattern);
            return regex.IsMatch(input);
        }
    }

    public class DetectionModel
    {
        public string InputData { get; set; }
    }

    public class CustomPatternModel
    {
        public string InputData { get; set; }
        public string CustomPattern { get; set; }
    }

    public class AnalysisResult
    {
        public int TotalMatches { get; set; }
        public List<MatchDetails> Matches { get; set; }
    }

    public class MatchDetails
    {
        public string Value { get; set; }
        public int Index { get; set; }
    }
}

