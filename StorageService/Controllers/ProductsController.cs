using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorageService.Model;
using Swashbuckle.AspNetCore.Annotations;

namespace StorageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        AppDbContext _db;
        public ProductsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Получить список товаров
        /// </summary>
        [HttpGet]
        [ProducesResponseType((int)StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        public async Task<IResult> Get(int page = 0, int pageSize = 15)
        {
            var products = await _db.Products
                .OrderBy(p => p.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return TypedResults.Ok(products);
        }

        /// <summary>
        /// Получить подмножество товаров
        /// </summary>
        /// <remarks>
        /// Получить подмножество товаров по массиву ID. Несуществующие ID игнорируются
        /// </remarks>
        [HttpGet]
        [Route("multiple")]
        [ProducesResponseType((int)StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        public async Task<IResult> Get([FromQuery] int[] ids)
        {
            var products = await _db.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            return TypedResults.Ok(products);
        }

        /// <summary>
        /// Получить товар по ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType((int)StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType((int)StatusCodes.Status404NotFound)]
        public async Task<IResult> Get(int id) 
        {
            var product = await _db.Products.SingleOrDefaultAsync(p => p.Id == id);

            if (product == null) return TypedResults.NotFound();

            return TypedResults.Ok(product);
        }


    }
}
