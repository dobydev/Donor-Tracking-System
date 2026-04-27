using Microsoft.AspNetCore.Identity;

namespace DonorTrackingSystem.Models
{
    // Inherit from IdentityUser to include properties like User ID and other identity-related information.
    public class ApplicationUser : IdentityUser
    {
        // Tracks whether the staff account is active; inactive accounts cannot log in
        public bool IsActive { get; set; } = true;
    }
}
