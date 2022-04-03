using ServiceContracts;
using System.Threading.Tasks;

namespace UserService
{
    public class UserService : IUserService
    {
        public async Task<User> GetUserByIdAsync(int id)
        {
            await Task.Delay(1000);

            return new User
            {
                Id = System.Guid.NewGuid(),
                FirstName = "Jack",
                LastName = "Sheperd",
                Mobile = "0730000000"
            };
        }
    }
}
