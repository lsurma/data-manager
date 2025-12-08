using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSets;

public class GetDataSetByIdQuery : IRequest<DataSetDto?>
{
    public Guid Id { get; set; }
}
