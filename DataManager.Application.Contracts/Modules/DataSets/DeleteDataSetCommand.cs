using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSets;

public class DeleteDataSetCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
