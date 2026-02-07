using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    public bool RememberMe { get; set; }
}
