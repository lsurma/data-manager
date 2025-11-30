using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSet;

public class DeleteDataSetCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
