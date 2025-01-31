using Microsoft.AspNetCore.Mvc;
using OrdersService.Dto;
using OrdersService.ErrorHandling;
using OrdersService.Model;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace OrdersService.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        IHttpClientFactory _httpClientFactory;
        public OrderManagementService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<CreateOrderResponseDto>> CreateOrderAsync(CreateOrderDto orderDto)
        {
            HttpClient client = _httpClientFactory.CreateClient("StorageService");

            var ids = orderDto.Items.Select(i => i.ProductId).ToList();

            string queryParam = "?" + string.Join("&", ids.Select(id => $"ids={id}"));
            string uri = "/api/products/multiple" + queryParam;

            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
                return Result<CreateOrderResponseDto>.Failure(InfrastructureError.ProductServiceError());

            var json = await response.Content.ReadAsStringAsync();
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // проверка на доступность всех товаров для заказа
            if (products.Count != ids.Count)
            {
                var absentProductIds = ids
                    .Except(products.Select(p => p.Id))
                    .ToList();

                return Result<CreateOrderResponseDto>.Failure(OrderErrors.ProductsNotExist(absentProductIds));
            }

            // проверка на достаточность всех товаров на складе
            var notEnoughIds = products.Join(
                orderDto.Items,
                product => product.Id,
                item => item.ProductId,
                (product, item) => (product, item))
                .Where(pi => pi.product.StockQuantity - pi.item.Quantity < 0)
                .Select(pi => pi.product.Id)
                .ToList();

            if (notEnoughIds.Count > 0)
                return Result<CreateOrderResponseDto>.Failure(OrderErrors.NotEnough(notEnoughIds));

            uri = "api/orders";
            response = await client.PostAsJsonAsync(uri, orderDto);

            if (!response.IsSuccessStatusCode)
            {
                return Result<CreateOrderResponseDto>.Failure(Error.Failure(
                    code: "StorageService.Error",
                    description: "Ошибка сервиса товаров при создании заказа")); 
            }

            return Result<CreateOrderResponseDto>.Success(new CreateOrderResponseDto());
        }

        public async Task<Result<UpdateOrderStatusResponseDto>> UpdateOrderStatusAsync(int id, OrderStatus newStatus)
        {
            HttpClient client = _httpClientFactory.CreateClient("StorageService");

            string uri = $"api/orders/{id}";
            var response = await client.GetAsync(uri);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result<UpdateOrderStatusResponseDto>.Failure(OrderErrors.ProductsNotExist(id));

            if (!response.IsSuccessStatusCode)
                return Result<UpdateOrderStatusResponseDto>.Failure(InfrastructureError.ProductServiceError());

            var json = await response.Content.ReadAsStringAsync();

            Order order = JsonSerializer.Deserialize<Order>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!order.IsValidStatusSwitch(newStatus))
                return Result<UpdateOrderStatusResponseDto>.Failure(OrderErrors.InvalidStatusSwitch(order.Status, newStatus));

            order.Status = newStatus;

            uri = $"api/orders/{id}";
            response = await client.PatchAsJsonAsync(uri, order);

            if (!response.IsSuccessStatusCode)
                return Result<UpdateOrderStatusResponseDto>.Failure(InfrastructureError.ProductServiceError());

            return Result<UpdateOrderStatusResponseDto>.Success(new UpdateOrderStatusResponseDto());
        }

        public async Task<Result<UpdateOrderItemsResponseDto>> UpdateOrderItemsAsync(int id, [FromBody] List<OrderItemDto> newItems)
        {
            HttpClient client = _httpClientFactory.CreateClient("StorageService");

            string uri = $"api/orders/{id}";
            var response = await client.GetAsync(uri);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result<UpdateOrderItemsResponseDto>.Failure(OrderErrors.ProductsNotExist(id));

            if (!response.IsSuccessStatusCode)
                return Result<UpdateOrderItemsResponseDto>.Failure(InfrastructureError.ProductServiceError());

            var json = await response.Content.ReadAsStringAsync();

            Order currentOrder = JsonSerializer.Deserialize<Order>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (currentOrder.Status != OrderStatus.ConfirmationAwaiting)
                return Result<UpdateOrderItemsResponseDto>.Failure(OrderErrors.ItemsChangeForbidden());

            string queryParam = "?" + string.Join("&", newItems.Select(ni => $"ids={ni.ProductId}"));
            uri = "/api/products/multiple" + queryParam;

            response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
                return Result<UpdateOrderItemsResponseDto>.Failure(InfrastructureError.ProductServiceError());

            json = await response.Content.ReadAsStringAsync();
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(json,new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            List<OrderItem> currentItems = currentOrder.Items;

            // далее логика для проверки существования всех товаров из нового списка,
            // а также проверки достаточности на складе с учетом старого и нового списков позиций...
            var currentQuantities = currentItems.ToDictionary(item => item.ProductId, item => item.Quantity);

            foreach (var newItem in newItems)
            {
                // проверка существования товара в БД
                var product = products.FirstOrDefault(p => p.Id == newItem.ProductId);
                if (product == null)    
                    return Result<UpdateOrderItemsResponseDto>.Failure(OrderErrors.ProductsNotExist(newItem.ProductId));

                // проверка достаточности на складе
                int oldQuantity = currentQuantities.TryGetValue(newItem.ProductId, out int existingQuantity) ? existingQuantity : 0;
                int stockAfterUpdate = product.StockQuantity - (newItem.Quantity - oldQuantity);

                if (stockAfterUpdate < 0)
                {
                    return Result<UpdateOrderItemsResponseDto>.Failure(OrderErrors.NotEnough(newItem.ProductId));
                }
            }

            var orderItems = newItems.Select(ni => new OrderItem
            {
                OrderId = currentOrder.Id,
                ProductId = ni.ProductId,
                Quantity = ni.Quantity,
                Price = products.Single(p => p.Id == ni.ProductId).Price
            }).ToList();

            currentOrder.Items = orderItems;
            currentOrder.TotalPrice = currentOrder.Items.Select(i => i.Quantity*i.Price).Sum();
            currentOrder.ProductQuantity = currentOrder.Items.Select(i => i.Quantity).Sum();

            uri = $"api/orders/{id}";
            response = await client.PatchAsJsonAsync(uri, currentOrder);

            if (!response.IsSuccessStatusCode)
                return Result<UpdateOrderItemsResponseDto>.Failure(InfrastructureError.ProductServiceError());

            return Result<UpdateOrderItemsResponseDto>.Success(new UpdateOrderItemsResponseDto());        
        }
    }
}
