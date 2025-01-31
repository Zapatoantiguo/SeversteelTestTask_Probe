using OrdersService.ErrorHandling;
using OrdersService.Services;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Базовый адрес сервиса продуктов из среды окружения (docker-compose)
var storageUrl = Environment.GetEnvironmentVariable("StorageService__BaseUrl");

if (string.IsNullOrEmpty(storageUrl))
    throw new InvalidOperationException("URL сервиса продуктов не сконфигурирован");

// Для запросов к сервису продуктов будет использоваться IHttpClientFactory для избежания растраты сокетов
builder.Services.AddHttpClient("StorageService", client => client.BaseAddress = new Uri(storageUrl));

builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();

var app = builder.Build();

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
