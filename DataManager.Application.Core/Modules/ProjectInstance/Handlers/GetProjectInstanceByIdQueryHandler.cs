using DataManager.Application.Contracts.Modules.ProjectInstance;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.ProjectInstance.Handlers;

public class GetProjectInstanceByIdQueryHandler : IRequestHandler<GetProjectInstanceByIdQuery, ProjectInstanceDto?>
{
    private readonly DataManagerDbContext _context;
    private readonly ProjectInstancesQueryService _queryService;

    public GetProjectInstanceByIdQueryHandler(DataManagerDbContext context, ProjectInstancesQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<ProjectInstanceDto?> Handle(GetProjectInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<ProjectInstance, Guid>
        {
            AsNoTracking = true
        };

        var instance = await _queryService.GetByIdAsync(
            request.Id,
            options: options,
            cancellationToken: cancellationToken
        );

        return instance?.ToDto();
    }
}
