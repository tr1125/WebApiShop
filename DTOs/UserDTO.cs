using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record UserDTO
    (
        int UserId,
        [Required, EmailAddress]
        string UserName,
        [Required]
        string FirstName,
        [Required]
        string LastName,
        [Required]
        string Password
    ); 
}
