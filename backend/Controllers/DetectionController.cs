using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic; // For storing analysis results
using System.Text.Json; // For JSON serialization

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
            var allowedPattern = @"^[a-zA-Z0-9\s.,!?@#\$%\^&\*\(\)\-_\+=\[\]\{\}\|\\;:'""<>\?\/`~]*$"; // Example
            var regex = new Regex(allowedPattern);
            return regex.IsMatch(input);
        }
    }

    public class DetectionModel
    {
        public string InputData { get; set; }
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

