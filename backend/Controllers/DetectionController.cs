using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DetectionController : ControllerBase
    {
        // Improved XSS detection regex pattern (refine based on real-world cases)
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

            // Perform XSS detection using regular expression
            bool isMalicious = DetectXss(model.InputData);

            // Log the detection attempt, avoid logging sensitive information
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

            // Sanitize the input data
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

            // Analyze the input data for potential threats
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

            // Validate the input data for allowed characters and patterns
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

            // Log the input data securely
            _logger.LogInformation($"Input data logged securely: {model.InputData}");

            return Ok(new { Message = "Input data logged successfully." });
        }

        private bool DetectXss(string input)
        {
            // Check if the input contains any malicious XSS patterns using the regex
            var regex = new Regex(XssPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }

        private string SanitizeInput(string input)
        {
            // Remove potentially harmful tags and attributes
            var sanitizedInput = Regex.Replace(input, XssPattern, string.Empty, RegexOptions.IgnoreCase);
            return sanitizedInput;
        }

        private object AnalyzeInput(string input)
        {
            // Analyze the input for potential threats
            var regex = new Regex(XssPattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(input);

            return new
            {
                TotalMatches = matches.Count,
                Matches = matches.Select(m => m.Value).ToList()
            };
        }

        private bool ValidateInput(string input)
        {
            // Validate the input for allowed characters and patterns
            var allowedPattern = @"^[a-zA-Z0-9\s.,!?@#\$%\^&\*\(\)\-_\+=\[\]\{\}\|\\;:'""<>\?\/`~]*$";
            var regex = new Regex(allowedPattern);
            return regex.IsMatch(input);
        }
    }

    public class DetectionModel
    {
        public string InputData { get; set; }
    }
}
