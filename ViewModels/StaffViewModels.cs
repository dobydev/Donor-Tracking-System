using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.ViewModels
{
    // ViewModel for displaying staff members in a list
    public class StaffListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string StaffId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // ViewModel for adding a new staff member
    public class AddStaffViewModel
    {
        [Required(ErrorMessage = "Staff ID is required.")]
        [Display(Name = "Staff ID")]
        public string StaffId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;
    }

    // ViewModel for editing an existing staff member
    public class EditStaffViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Staff ID")]
        public string StaffId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }
    }
}
