using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a non-member donor in the donor tracking system
    public class NonMember : IValidatableObject
    {
        // Unique identifier for the non-member donor
        public int ID { get; set; }

        // First name of the donor (optional if company/organization provided)
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        // Last name of the donor (optional if company/organization provided)
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        // Company or organization name (optional if first/last name provided)
        [Display(Name = "Company/Organization")]
        public string? CompanyOrganization { get; set; }

        // Optional contact information (phone, email, address, etc.)
        [Display(Name = "Contact Details")]
        public string? ContactDetails { get; set; }

        // Custom validation to ensure either name or company/organization is provided
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check if donor has either a name OR a company/organization
            bool hasName = !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName);
            bool hasCompany = !string.IsNullOrWhiteSpace(CompanyOrganization);

            // If neither is provided, return validation error
            if (!hasName && !hasCompany)
            {
                yield return new ValidationResult(
                    "Either First/Last Name or Company/Organization must be provided.",
                    new[] { nameof(FirstName), nameof(LastName), nameof(CompanyOrganization) }
                );
            }
        }
    }
}
