using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Modules.Translations.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DataManager.Host.WA.Modules.Translations;

public partial class ExportTranslationsPanel
{
    [Inject]
    private IRequestSender RequestSender { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter]
    public ExportTranslationsModel Model { get; set; } = new();

    [Parameter]
    public List<IQueryFilter> Filters { get; set; } = new();

    private async Task OnExport()
    {
        var query = new ExportTranslationsQuery
        {
            Format = Model.ExportFormat.ToString().ToLower(),
            ExportType = Model.ExportType.ToString(),
            UseCurrentFilters = Model.UseCurrentFilters,
            BaseLanguage = Model.BaseLanguage,
            Filtering = new FilteringParameters
            {
                QueryFilters = Filters
            }
        };

        var stream = await RequestSender.SendAsync(query, CancellationToken.None);
        var fileName = $"translations.{Model.ExportFormat.ToString().ToLower()}";

        using var streamRef = new DotNetStreamReference(stream);
        await JsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
