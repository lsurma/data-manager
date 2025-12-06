using DataManager.Application.Contracts.Modules.TranslationsSet;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationsSet;

public class GetUploadedFilesQueryHandler : IRequestHandler<GetUploadedFilesQuery, List<UploadedFileDto>>
{
    public Task<List<UploadedFileDto>> Handle(GetUploadedFilesQuery request, CancellationToken cancellationToken)
    {
        var searchPattern = $"{request.TranslationsSetId}_*";
        var files = Directory.GetFiles(Path.GetTempPath(), searchPattern)
            .Select(Path.GetFileName)
            .Select(fileName => new UploadedFileDto { FileName = fileName.Replace($"{request.TranslationsSetId}_", "") })
            .ToList();

        return Task.FromResult(files);
    }
}