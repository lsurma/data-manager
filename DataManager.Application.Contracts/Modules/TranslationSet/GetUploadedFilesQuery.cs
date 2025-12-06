using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationSet;

public class GetUploadedFilesQuery : IRequest<List<UploadedFileDto>>
{
    public Guid TranslationSetId { get; set; }
}