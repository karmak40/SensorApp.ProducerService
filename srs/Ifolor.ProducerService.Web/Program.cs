using Ifolor.ProducerService.Core.Models;
using Ifolor.ProducerService.Core.Services;
using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Application.Generator;
using IfolorProducerService.Application.Mapping;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Generator;
using IfolorProducerService.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Bind environment variables to the RabbitMQConfig class
builder.Configuration.AddEnvironmentVariables(prefix: "RabbitMQ__");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<ProducerPolicy>(builder.Configuration.GetSection("Producerpolicy"));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.Configure<SQLiteConfig>(builder.Configuration.GetSection("SQLite"));

// SQLite
builder.Services.AddDbContextFactory<ProducerDbContext>((serviceProvider, options) =>
{
    var sqliteConfig = serviceProvider.GetRequiredService<IOptions<SQLiteConfig>>().Value;
    options.UseSqlite(sqliteConfig.ConnectionString);
});

// DI
builder.Services.AddTransient<IMessageProducer, RabbitMQProducer>();
builder.Services.AddTransient<ISensorService, SensorService>();
builder.Services.AddSingleton<IControlService, ControlService>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<ISendService, SendService>();
builder.Services.AddTransient<IResendService, ResendService>();
builder.Services.AddTransient<ISensorDataGenerator, SensorDataGenerator>();
builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();


var app = builder.Build();

// Configure the HTTP request pipeline

app.UseSwagger();
app.UseSwaggerUI();


app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProducerDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

var rabbitMQConfig = app.Services.GetRequiredService<IOptions<RabbitMQConfig>>().Value;
Console.WriteLine($"RabbitMQ Host: {rabbitMQConfig.HostName}");

await app.RunAsync();