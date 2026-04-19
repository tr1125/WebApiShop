using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public record OrderDTO
    (
        int OrderId,
        DateOnly? OrderDate = null,
        double? OrderSum = null,
        List<OrderItemDTO> OrderItems = null,
        string Status = null,
        int? UserId = null
    );
}
