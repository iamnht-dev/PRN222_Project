using System.ComponentModel.DataAnnotations;

namespace zCoach.MVCWebApp.ThaiNH.Models;

public class LoginRequest
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
