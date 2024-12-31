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
        [StringLength(50)] // Optional: Add this to explicitly specify the max length.
        public string LogLevel { get; set; } = "Info";  // Default to "Info" if not set

        // The source of the log (e.g., "AuthController", "DetectionService")
        [MaxLength(100)]
        [StringLength(100)]
        public string Source { get; set; } = "System";  // Default to "System" if not set

        // User who generated the log (optional and nullable)
        public string? User { get; set; }  // Nullable user, in case not all logs are associated with a user

        // Optional: Additional properties
        [MaxLength(100)]
        public string? RequestId { get; set; }  // Optional request identifier for tracing purposes
        
        [MaxLength(200)]
        public string? ExceptionDetails { get; set; }  // Optional field for exception details (if applicable)

        public Log()
        {
            Timestamp = DateTime.UtcNow;  // Automatically set the timestamp when the log is created
        }
    }
}
