using System.ComponentModel.DataAnnotations;

namespace StorageService.Dto
{
    public class CreateOrderDto
    {
        [Required]
        public required string FullName { get; set; }
        [Required]
        public required string Phone { get; set; }

        public List<OrderRequestItem> Items { get; set; } = [];
    }

    public class OrderRequestItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
