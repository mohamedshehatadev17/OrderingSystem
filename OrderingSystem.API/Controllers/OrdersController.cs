using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderingSystem.API.DTOs.orderDtos;
using OrderingSystem.Domain.Entities;
using OrderingSystem.Domain.Interfaces;
using System.Security.Claims;

namespace OrderingSystem.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController(IOrderRepository orderRepository,ICustomerRepository customerRepository) : ControllerBase
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly ICustomerRepository _customerRepository = customerRepository;
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderRepository.GetAllAsync();
            return Ok(orders);
        }
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            var orders = await _orderRepository.GetAllAsync(
                o => o.CustomerId == customerId,
                o => o.Customer);

            return Ok(orders.Select(o => new orderResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Date = o.OrderDate,
                Status = o.OrderStatus,
                Amount = o.Amount
            }));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound();
            return Ok(new orderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Date = order.OrderDate,
                Status = order.OrderStatus,
                Amount = order.Amount
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            if (request == null)
                return BadRequest();

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(customerId, out var customerIdInt))
                return BadRequest("Invalid customer ID");

            var customer = await _customerRepository.GetByIdAsync(customerIdInt);

            if (customer == null)
                return NotFound("Customer not found");

            // Check current ban
            if (customer.BannedUntil.HasValue &&customer.BannedUntil > DateTime.Now)
            {
                return BadRequest(
                    $"You are banned from creating orders until " +
                    $"{customer.BannedUntil:yyyy-MM-dd HH:mm}");
            }

            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            var deletedOrdersCount = await _orderRepository.CountAsync(
                o => o.CustomerId == customerIdInt &&
                     o.IsDeleted &&
                     o.OrderDate >= today &&
                     o.OrderDate < tomorrow 
                     );

            if (deletedOrdersCount >= 3)
            {
                customer.BannedUntil = DateTime.Now.AddHours(6);
                _customerRepository.UpdateAsync(customer);

                return BadRequest(
                    "You have deleted 3 or more orders today. " +
                    "You are banned from creating orders for 6 hours.");
            }

            var order = new Order
            {
                CustomerId = customerIdInt,
                Amount = request.Amount,
                OrderDate = DateTime.Now
            };

            var createdOrder = await _orderRepository.AddAsync(order);

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = createdOrder.Id },
                new orderResponse
                {
                    Id = createdOrder.Id,
                    CustomerId = createdOrder.CustomerId,
                    Date = createdOrder.OrderDate,
                    Status = createdOrder.OrderStatus,
                    Amount = createdOrder.Amount
                });
        }
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteOrder(int customerId)
        {
            var order = await _orderRepository.GetByIdAsync(customerId);
            if (order == null)
                return NotFound();
            var deleted = await _orderRepository.DeleteWhereAsync(c=>c.CustomerId == customerId);
            if (!deleted)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting order");
            order.IsDeleted = true;
            _orderRepository.UpdateAsync(order);
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] updateOrderDto order)
        {
            if (order == null || id < 1)
                return BadRequest();
            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null)
                return NotFound();
           
            existingOrder.OrderStatus = order.Status;
            existingOrder.Amount = order.Amount;
            existingOrder.UpdatedAt = DateTime.Now;
            var updated = _orderRepository.UpdateAsync(existingOrder);
            if (!updated)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating order");
            return NoContent();
        }

    }
}
