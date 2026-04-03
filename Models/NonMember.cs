using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.Models
{
    // Represents a non-member donor in the donor tracking system
    public class NonMember : IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Company/Organization")]
        public string? CompanyOrganization { get; set; }

        [Display(Name = "Contact Details")]
        public string? ContactDetails { get; set; }

        // Custom validation to ensure either name or company/organization is provided
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool hasName = !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName);
            bool hasCompany = !string.IsNullOrWhiteSpace(CompanyOrganization);

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
