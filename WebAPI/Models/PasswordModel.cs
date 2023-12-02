using System.ComponentModel.DataAnnotations;
using WebAPI.Utils;

namespace WebAPI.Models
{
    public class PasswordModel
    {

        [Required(ErrorMessage = "old password is required")]
        [Display(Name = "Old password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "new password is required")]
        [Display(Name = "New password")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [UppercaseLetter(ErrorMessage = "The new password must contain at least one uppercase letter.")]
        [LowercaseLetter(ErrorMessage = "The new password must contain at least one lowercase letter.")]
        [NumericSymbol(ErrorMessage = "The new password must contain at least one numeric symbol.")]
        [NonAlphanumericSymbol(ErrorMessage = "The new password must contain at least one non-alphabetic and non-numeric symbol.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation new password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
