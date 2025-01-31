using System.Text.Json.Serialization;

namespace OrdersService.Model
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
        public List<OrderItem> Items { get; set; }
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

    public static class OrderExtensions
    {
        public static bool IsValidStatusSwitch(this Order order, OrderStatus newStatus)
        {
            OrderStatus currentStatus = order.Status;

            // условная логика возможных изменений статуса в зависимости от текущего
            // из каждого состояния можно перейти в следующее,
            // отменить заказ можно только при ожидании подтверждения или если заказ готов к выдаче
            return (currentStatus, newStatus) switch
            {
                (OrderStatus.ConfirmationAwaiting, OrderStatus.Confirmed) => true,
                (OrderStatus.ConfirmationAwaiting, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.ReadyForDelivery) => true,
                (OrderStatus.ReadyForDelivery, OrderStatus.Completed) => true,
                (OrderStatus.ReadyForDelivery, OrderStatus.Cancelled) => true,
                _ => false
            };
        }
    }
}
