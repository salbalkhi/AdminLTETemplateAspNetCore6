using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Tadawi.Models
{
    public class Personels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name field is required.")]
        [StringLength(100, ErrorMessage = "Name can be at most 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname field is required.")]
        [StringLength(100, ErrorMessage = "Surname can be at most 100 characters.")]
        public string Surname { get; set; }
        
        [StringLength(100, ErrorMessage = "Email can be at most 100 characters.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Required(ErrorMessage = "Email field is required.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password field is required.")]
        public string Password { get; set; }
        
        [StringLength(100, ErrorMessage = "Phone number can be at most 100 characters.")]
        public string PhoneNumber { get; set; }
        
        [StringLength(100, ErrorMessage = "Card number can be at most 100 characters.")]
        public string CardNumber { get; set; }
        
        [StringLength(100, ErrorMessage = "Firm name can be at most 100 characters.")]
        public string FirmName { get; set; }
        
        [StringLength(100, ErrorMessage = "Sicil number can be at most 100 characters.")]
        public string SicilNo { get; set; }
    }
}
