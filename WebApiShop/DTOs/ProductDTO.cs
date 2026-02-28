using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public record ProductDTO
    (
        int ProductId = 0,
        [Required]
        string ProductName = "",
        [Required] 
        double Price = 0,
        int CategoryId = 0,
        string? Description = "",
        string? ImageURL = "",
        string? Color = "",
        bool IsDeleted = false
    );
    
}
