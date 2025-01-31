using StorageService.Model;

namespace StorageService
{
    public static class TestDataSeeder
    {
        public static (List<Product> products, List<Order> orders) Seed()
        {
            List<Product> products = new()
            {
                new Product { Article = "Prod1Art", Description = "Prod1Desc", Name = "Prod1Name", Price = 111, StockQuantity = 11 },
                new Product { Article = "Prod2Art", Description = "Prod2Desc", Name = "Prod2Name", Price = 222, StockQuantity = 22 },
                new Product { Article = "Prod3Art", Description = "Prod3Desc", Name = "Prod3Name", Price = 333, StockQuantity = 33 },
                new Product { Article = "Prod4Art", Description = "Prod4Desc", Name = "Prod4Name", Price = 444, StockQuantity = 44 },
                new Product { Article = "Prod5Art", Description = "Prod5Desc", Name = "Prod5Name", Price = 555, StockQuantity = 55 }
            };

            List<OrderItem> order1Items = new()
            {
                new OrderItem { Product = products[0], Price = products[0].Price, Quantity = 2}
            };

            List<OrderItem> order2Items = new()
            {
                new OrderItem { Product = products[1], Price = products[1].Price, Quantity = 1},
                new OrderItem { Product = products[2], Price = products[2].Price, Quantity = 3}
            };

            List<Order> orders = new()
            {
                new Order { CustomerFullName = "Order1Customer", CustomerPhone = "+123456789", CreatedAt = DateTime.UtcNow, Items = order1Items,
                            TotalPrice = order1Items.Select(oi => oi.Price * oi.Quantity).Sum(),
                            ProductQuantity = order1Items.Select(oi => oi.Quantity).Sum()},


                new Order { CustomerFullName = "Order2Customer", CustomerPhone = "+987654321", CreatedAt = DateTime.UtcNow, Items = order2Items,
                            TotalPrice = order2Items.Select(oi => oi.Price * oi.Quantity).Sum(),
                            ProductQuantity = order2Items.Select(oi => oi.Quantity).Sum()},
            };

            order1Items[0].Order = orders[0];
            order2Items[0].Order = orders[1];
            order2Items[1].Order = orders[1];

            return (products, orders);

        }

    }
}
