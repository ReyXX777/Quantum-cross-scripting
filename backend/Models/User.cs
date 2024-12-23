using System;
using System.ComponentModel.DataAnnotations;

namespace QuantumCrossScripting.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }  // User's username, should be unique

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; }  // User's password, should be stored securely

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }  // User's email address

        public DateTime CreatedAt { get; set; }  // Timestamp when the user was created
        public DateTime? UpdatedAt { get; set; }  // Timestamp when the user was last updated (nullable)

        [MaxLength(50)]
        public string Role { get; set; }  // User's role (e.g., Admin, User, Moderator)

        public bool IsActive { get; set; }  // Flag indicating if the user is active

        public User()
        {
            CreatedAt = DateTime.UtcNow;  // Automatically set the creation timestamp
            IsActive = true;  // New users are active by default
        }

        // Optionally: Methods for password hashing, validation, etc.
    }
}
