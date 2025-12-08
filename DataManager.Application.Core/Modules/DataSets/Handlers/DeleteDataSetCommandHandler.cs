using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.DataSets.Handlers;

public class DeleteDataSetCommandHandler : IRequestHandler<DeleteDataSetCommand, bool>
{
    private readonly DataManagerDbContext _context;
    private readonly DataSetsQueryService _queryService;

    public DeleteDataSetCommandHandler(DataManagerDbContext context, DataSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<bool> Handle(DeleteDataSetCommand request, CancellationToken cancellationToken)
    {
        var dataSet = await _queryService.GetByIdAsync(
            request.Id,
            cancellationToken: cancellationToken
        );

        if (dataSet == null)
        {
            return false;
        }

        _context.DataSets.Remove(dataSet);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
