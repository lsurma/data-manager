using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSet;

public class ResolveDataSetQuery : IRequest<Core.Modules.DataSet.DataSet?>
{
    public string NameOrId { get; set; } = string.Empty;
}
