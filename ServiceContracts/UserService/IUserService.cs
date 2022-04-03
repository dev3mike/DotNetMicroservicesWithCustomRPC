using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IUserService : IService
    {
        public Task<User> GetUserByIdAsync(int id);
    }
}
