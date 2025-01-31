using System.Text.Json.Serialization;

namespace StorageService.Model
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StorageUntil { get; set; }
        public OrderStatus Status { get; set; }
        public string CustomerFullName { get; set; }
        public string CustomerPhone { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItem> Items { get; set; } = [];
        public int ProductQuantity { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        ConfirmationAwaiting,   // ожидание подтверждения
        Confirmed,              // подтвержден
        ReadyForDelivery,       // готов к выдаче
        Completed,              // завершен
        Cancelled               // отменен
    }
}
