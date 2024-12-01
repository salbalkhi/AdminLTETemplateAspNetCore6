using System.ComponentModel.DataAnnotations;

namespace Tadawi.Models
{
    public class UserLoginModel
    {
        public bool RememberMe { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Required(ErrorMessage = "Email field is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        public string Password { get; set; }
    }
}
