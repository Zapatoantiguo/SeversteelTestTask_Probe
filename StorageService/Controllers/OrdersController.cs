using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorageService.Dto;
using StorageService.Model;

namespace StorageService.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrdersController : ControllerBase
    {
        AppDbContext _db;
        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Получить список заказов без указания содержимого
        /// </summary>
        [HttpGet]
        [ProducesResponseType((int)StatusCodes.Status200OK, Type = typeof(IEnumerable<Order>))]
        public async Task<IResult> Get(int page = 0, int pageSize = 15)
        {
            var orders = await _db.Orders
                .OrderBy(o => o.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return TypedResults.Ok(orders);
        }

        /// <summary>
        /// Получить заказ по ID, включая детали
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IResult> Get(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .SingleOrDefaultAsync(o => o.Id == id);

            if (order == null) return TypedResults.NotFound();

            return TypedResults.Ok(order);
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IResult> Post([FromBody] CreateOrderDto orderDto)
        {
            var products = await _db.Products
                .Where(p => orderDto.Items.Select(i => i.ProductId).Contains(p.Id))
                .ToListAsync();

            var productsQuantities = products.Join(
                orderDto.Items,
                product => product.Id,
                item => item.ProductId,
                (product, item) => (product, item.Quantity))
                .ToList();

            List<OrderItem> items = new List<OrderItem>();
            foreach (var pq in productsQuantities)
            {
                items.Add(new OrderItem
                {
                    Price = pq.product.Price,
                    Quantity = pq.Quantity,
                    ProductId = pq.product.Id
                });
                pq.product.StockQuantity -= pq.Quantity;
            }

            var order = new Order
            {
                CustomerFullName = orderDto.FullName,
                CustomerPhone = orderDto.Phone,
                Items = items,
                TotalPrice = items.Select(i => i.Price * i.Quantity).Sum(),
                ProductQuantity = items.Select(i => i.Quantity).Sum(),
                CreatedAt = DateTime.UtcNow,
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return TypedResults.Created();

        }

        [HttpDelete("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IResult> Delete(int id) 
        {
            var order = await _db.Orders
                .SingleOrDefaultAsync(o => o.Id == id);

            if (order == null) return TypedResults.NotFound();

            _db.Orders.Remove(order);

            return TypedResults.Ok();
        }

        [HttpPatch("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IResult> Patch(int id, [FromBody] Order newOrder) 
        { 
            var order = await _db.Orders
                .Include(o => o.Items)
                .SingleOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return TypedResults.NotFound();

            if (newOrder.Items != null)
            {
                _db.OrderItems.RemoveRange(order.Items);
                _db.OrderItems.AddRange(newOrder.Items);

                // TODO: изменение количества товаров на складе в соответствии с новым заказом
            }

            _db.Entry(order).CurrentValues.SetValues(newOrder);
            await _db.SaveChangesAsync();

            return TypedResults.Ok();
        }

    }
}
