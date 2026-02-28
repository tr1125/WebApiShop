using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record UserLoginDTO
    (
        [Required, EmailAddress]
        string UserName,
        [Required]
        string Password
    );
}
