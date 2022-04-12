using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client.Services
{
    public interface IWeatherService
    {
        public Task<string> GetForecast();
    }
}
