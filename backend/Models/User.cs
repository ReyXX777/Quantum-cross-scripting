using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BCrypt.Net; // Use bcrypt for secure password hashing

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
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }  // User's password, should be stored securely

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }  // User's email address

        public DateTime CreatedAt { get; set; }  // Timestamp when the user was created
        public DateTime? UpdatedAt { get; set; }  // Timestamp when the user was last updated (nullable)

        [MaxLength(50)]
        [RegularExpression("Admin|User|Moderator", ErrorMessage = "Role must be 'Admin', 'User', or 'Moderator'.")]
        public string Role { get; set; }  // User's role (e.g., Admin, User, Moderator)

        public bool IsActive { get; set; }  // Flag indicating if the user is active

        public User()
        {
            CreatedAt = DateTime.UtcNow;  // Automatically set the creation timestamp
            IsActive = true;  // New users are active by default
            Role = "User";  // Default role for new users
        }

        // Hash the password before storing it
        public void SetPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("Password cannot be empty or whitespace.");

            Password = BCrypt.Net.BCrypt.HashPassword(plainPassword);  // Use bcrypt for secure hashing
        }

        // Method to validate a password during login (comparison against the hashed password)
        public bool ValidatePassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(plainPassword, Password);  // Verify using bcrypt
        }

        // Update the UpdatedAt timestamp
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
