using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSet;

public class UploadTranslationFileCommand : IRequest
{
    public Guid DataSetId { get; set; }
    public string? FileName { get; set; }
    public Stream? Content { get; set; }
}