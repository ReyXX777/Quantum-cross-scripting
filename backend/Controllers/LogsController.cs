using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly string _logFilePath;
        private readonly ILogger<LogsController> _logger;

        // Constructor to inject IConfiguration and ILogger
        public LogsController(IConfiguration configuration, ILogger<LogsController> logger)
        {
            _logger = logger;
            // Safely combining the log file path, ensuring no directory traversal happens
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), configuration["Logging:LogFilePath"] ?? "logs.txt");
        }

        [HttpGet]
        public IActionResult GetLogs(int? lines = 100)
        {
            // Check if the log file exists
            if (!System.IO.File.Exists(_logFilePath))
            {
                _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                return NotFound("Log file not found.");
            }

            try
            {
                var logs = ReadLogsFromFile(_logFilePath, lines ?? 100); // Default to the last 100 lines
                return Ok(logs);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error reading logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error reading logs: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied to log file: {LogFilePath}", _logFilePath);
                return StatusCode(403, $"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while reading logs.");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("clear")]
        public IActionResult ClearLogs()
        {
            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                System.IO.File.WriteAllText(_logFilePath, string.Empty);
                _logger.LogInformation("Logs cleared successfully.");
                return Ok(new { Message = "Logs cleared successfully." });
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error clearing logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error clearing logs: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied to log file: {LogFilePath}", _logFilePath);
                return StatusCode(403, $"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while clearing logs.");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("search")]
        public IActionResult SearchLogs([FromBody] SearchModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Keyword))
            {
                return BadRequest("Keyword is required.");
            }

            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                var logs = System.IO.File.ReadLines(_logFilePath)
                    .Where(line => line.Contains(model.Keyword, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Ok(logs);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error searching logs in file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error searching logs: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied to log file: {LogFilePath}", _logFilePath);
                return StatusCode(403, $"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while searching logs.");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        private List<string> ReadLogsFromFile(string filePath, int lineCount)
        {
            var logs = new List<string>();

            // Ensure file is available before reading
            if (new FileInfo(filePath).Length == 0) return logs;

            // Read the file in reverse order to get the most recent entries first
            var lines = System.IO.File.ReadLines(filePath).Reverse().Take(lineCount);

            logs.AddRange(lines);
            return logs;
        }
    }

    public class SearchModel
    {
        public string Keyword { get; set; }
    }
}
