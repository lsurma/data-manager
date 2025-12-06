using DataManager.Application.Contracts.Modules.TranslationSet;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationSet;

public class UploadTranslationFileCommandHandler : IRequestHandler<UploadTranslationFileCommand>
{
    public async Task Handle(UploadTranslationFileCommand request, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{request.TranslationSetId}_{request.FileName}");

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.Content.CopyToAsync(stream, cancellationToken);
        }
    }
}