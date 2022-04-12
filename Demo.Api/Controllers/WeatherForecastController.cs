using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Demo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            var rnd = new Random();

            if (rnd.Next() % 3 == 0)
            {
                return StatusCode((int)HttpStatusCode.TooManyRequests);
            }

            if (rnd.Next() % 2 == 0) 
            {
                return StatusCode((int)HttpStatusCode.BadGateway);
            }

            return Ok(Summaries[rnd.Next(Summaries.Length)]);
        }
    }
}