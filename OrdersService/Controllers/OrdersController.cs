using Microsoft.AspNetCore.Mvc;
using OrdersService.Dto;
using OrdersService.ErrorHandling;
using OrdersService.Model;
using OrdersService.Services;

namespace OrdersService.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrdersController : ControllerBase
    {
        IOrderManagementService _orderManagementService;
        public OrdersController(IOrderManagementService orderManagementService)
        {
            _orderManagementService = orderManagementService;
        }

        /// <summary>
        /// Создать заказ
        /// </summary>
        [HttpPost]
        [ProducesResponseType((int)StatusCodes.Status201Created)]
        [ProducesResponseType((int)StatusCodes.Status404NotFound)]
        [ProducesResponseType((int)StatusCodes.Status409Conflict)]
        public async Task<IResult> Post([FromBody] CreateOrderDto orderDto)
        {
            var result = await _orderManagementService.CreateOrderAsync(orderDto);

            if (result.IsFailure)
                return result.ToProblemDetails();

            return TypedResults.Created();
        }

        /// <summary>
        /// Изменить статус заказа
        /// </summary>
        /// <remarks>
        /// Возможны переходы в следующее рабочее состояние, отмена возможна только до подтверждения или при готовности к выдаче
        /// </remarks>
        [HttpPatch]
        [Route("{id}/status/{newStatus}")]
        [ProducesResponseType((int)StatusCodes.Status200OK)]
        [ProducesResponseType((int)StatusCodes.Status404NotFound)]
        [ProducesResponseType((int)StatusCodes.Status409Conflict)]
        public async Task<IResult> UpdateStatus(int id, OrderStatus newStatus) 
        { 
            var result = await _orderManagementService.UpdateOrderStatusAsync(id, newStatus);

            if (result.IsFailure)
                return result.ToProblemDetails();

            return TypedResults.Ok();
        }

        /// <summary>
        /// Заменить список позиций в заказе
        /// </summary>
        [HttpPatch]
        [Route(("{id}/items"))]
        [ProducesResponseType((int)StatusCodes.Status200OK)]
        [ProducesResponseType((int)StatusCodes.Status404NotFound)]
        [ProducesResponseType((int)StatusCodes.Status409Conflict)]
        public async Task<IResult> ChangeOrderItems(int id, [FromBody] List<OrderItemDto> items) 
        {
            if (items.Count == 0)
                return Result<object>.Failure(OrderErrors.EmptyItemList()).ToProblemDetails();

            var result = await _orderManagementService.UpdateOrderItemsAsync(id, items);

            if (result.IsFailure)
                return result.ToProblemDetails();

            return TypedResults.Ok();
        }



    }
}
