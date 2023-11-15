using System.ComponentModel.DataAnnotations;
namespace Api.Models

{
    public class RegisterModel
    {
        [Required(ErrorMessage = "firstname is required")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "lastname is required")]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "username is required")]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "password is required")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
