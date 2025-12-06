using MediatR;

namespace DataManager.Application.Contracts.Modules.TranslationsSet;

public class GetUploadedFilesQuery : IRequest<List<UploadedFileDto>>
{
    public Guid TranslationsSetId { get; set; }
}