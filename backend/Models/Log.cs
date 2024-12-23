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

        // Optional: You can add more properties if needed, such as log level or type
        [MaxLength(50)]
        public string LogLevel { get; set; }  // e.g., "Info", "Warning", "Error"

        [MaxLength(100)]
        public string Source { get; set; }  // The source of the log, such as "AuthController" or "DetectionService"
        
        // Optional: User who generated the log (if applicable)
        public string User { get; set; }  // Assumes you want to track the user, could be null
        
        public Log()
        {
            Timestamp = DateTime.UtcNow;  // Automatically set the timestamp when the log is created
        }
    }
}
