using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a user in the donor tracking system
    public class User
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        // Ensures the ID is a 4-digit number
        [Range(1000, 9999, ErrorMessage = "ID must be a 4-digit number.")]
        public int ID { get; set; }

        public string Role { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        // Stores the hashed password (input should be a 6-digit number)
        public required string PasswordHash { get; set; }

        // Track the last login time for the user
        public DateTime LastLogin { get; set; } = DateTime.Now;
    }
}
