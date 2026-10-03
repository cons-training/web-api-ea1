using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Infrastructure.Repositories.Contracts;

namespace GdbWebApi.Application.Services.Implementations
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
