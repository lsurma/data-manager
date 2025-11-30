using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSet;

public class GetUploadedFilesQuery : IRequest<List<UploadedFileDto>>
{
    public Guid DataSetId { get; set; }
}