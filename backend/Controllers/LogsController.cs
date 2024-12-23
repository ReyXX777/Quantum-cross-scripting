using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly string _logFilePath = "logs.txt"; // Example file path for logs

        public LogsController()
        {
            // This is just an example. You might want to inject a service for logging.
        }

        [HttpGet]
        public IActionResult GetLogs()
        {
            // Retrieve logs from a file, database, or other sources.
            // This example reads logs from a text file.

            if (!System.IO.File.Exists(_logFilePath))
            {
                return NotFound("Log file not found.");
            }

            try
            {
                var logs = ReadLogsFromFile(_logFilePath);
                return Ok(logs);
            }
            catch (IOException ex)
            {
                // Log the exception and return an internal server error response
                return StatusCode(500, $"Error reading logs: {ex.Message}");
            }
        }

        private List<string> ReadLogsFromFile(string filePath)
        {
            // Example: Reads logs from a file. Adjust this method based on your log storage solution.
            var logs = new List<string>();

            foreach (var line in System.IO.File.ReadLines(filePath))
            {
                logs.Add(line);
            }

            return logs;
        }
    }
}
