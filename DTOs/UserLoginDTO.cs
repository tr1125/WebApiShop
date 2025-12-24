using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record UserLoginDTO
    (
        [Required, EmailAddress]
        string Name,
        [Required]
        string Password
    );
}
