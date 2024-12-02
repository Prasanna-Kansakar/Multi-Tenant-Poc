using Microsoft.AspNetCore.Mvc;
using Multi_Tenant_Poc.Persistence;

namespace Multi_Tenant_Poc.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public WeatherForecastController(AppDbContext dbContext)
        => _dbContext = dbContext;

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
        => _dbContext.Forecasts.OrderBy(f => f.Date).ToArray();

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        string[] summaries = ["Sweltering", "Chilly", "Cool", "Warn"];
        var weatherForecast = new WeatherForecast()
        {
            Date = DateTime.Now,
            Summary = summaries[Random.Shared.Next(summaries.Length)],
            TemperatureC = Random.Shared.Next(-20, 55),
        };
        await _dbContext.Forecasts.AddAsync(weatherForecast);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = weatherForecast.Id }, weatherForecast);
    }
}