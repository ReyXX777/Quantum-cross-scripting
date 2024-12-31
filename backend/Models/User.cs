using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

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

        // Hash the password before storing it
        public void SetPassword(string plainPassword)
        {
            // Use a secure hashing algorithm like SHA256 or bcrypt (bcrypt is recommended)
            Password = HashPassword(plainPassword);
        }

        // Hash password securely (Using SHA256 for simplicity, bcrypt is preferred in real scenarios)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // Method to validate a password during login (comparison against the hashed password)
        public bool ValidatePassword(string plainPassword)
        {
            var hashedPassword = HashPassword(plainPassword);
            return hashedPassword == Password;
        }
    }
}
