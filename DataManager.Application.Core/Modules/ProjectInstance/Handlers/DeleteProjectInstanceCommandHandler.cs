using DataManager.Application.Contracts.Modules.ProjectInstance;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.ProjectInstance.Handlers;

public class DeleteProjectInstanceCommandHandler : IRequestHandler<DeleteProjectInstanceCommand, bool>
{
    private readonly DataManagerDbContext _context;
    private readonly ProjectInstancesQueryService _queryService;

    public DeleteProjectInstanceCommandHandler(DataManagerDbContext context, ProjectInstancesQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<bool> Handle(DeleteProjectInstanceCommand request, CancellationToken cancellationToken)
    {
        var instance = await _queryService.GetByIdAsync(
            request.Id,
            cancellationToken: cancellationToken
        );

        if (instance == null)
        {
            return false;
        }

        _context.ProjectInstances.Remove(instance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
