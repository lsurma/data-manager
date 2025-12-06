using DataManager.Application.Contracts.Modules.TranslationSet;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationSet;

public class GetUploadedFilesQueryHandler : IRequestHandler<GetUploadedFilesQuery, List<UploadedFileDto>>
{
    public Task<List<UploadedFileDto>> Handle(GetUploadedFilesQuery request, CancellationToken cancellationToken)
    {
        var searchPattern = $"{request.TranslationSetId}_*";
        var files = Directory.GetFiles(Path.GetTempPath(), searchPattern)
            .Select(Path.GetFileName)
            .Select(fileName => new UploadedFileDto { FileName = fileName.Replace($"{request.TranslationSetId}_", "") })
            .ToList();

        return Task.FromResult(files);
    }
}