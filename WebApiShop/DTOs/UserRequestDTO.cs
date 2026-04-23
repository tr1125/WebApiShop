using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record UserRequestDTO
    (
        [Required, EmailAddress]
        string UserName,
        [Required]
        string FirstName,
        [Required]
        string LastName,
        [Required]
        string Password,
        string Address,
        string Phone,
        bool IsAdmin = false
    );
}
