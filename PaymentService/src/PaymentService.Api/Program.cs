using PaymentService.Application.Payments;
using PaymentService.Infrastructure.Persistence;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<ProcessPaymentHandler>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/payments/process", async (ProcessPaymentRequest request, ISender sender, CancellationToken ct) =>
{
    try
    {
        var response = await sender.Send(request, ct);
        return Results.Ok(response);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

public partial class Program { }
