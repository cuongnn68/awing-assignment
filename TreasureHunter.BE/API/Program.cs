using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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

app.MapPost("/api/treasure-map/calculate-route", ([FromBody] CalculateFastestRouteRequest request) =>
{
    // TODO: validate
    var finalKey = request.P;
    var islands = request.Matrix.SelectMany(MapToIslands);
    var currentIsland = islands.First();
    var currentKey = 0;
    while (currentKey <= finalKey)
    {
        var nextPossibleIslands = islands.Where(island => island.TreasureChest == currentKey + 1);
        if (nextPossibleIslands is null) return null;
        // TODO: made a big mistake, it not that simple, thinking about using dynamic programing
        nextPossibleIslands.Select(island => new { island, distance = CalDistance(island, currentIsland) })
            .MinBy(e => e.distance);
    }
    return request;
    IEnumerable<Island> MapToIslands(int[] row, int y)
        => row.Select((int value, int x) => new Island(value, x, y));
    double CalDistance(Island island1, Island island2)
        => Math.Sqrt(Math.Pow(island1.X - island2.X, 2) + Math.Pow(island1.Y - island2.Y, 2));
});

app.Run();

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