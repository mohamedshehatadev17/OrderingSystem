using OrderingSystem.Domain.Enums;

namespace OrderingSystem.API.DTOs.orderDtos
{
    public class updateOrderDto
    {
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; }
        public bool IsDeleted { get; set; }=false;
    }
}
