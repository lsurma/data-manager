using DataManager.Application.Contracts.Modules.DataSet;
using MediatR;

namespace DataManager.Application.Core.Modules.DataSet;

public class UploadTranslationFileCommandHandler : IRequestHandler<UploadTranslationFileCommand>
{
    public async Task Handle(UploadTranslationFileCommand request, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{request.DataSetId}_{request.FileName}");

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.Content.CopyToAsync(stream, cancellationToken);
        }
    }
}