using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a household/family group for donation tracking
    public class Family
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Family Name")]
        public string FamilyName { get; set; } = string.Empty;

        // Navigation property to member congregants
        public List<Congregant> Members { get; set; } = new List<Congregant>();
    }
}
