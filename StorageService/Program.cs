using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StorageService;
using StorageService.ErrorHandling;
using StorageService.Model;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Строка подключения задана в переменной окружения в файле docker-compose
string connString = Environment.GetEnvironmentVariable("ConnectionString");
if (string.IsNullOrEmpty(connString))
    throw new InvalidOperationException("Строка подключения для БД не задана.");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connString));

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
    FillDbWithTestData(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<ErrorHandlingMiddleware>();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

static void FillDbWithTestData(AppDbContext db)
{
    var data = TestDataSeeder.Seed();
    List<Product> products = data.products;
    List<Order> orders = data.orders;

    db.Products.AddRange(products);
    db.Orders.AddRange(orders);

    db.SaveChanges();
}