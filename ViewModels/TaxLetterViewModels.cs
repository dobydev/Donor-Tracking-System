namespace DonorTrackingSystem.ViewModels
{
    // This class represents a single line item in a tax letter, which includes the date of the donation, the name of the fund to which the donation was made, and the amount of the donation.
    public class TaxLetterDonationLine
    {
        public DateTime Date { get; set; }
        public string FundName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    // This view model is for an individual tax letter, which includes the recipient's name, address, the year of the tax letter, and a list of donations made by the recipient. It also has a computed property to calculate the total amount of donations.
    public class TaxLetterViewModel
    {
        public string RecipientName { get; set; } = string.Empty;
        public string? RecipientAddress { get; set; }
        public int Year { get; set; }
        public List<TaxLetterDonationLine> Donations { get; set; } = new();
        public decimal Total => Donations.Sum(d => d.Amount);
    }

    // This view model is for a family tax letter, which includes the family name and the names of the family members. It inherits from TaxLetterViewModel to reuse the common properties.
    public class FamilyTaxLetterViewModel : TaxLetterViewModel
    {
        public string FamilyName { get; set; } = string.Empty;
        public List<string> MemberNames { get; set; } = new();
    }
}
