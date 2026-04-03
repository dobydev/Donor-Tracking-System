using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a donation in the donor tracking system
    public class Donation
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Donor ID")]
        public int DonorID { get; set; }

        [Required]
        [Display(Name = "Donation Amount")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Donation amount must be greater than zero.")]
        public decimal DonationAmount { get; set; }

        [Required]
        [Display(Name = "Donation Date")]
        [DataType(DataType.Date)]
        public DateTime DonationDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Staff Member ID")]
        public int StaffMemberID { get; set; }
    }
}
