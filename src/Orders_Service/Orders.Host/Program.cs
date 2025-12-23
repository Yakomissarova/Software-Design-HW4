using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;
using Orders.UseCases;
using Orders.Infrastructure;
using Orders.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Presentation
builder.Services.AddOrdersPresentation();

// UseCases
builder.Services.AddOrdersUseCases();

// Infrastructure (SQLite + RabbitMQ + background workers)
builder.Services.AddOrdersInfrastructure(builder.Configuration);

// Swagger — только в Host
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Автомиграции — при старте сервиса
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();