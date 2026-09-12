using OrderingSystem.Domain.Enums;

namespace OrderingSystem.API.DTOs.orderDtos
{
    public class orderResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime Date { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Amount { get; set; }
    }
}
