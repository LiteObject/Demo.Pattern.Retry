using Demo.Client.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client.Services
{
    public interface IUserService
    {
        public Task<List<User>> GetUsers(int count = 10);
    }
}
