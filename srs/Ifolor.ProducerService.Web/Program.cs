using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using Ifolor.ProducerService.Web.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Bind environment variables to the RabbitMQConfig class
builder.Configuration.AddEnvironmentVariables(prefix: "RabbitMQ__");

// Add services to the container
builder.Services.AddSystemsServices();

// Configuration
builder.Services.AddConfiguration(builder.Configuration);

// Add mapping
builder.Services.AddAutoMapper();

// SQLite
builder.Services.AddDbContextFactory<ProducerDbContext>((serviceProvider, options) =>
{
    var sqliteConfig = serviceProvider.GetRequiredService<IOptions<SQLiteConfig>>().Value;
    options.UseSqlite(sqliteConfig.ConnectionString);
});

// DI
builder.Services.AddDunkelverarbeitungs();

var app = builder.Build();

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