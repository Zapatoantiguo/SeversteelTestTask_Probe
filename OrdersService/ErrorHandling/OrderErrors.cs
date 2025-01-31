using OrdersService.Model;

namespace OrdersService.ErrorHandling
{
    public class OrderErrors
    {
        public static Error EmptyItemList() => Error.Validation(
            "Orders.Validation", $"Список товаров в заказе не должен быть пустым");
        public static Error ProductsNotExist(int id) => Error.NotFound(
            "Orders.NotFound", $"В БД отсутствуют товар с ID: {id}");
        public static Error ProductsNotExist(List<int> ids) => Error.NotFound(
            "Orders.NotFound", $"В БД отсутствуют товары со следующими ID: {string.Join(", ", ids)}");

        public static Error NotEnough(int id) => Error.Conflict(
            "Orders.Conflict", $"На складе недостаточно товаров с ID: {id}");
        public static Error NotEnough(List<int> ids) => Error.Conflict(
            "Orders.Conflict", $"На складе недостаточно товаров со следующими ID: {string.Join(", ", ids)}");

        public static Error InvalidStatusSwitch(OrderStatus currentStatus, OrderStatus newStatus) => Error.Conflict(
            "Orders.Conflict", $"Невозможно изменить статус заказа с {currentStatus} на {newStatus}");

        public static Error ItemsChangeForbidden() => Error.Conflict(
            "Order.Conflict", $"Изменение состава заказа возможно только до его подтверждения");
    }
}
