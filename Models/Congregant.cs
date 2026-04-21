using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Enum to represent the active status of a congregant
    public enum ActiveStatus
    {
        [Display(Name = "Current Member")]
        CurrentMember,
        
        [Display(Name = "Transferred Membership")]
        TransferredMembership,
        
        [Display(Name = "Left Church")]
        LeftChurch
    }

    // Represents a congregant (church member) in the donor tracking system
    public class Congregant
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? EmailAddress { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Active Status")]
        public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.CurrentMember;

        [Display(Name = "Committee Memberships")]
        public List<string> CommitteeMemberships { get; set; } = new List<string>();

        // Optional link to a family/household group
        [Display(Name = "Family")]
        public int? FamilyID { get; set; }

        // Navigation property to the family
        public Family? Family { get; set; }
    }
}
