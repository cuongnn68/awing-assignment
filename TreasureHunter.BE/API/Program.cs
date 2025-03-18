using System.ComponentModel.DataAnnotations;

using Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
Console.WriteLine("yoooo");
Console.WriteLine(builder.Configuration.GetConnectionString("TreasureHunter"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "custom",
                      policy => policy
                        .WithOrigins(["http://localhost:5172", "http://localhost:5173"])
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});
builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("TreasureHunter"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("custom");
}

app.UseHttpsRedirection();

using var scope = app.Services.CreateAsyncScope();
using var db = scope.ServiceProvider.GetService<AppDbContext>();

if (db is null || await db.Database.CanConnectAsync())
{
    throw new Exception("Cant connect to database");
}

if((await db.Database.GetPendingMigrationsAsync()).Any()) 
{
    await db.Database.MigrateAsync();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/test", async ([FromServices] AppDbContext db, CancellationToken cancel) =>
{
    Console.WriteLine("Im still alive");
    return await db.TreasureMaps.ToListAsync(cancel);
});

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/api/treasure-map/calculate-route", double? ([FromBody] CalculateFastestRouteRequest request) =>
{
    // TODO: validate

    var finalKey = request.P;
    var islands = request.Matrix.SelectMany(MapToIslands).ToList();
    List<Route> routes = [new Route(islands.First(), 0)];
    for (int currentKey = 0; currentKey < finalKey; currentKey++)
    {
        if (routes.Count == 0) return null;
        routes = islands
            .Where(island => island.TreasureChest == currentKey + 1)
            .Select(ShortestRoute)
            .ToList();
    }
    return routes.First().PassedDistance;

    IEnumerable<Island> MapToIslands(int[] row, int y)
        => row.Select((int value, int x) => new Island(value, x + 1, y + 1));

    double CalDistance(Island island1, Island island2)
        => Math.Sqrt(Math.Pow(island1.X - island2.X, 2) + Math.Pow(island1.Y - island2.Y, 2));

    Route ShortestRoute(Island nextIsland)
        => routes
            .Select(route => new Route(
                nextIsland,
                route.PassedDistance + CalDistance(route.CurrentIsland, nextIsland)))
            .MinBy(route => route.PassedDistance)!;

});

app.Run();

record Route(Island CurrentIsland, double PassedDistance);
record Island(int TreasureChest, int X, int Y);
record CalculateFastestRouteRequest
{
    [Required] public int M { get; set; }
    [Required] public int N { get; set; }
    [Required] public int P { get; set; }
    [Required] public int[][] Matrix { get; set; } = null!;
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}