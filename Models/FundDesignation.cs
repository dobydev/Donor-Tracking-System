using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a fund designation in the donor tracking system
    public class FundDesignation
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Active Status")]
        public bool ActiveStatus { get; set; } = true;
    }
}
