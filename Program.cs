using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<OrderDb>(opt => opt.UseInMemoryDatabase("OrderList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSingleton<OrderEvents>();
var app = builder.Build();

app.MapGet("/", () => "Welcome!");

app.MapGet("/orders", async (OrderDb db) => await db.Orders.ToListAsync());

app.MapPost(
    "/placeorder",
    async (Order order, OrderDb db, OrderEvents orderEvents) =>
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        orderEvents.RaiseOrderPlaced(order); // Raise the event

        return Results.Created($"/orders/{order.Id}", order);
    }
);

app.MapGet(
    "/events",
    async (HttpContext httpContext) =>
    {
        var response = httpContext.Response;
        var request = httpContext.Request;

        if (request.Headers["Accept"].ToString().Contains("text/event-stream"))
        {
            response.Headers["Content-Type"] = "text/event-stream";
            response.Headers["Cache-Control"] = "no-cache";
            response.Headers["Connection"] = "keep-alive";

            var feature = response.HttpContext.Features.Get<IHttpResponseBodyFeature>();
            feature.DisableBuffering();

            var orderEvents = httpContext.RequestServices.GetRequiredService<OrderEvents>();
            orderEvents.OrderPlaced += async (order) =>
            {
                await response.WriteAsync($"data: {JsonSerializer.Serialize(order)}\n\n");
                await response.Body.FlushAsync();
            };

            // Keep the connection open
            await Task.Delay(Timeout.Infinite, httpContext.RequestAborted);
        }
    }
);

app.Run();
