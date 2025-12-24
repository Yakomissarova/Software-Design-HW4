using Microsoft.EntityFrameworkCore;
using Payments.Infrastructure.Persistence;
using Payments.UseCases;
using Payments.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Presentation
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// UseCases + Infrastructure
builder.Services.AddPaymentsUseCases();
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

var app = builder.Build();

// Автомиграции
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();