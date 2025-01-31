using Microsoft.AspNetCore.Mvc;
using OrdersService.Dto;
using OrdersService.ErrorHandling;
using OrdersService.Model;

namespace OrdersService.Services
{
    public interface IOrderManagementService
    {
        public Task<Result<CreateOrderResponseDto>> CreateOrderAsync(CreateOrderDto orderDto);
        public Task<Result<UpdateOrderStatusResponseDto>> UpdateOrderStatusAsync(int id, OrderStatus newStatus);
        public Task<Result<UpdateOrderItemsResponseDto>> UpdateOrderItemsAsync(int id, [FromBody] List<OrderItemDto> items);
    }
}
