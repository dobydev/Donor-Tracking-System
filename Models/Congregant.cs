using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Enum to represent the active status of a congregant
    public enum ActiveStatus
    {
        CurrentMember,
        TransferredMembership,
        LeftChurch
    }

    // Represents a congregant (church member) in the donor tracking system
    public class Congregant
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Active Status")]
        public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.CurrentMember;

        [Display(Name = "Committee Memberships")]
        public List<string> CommitteeMemberships { get; set; } = new List<string>();
    }
}
