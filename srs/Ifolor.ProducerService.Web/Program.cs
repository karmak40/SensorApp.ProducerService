using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services.Configure<RabbitMQConfig>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<SQLiteConfig>(
    builder.Configuration.GetSection("SQLite"));

// SQLite
builder.Services.AddDbContextFactory<EventDbContext>((serviceProvider, options) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<SQLiteConfig>>().Value;
    options.UseSqlite(config.ConnectionString);
});

// DI
builder.Services.AddTransient<IEventGenerator, TestEventGenerator>();
builder.Services.AddTransient<IMessageProducer, RabbitMQProducer>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IEventProducerService, EventProducerService>();

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseSwagger();
app.UseSwaggerUI();


app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

await app.RunAsync();