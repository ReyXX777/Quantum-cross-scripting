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

        [MaxLength(50)]
        public string LogLevel { get; set; } = "Info";

        [MaxLength(100)]
        public string Source { get; set; } = "System";

        public string? User { get; set; }

        [MaxLength(100)]
        public string? RequestId { get; set; }

        [MaxLength(200)]
        public string? ExceptionDetails { get; set; }

        // Added Category and EventId
        [MaxLength(100)]
        public string? Category { get; set; } // Categorize logs (e.g., "Security", "Application")

        public int EventId { get; set; } // Numerical event identifier

        // Constructor to set the timestamp automatically
        public Log()
        {
            Timestamp = DateTime.UtcNow;
            EventId = 0; // Initialize EventId
        }

        public Log(string message, string? user = null, string? requestId = null, string? exceptionDetails = null, string? category = null, int eventId = 0)
        {
            Message = message;
            User = user;
            RequestId = requestId;
            ExceptionDetails = exceptionDetails;
            Timestamp = DateTime.UtcNow;
            LogLevel = "Info";
            Source = "System";
            Category = category; // Set category
            EventId = eventId;   // Set event ID
        }

        public void SetLogLevel(string logLevel)
        {
            if (string.IsNullOrWhiteSpace(logLevel))
            {
                throw new ArgumentException("Log level cannot be null or empty.");
            }

            LogLevel = logLevel;
        }

        public void SetSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("Source cannot be null or empty.");
            }

            Source = source;
        }

        public void SetCategory(string category)
        {
            Category = category;
        }

        public void SetEventId(int eventId)
        {
            EventId = eventId;
        }
    }
}

