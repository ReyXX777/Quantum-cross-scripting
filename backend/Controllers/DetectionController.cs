using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DetectionController : ControllerBase
    {
        // XSS detection regex pattern (basic example, refine as needed)
        private static readonly string XssPattern = @"<[^>]*script[^>]*>|<[^>]*on\w+=|javascript:|data:text/";

        [HttpPost("detect")]
        public IActionResult Detect([FromBody] DetectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.InputData))
            {
                return BadRequest("Input data is required.");
            }

            // Perform basic XSS detection using regular expression
            bool isMalicious = DetectXss(model.InputData);

            return Ok(new { IsMalicious = isMalicious });
        }

        private bool DetectXss(string input)
        {
            // Check if the input contains any malicious XSS patterns
            var regex = new Regex(XssPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }
    }

    public class DetectionModel
    {
        public string InputData { get; set; }
    }
}
