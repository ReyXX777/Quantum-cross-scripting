using System;
using System.ComponentModel.DataAnnotations;

namespace QuantumCrossScripting.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        // Log level with a default value if not provided
        [MaxLength(50)]
        public string LogLevel { get; set; } = "Info";  // Default to "Info" if not set

        // The source of the log (e.g., "AuthController", "DetectionService")
        [MaxLength(100)]
        public string Source { get; set; } = "System";  // Default to "System" if not set

        // User who generated the log (optional and nullable)
        public string? User { get; set; }  // Nullable user, in case not all logs are associated with a user

        // Optional: Additional properties
        [MaxLength(100)]
        public string? RequestId { get; set; }  // Optional request identifier for tracing purposes
        
        [MaxLength(200)]
        public string? ExceptionDetails { get; set; }  // Optional field for exception details (if applicable)

        // Constructor to set the timestamp automatically
        public Log()
        {
            Timestamp = DateTime.UtcNow;  // Automatically set the timestamp when the log is created
        }

        // Optional constructor to initialize with a message and source
        public Log(string message, string? user = null, string? requestId = null, string? exceptionDetails = null)
        {
            Message = message;
            User = user;
            RequestId = requestId;
            ExceptionDetails = exceptionDetails;
            Timestamp = DateTime.UtcNow;  // Set the timestamp
            LogLevel = "Info";  // Default value
            Source = "System";  // Default value
        }

        // Method to update the log level
        public void SetLogLevel(string logLevel)
        {
            if (string.IsNullOrWhiteSpace(logLevel))
            {
                throw new ArgumentException("Log level cannot be null or empty.");
            }

            LogLevel = logLevel;
        }

        // Method to update the source of the log
        public void SetSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("Source cannot be null or empty.");
            }

            Source = source;
        }
    }
}
