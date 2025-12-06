using DataManager.Application.Contracts.Modules.TranslationsSet;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationsSet;

public class UploadTranslationFileCommandHandler : IRequestHandler<UploadTranslationFileCommand>
{
    public async Task Handle(UploadTranslationFileCommand request, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{request.TranslationsSetId}_{request.FileName}");

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.Content.CopyToAsync(stream, cancellationToken);
        }
    }
}