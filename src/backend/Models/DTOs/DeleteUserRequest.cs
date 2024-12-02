using System.ComponentModel.DataAnnotations;

namespace Tadawi.Models.DTOs;

public class DeleteUserRequest
{
    [Required(ErrorMessage = "Password is required to delete account")]
    public string Password { get; set; } = string.Empty;
}
