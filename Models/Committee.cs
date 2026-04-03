using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a committee in the donor tracking system
    public class Committee
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
