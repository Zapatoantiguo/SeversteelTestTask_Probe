namespace OrdersService.ErrorHandling
{
    public static class InfrastructureError
    {
        public static Error ProductServiceError() => Error.Failure("Orders.Failure", $"Ошибка сервиса товаров");
    }
}
