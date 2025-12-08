using DataManager.AI.Core.Services;
using DataManager.Application.Contracts.Modules.AI;
using MediatR;

namespace DataManager.AI.Core.Handlers;

/// <summary>
/// Handler for TranslateTextCommand that uses OpenRouter AI service.
/// </summary>
public class TranslateTextCommandHandler : IRequestHandler<TranslateTextCommand, TranslateTextResult>
{
    private readonly IOpenRouterService _openRouterService;

    public TranslateTextCommandHandler(IOpenRouterService openRouterService)
    {
        _openRouterService = openRouterService;
    }

    public async Task<TranslateTextResult> Handle(TranslateTextCommand request, CancellationToken cancellationToken)
    {
        // Use context "ecommerce" by default if not specified
        var context = request.Context ?? "ecommerce";

        var translations = await _openRouterService.TranslateTextAsync(
            request.Text,
            request.SourceCulture,
            request.TargetCultures,
            context,
            request.Model,
            cancellationToken);

        return new TranslateTextResult
        {
            Translations = translations
        };
    }
}
