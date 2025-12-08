using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using MediatR;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class GetAvailableCulturesQueryHandler : IRequestHandler<GetAvailableCulturesQuery, List<string>>
{
    private readonly ICultureService _cultureService;

    public GetAvailableCulturesQueryHandler(ICultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public Task<List<string>> Handle(GetAvailableCulturesQuery request, CancellationToken cancellationToken)
    {
        // Return predefined list of cultures from CultureService
        return Task.FromResult(_cultureService.GetAvailableCultures());
    }
}
