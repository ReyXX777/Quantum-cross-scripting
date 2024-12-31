using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

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
            _logFilePath = configuration["Logging:LogFilePath"] ?? "logs.txt"; // Get log file path from configuration or default to logs.txt
        }

        [HttpGet]
        public IActionResult GetLogs()
        {
            // Check if the log file exists
            if (!System.IO.File.Exists(_logFilePath))
            {
                _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                return NotFound("Log file not found.");
            }

            try
            {
                var logs = ReadLogsFromFile(_logFilePath);
                return Ok(logs);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error reading logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error reading logs: {ex.Message}");
            }
        }

        private List<string> ReadLogsFromFile(string filePath)
        {
            // Reads logs from the file. You might want to adjust this based on your log format or storage solution.
            var logs = new List<string>();

            // Using a stream reader for better performance on large files
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    logs.Add(reader.ReadLine());
                }
            }

            return logs;
        }
    }
}
