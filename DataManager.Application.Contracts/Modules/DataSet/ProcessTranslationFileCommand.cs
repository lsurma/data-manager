using MediatR;

namespace DataManager.Application.Contracts.Modules.DataSet;

public class ProcessTranslationFileCommand : IRequest
{
    public Guid DataSetId { get; set; }
    public string? FileName { get; set; }
}