using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a donation in the donor tracking system

    public class Donation
    {
        // Unique identifier for the donation record
        public int ID { get; set; }

        // ID of the person who made the donation (auto-assigned starting from 1000)
        [Display(Name = "Donor ID")]
        public int DonorID { get; set; }

        // Optional envelope number for envelope donations
        [Display(Name = "Envelope Number")]
        public string? EnvelopeNumber { get; set; }

        // Amount of money donated (must be greater than zero)
        [Required]
        [Display(Name = "Donation Amount")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Donation amount must be greater than zero.")]
        public decimal DonationAmount { get; set; }

        // Date when the donation was made (defaults to today)
        [Required]
        [Display(Name = "Donation Date")]
        public DateTime DonationDate { get; set; } = DateTime.Now;

        // ID of the fund where this donation should be allocated
        [Required]
        [Display(Name = "Fund Designation")]
        public int FundDesignationID { get; set; }

        // Navigation property to access the full fund designation details
        public FundDesignation? FundDesignation { get; set; }

        // Optional link to congregant donor (null if non-member or anonymous)
        [Display(Name = "Congregant")]
        public int? CongregantID { get; set; }

        // Navigation property to access congregant details
        public Congregant? Congregant { get; set; }

        // Optional link to non-member donor (null if anonymous or congregant donation)
        [Display(Name = "Non-Member Donor")]
        public int? NonMemberID { get; set; }

        // Navigation property to access non-member donor details
        public NonMember? NonMember { get; set; }

        // ID of the staff member who recorded this donation
        [Required]
        [Display(Name = "Staff Member ID")]
        public int StaffMemberID { get; set; }

        // Timestamp when the donation record was created
        private DateTimeOffset _created;
        public DateTimeOffset Created
        {
            get => _created;
            set => _created = value.ToUniversalTime();
        }
    }
}
