using System.ComponentModel.DataAnnotations;

namespace OrdersService.Dto
{
    public class CreateOrderDto
    {
        [Required]
        public required string FullName { get; set; }
        [Required]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Номер телефона должен содержать только цифры и быть длиной от 10 до 15 символов.")]
        public required string Phone { get; set; }
        [MinLength(1)]
        public List<OrderItemDto> Items { get; set; } = [];
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
