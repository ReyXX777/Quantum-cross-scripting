using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace QuantumCrossScripting.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(50)]
        [RegularExpression("Admin|User|Moderator", ErrorMessage = "Role must be 'Admin', 'User', or 'Moderator'.")]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        // Added two new properties:
        [MaxLength(200)]
        public string? ProfilePictureUrl { get; set; } // URL to the user's profile picture

        [MaxLength(500)]
        public string? Bio { get; set; } // User's biography or about me section


        public User()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Role = "User";
        }

        public void SetPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("Password cannot be empty or whitespace.");

            Password = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        public bool ValidatePassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(plainPassword, Password);
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateTimestamp();
        }

        public void Activate()
        {
            IsActive = true;
            UpdateTimestamp();
        }

        // Method to update profile picture URL
        public void SetProfilePicture(string pictureUrl)
        {
            ProfilePictureUrl = pictureUrl;
            UpdateTimestamp(); // Update timestamp when profile picture is changed
        }

        // Method to update user bio
        public void SetBio(string bio)
        {
            Bio = bio;
            UpdateTimestamp(); // Update timestamp when bio is changed
        }
    }
}

