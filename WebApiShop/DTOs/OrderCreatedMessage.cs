namespace DTOs
{
    public record OrderItemMessage(
        int ProductId,
        string? ProductName,
        int? Quantity,
        double? Price
    );

    public record OrderCreatedMessage(
        int OrderId,
        int? UserId,
        DateOnly? OrderDate,
        double? OrderSum,
        string? Status,
        List<OrderItemMessage> OrderItems
    );
}
