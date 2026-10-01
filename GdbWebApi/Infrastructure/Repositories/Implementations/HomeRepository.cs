using GDB.App.Infrastructure.Repositories.Contracts;

namespace GDB.App.Infrastructure.Repositories.Implementations
{
    public class HomeRepository : IHomeRepository
    {
        public string GetWelcomeMessage()
        {
            return "Welcome to GDB Web API!";
        }
    }
}
