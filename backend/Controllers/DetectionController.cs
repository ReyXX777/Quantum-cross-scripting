using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

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

        private bool DetectXss(string input)
        {
            // Check if the input contains any malicious XSS patterns using the regex
            var regex = new Regex(XssPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }
    }

    public class DetectionModel
    {
        public string InputData { get; set; }
    }
}
