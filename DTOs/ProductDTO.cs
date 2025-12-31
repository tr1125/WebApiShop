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
        int ProductId,
        [Required]
        string ProductName,
        [Required] 
        double Price,
        int CategoryId,
        string Description
    );
    
}
