using Microsoft.EntityFrameworkCore;
using Payments.Infrastructure.Persistence;
using Payments.UseCases;
using Payments.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Presentation (контроллеры)
builder.Services.AddControllers();

// Swagger (только в Host)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// UseCases + Infrastructure
builder.Services.AddPaymentsUseCases();
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

var app = builder.Build();

// Автомиграции (технически верно: до обработки запросов)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.Migrate();
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();