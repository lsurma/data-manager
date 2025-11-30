using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSet;

public class GetDataSetByIdQuery : IRequest<DataSetDto?>
{
    public Guid Id { get; set; }
}
