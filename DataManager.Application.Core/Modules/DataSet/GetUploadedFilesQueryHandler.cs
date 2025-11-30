using DataManager.Application.Contracts.Modules.DataSet;
using MediatR;

namespace DataManager.Application.Core.Modules.DataSet;

public class GetUploadedFilesQueryHandler : IRequestHandler<GetUploadedFilesQuery, List<UploadedFileDto>>
{
    public Task<List<UploadedFileDto>> Handle(GetUploadedFilesQuery request, CancellationToken cancellationToken)
    {
        var searchPattern = $"{request.DataSetId}_*";
        var files = Directory.GetFiles(Path.GetTempPath(), searchPattern)
            .Select(Path.GetFileName)
            .Select(fileName => new UploadedFileDto { FileName = fileName.Replace($"{request.DataSetId}_", "") })
            .ToList();

        return Task.FromResult(files);
    }
}