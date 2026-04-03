using System.ComponentModel.DataAnnotations;

namespace DonorTrackingSystem.ViewModels
{
    public class AccountLoginViewModel
    {
        [Required]
        public int ID { get; set; }

        [Required, DataType(DataType.Password)]
        public required int Password { get; set; }
    }
}
