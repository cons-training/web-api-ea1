using GDB.App.Application.Services.Contracts;
using GDB.App.Infrastructure.Repositories.Contracts;

namespace GDB.App.Application.Services.Implementations
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _homeRepository;

        public HomeService(IHomeRepository homeRepository)
        {
            _homeRepository = homeRepository;
        }

        public string GetWelcomeMessage()
        {
            return _homeRepository.GetWelcomeMessage();
        }
    }
}
