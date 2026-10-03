using GdbWebApi.Infrastructure.Repositories.Contracts;

namespace GdbWebApi.Infrastructure.Repositories.Implementations
{
    public class HomeRepository : IHomeRepository
    {
        public string GetWelcomeMessage()
        {
            return "Welcome to GDB Web API!";
        }
    }
}
