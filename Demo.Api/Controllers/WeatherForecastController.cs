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

            if (rnd.Next() % 4 == 0)
            {
                // return StatusCode((int)HttpStatusCode.BadGateway);
                return StatusCode(StatusCodes.Status401Unauthorized, "Unauthorized: Please try again later.");
            }

            if (rnd.Next() % 3 == 0)
            {
                //return StatusCode((int)HttpStatusCode.TooManyRequests);
                return StatusCode(StatusCodes.Status429TooManyRequests, "Too Many Requests: Please try again later.");
            }

            if (rnd.Next() % 2 == 0) 
            {
                // return StatusCode((int)HttpStatusCode.BadGateway);
                return StatusCode(StatusCodes.Status502BadGateway, "Bad Gateway: Please try again later.");
            }

            return Ok(Summaries[rnd.Next(Summaries.Length)]);
        }
    }
}