using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BlazorMonaco.Editor;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Mjml;
using DataManager.Application.Contracts.Modules.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace DataManager.Host.WA.Modules.Emails;

public partial class EmailEditorPanel : IDialogContentComponent<EmailEditorPanelParameters>, IDisposable
{
    [Parameter]
    public EmailEditorPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private ILogger<EmailEditorPanel> Logger { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private StandaloneCodeEditor? _mjmlEditor = null!;
    private StandaloneCodeEditor? _variablesEditor = null!;
    private StandaloneCodeEditor? _htmlOutputEditor = null!;

    private bool IsSaving { get; set; }
    private List<TranslationDto> AvailableTemplates { get; set; } = new();
    private TranslationDto? SelectedTemplate { get; set; }
    private List<string> AvailableCultures { get; set; } = new();
    private string SelectedCulture { get; set; } = "en-US";

    // Translation data per culture
    private Dictionary<string, TranslationData> TranslationsByCulture { get; set; } = new();

    private string _previewWidth = "100%";
    private bool _showHtmlOutput = false;
    private string _htmlOutput = string.Empty;
    private Timer? _debounceTimer;
    private const int DebounceTimeMs = 500;

    private class TranslationData
    {
        public Guid? TranslationId { get; set; }
        public string MjmlContent { get; set; } = string.Empty;
        public string Variables { get; set; } = @"{
  ""name"": ""User""
}";
    }

    protected override async Task OnInitializedAsync()
    {
        _debounceTimer = new Timer(async _ => await OnTimerCallback(), null, Timeout.Infinite, Timeout.Infinite);

        // Load available cultures
        await LoadAvailableCulturesAsync();

        // Load available templates (email translations that can be used as templates)
        await LoadAvailableTemplatesAsync();

        // Initialize translation data for each culture
        foreach (var culture in AvailableCultures)
        {
            TranslationsByCulture[culture] = new TranslationData();
        }

        // Load existing translation if TranslationId is provided
        if (Content.TranslationId.HasValue)
        {
            await LoadExistingTranslationAsync(Content.TranslationId.Value);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initial preview render
            await Task.Delay(100); // Give editors time to render
            await OnTimerCallback();
        }
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
    }

    private async Task LoadAvailableCulturesAsync()
    {
        try
        {
            var query = new GetAvailableCulturesQuery();
            AvailableCultures = await RequestSender.SendAsync(query);

            if (AvailableCultures.Any())
            {
                SelectedCulture = AvailableCultures.First();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load available cultures");
            ToastService.ShowError("Failed to load available cultures");
        }
    }

    private async Task LoadAvailableTemplatesAsync()
    {
        try
        {
            // Load all email translations that can be used as templates
            var query = GetTranslationsQuery.AllItems();
            query.Filtering = new FilteringParameters
            {
                QueryFilters = new List<IQueryFilter>
                {
                    new InternalGroupName1Filter { Value = "Email" }
                }
            };

            var result = await RequestSender.SendAsync(query);
            AvailableTemplates = result.Items;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load available templates");
            ToastService.ShowError("Failed to load available templates");
        }
    }

    private async Task LoadExistingTranslationAsync(Guid translationId)
    {
        try
        {
            var query = new GetTranslationWithRelatedQuery(translationId);
            var translation = await RequestSender.SendAsync(query);

            if (translation != null)
            {
                var culture = translation.MainTranslation.CultureName ?? "en-US";
                SelectedCulture = culture;

                if (!TranslationsByCulture.ContainsKey(culture))
                {
                    TranslationsByCulture[culture] = new TranslationData();
                }

                var translationData = TranslationsByCulture[culture];
                translationData.TranslationId = translation.MainTranslation.Id;
                translationData.MjmlContent = translation.MainTranslation.ContentTemplate ?? translation.MainTranslation.Content;

                // Load related translations (other cultures)
                if (translation.RelatedTranslations != null)
                {
                    foreach (var related in translation.RelatedTranslations)
                    {
                        var relatedCulture = related.CultureName ?? "en-US";
                        if (!TranslationsByCulture.ContainsKey(relatedCulture))
                        {
                            TranslationsByCulture[relatedCulture] = new TranslationData();
                        }

                        TranslationsByCulture[relatedCulture].TranslationId = related.Id;
                        TranslationsByCulture[relatedCulture].MjmlContent = related.ContentTemplate ?? related.Content;
                    }
                }

                // Set initial editor values
                if (_mjmlEditor != null)
                {
                    await _mjmlEditor.SetValue(translationData.MjmlContent);
                }
                if (_variablesEditor != null)
                {
                    await _variablesEditor.SetValue(translationData.Variables);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load existing translation");
            ToastService.ShowError("Failed to load existing translation");
        }
    }

    private async Task OnCultureChanged(string culture)
    {
        // Save current culture's editor content
        if (_mjmlEditor != null && _variablesEditor != null)
        {
            var currentData = TranslationsByCulture[SelectedCulture];
            currentData.MjmlContent = await _mjmlEditor.GetValue();
            currentData.Variables = await _variablesEditor.GetValue();
        }

        // Switch to new culture
        SelectedCulture = culture;

        // Load new culture's content
        var newData = TranslationsByCulture[SelectedCulture];
        if (_mjmlEditor != null)
        {
            await _mjmlEditor.SetValue(newData.MjmlContent);
        }
        if (_variablesEditor != null)
        {
            await _variablesEditor.SetValue(newData.Variables);
        }

        // Trigger preview update
        _debounceTimer?.Change(DebounceTimeMs, Timeout.Infinite);
        await InvokeAsync(StateHasChanged);
    }

    private void SetPreviewSize(string width)
    {
        _previewWidth = width;
        _showHtmlOutput = false;
        StateHasChanged();
    }

    private async Task ToggleHtmlOutput()
    {
        _showHtmlOutput = !_showHtmlOutput;

        if (_showHtmlOutput && _htmlOutputEditor != null)
        {
            await Task.Delay(100); // Give the editor time to render
            await _htmlOutputEditor.SetValue(_htmlOutput);
        }

        await InvokeAsync(StateHasChanged);
    }

    private StandaloneEditorConstructionOptions MjmlEditorConstructionOptions(StandaloneCodeEditor editor)
    {
        var initialContent = TranslationsByCulture.ContainsKey(SelectedCulture)
            ? TranslationsByCulture[SelectedCulture].MjmlContent
            : @"<mjml>
  <mj-body>
    <mj-section>
      <mj-column>
        <mj-text>
          Hello {{ name }}!
        </mj-text>
      </mj-column>
    </mj-section>
  </mj-body>
</mjml>";

        return new StandaloneEditorConstructionOptions
        {
            Language = "html",
            Value = initialContent,
            AutomaticLayout = true,
            WordWrap = "on"
        };
    }

    private StandaloneEditorConstructionOptions VariablesEditorConstructionOptions(StandaloneCodeEditor editor)
    {
        var initialContent = TranslationsByCulture.ContainsKey(SelectedCulture)
            ? TranslationsByCulture[SelectedCulture].Variables
            : @"{
  ""name"": ""User""
}";

        return new StandaloneEditorConstructionOptions
        {
            Language = "json",
            Value = initialContent,
            AutomaticLayout = true,
            WordWrap = "on"
        };
    }

    private StandaloneEditorConstructionOptions HtmlOutputEditorConstructionOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            Language = "html",
            Value = _htmlOutput,
            ReadOnly = true,
            AutomaticLayout = true,
            WordWrap = "on"
        };
    }

    private Task OnMjmlContentChanged()
    {
        _debounceTimer?.Change(DebounceTimeMs, Timeout.Infinite);
        return Task.CompletedTask;
    }

    private Task OnVariablesContentChanged()
    {
        _debounceTimer?.Change(DebounceTimeMs, Timeout.Infinite);
        return Task.CompletedTask;
    }

    private async Task OnTimerCallback()
    {
        if (_mjmlEditor is null || _variablesEditor is null)
        {
            return;
        }

        var mjml = await _mjmlEditor.GetValue();
        var variables = await _variablesEditor.GetValue();
        await ConvertMjmlToHtml(mjml, variables);
    }

    private async Task ConvertMjmlToHtml(string mjml, string variables)
    {
        try
        {
            var mjmlResult = await JSRuntime.InvokeAsync<MjmlResult>("mjml", mjml);

            if (mjmlResult.Errors.Length > 0)
            {
                var errorString = string.Join(", ", mjmlResult.Errors.Select(e => e.ToString()));
                _htmlOutput = $"<p>MJML Errors: {errorString}</p>";
            }
            else
            {
                var command = new RenderTemplateCommand
                {
                    Html = mjmlResult.Html,
                    Variables = variables
                };

                var result = await RequestSender.SendAsync<RenderedTemplateDto>(command);
                _htmlOutput = result.Html;
            }
        }
        catch (ApiErrorException apiEx)
        {
            // Display detailed API error information
            var errorHtml = $"<div style='padding: 20px; background-color: #fee; border: 1px solid #f00; border-radius: 4px;'>";
            errorHtml += $"<h4 style='color: #c00; margin-top: 0;'>Error</h4>";
            errorHtml += $"<p><strong>Message:</strong> {System.Net.WebUtility.HtmlEncode(apiEx.Error)}</p>";

            if (!string.IsNullOrWhiteSpace(apiEx.Details))
            {
                errorHtml += $"<p><strong>Details:</strong> {System.Net.WebUtility.HtmlEncode(apiEx.Details)}</p>";
            }

            if (apiEx.StatusCode > 0)
            {
                errorHtml += $"<p><strong>Status Code:</strong> {apiEx.StatusCode}</p>";
            }

            errorHtml += "</div>";
            _htmlOutput = errorHtml;
        }
        catch (Exception ex)
        {
            _htmlOutput = $"<p>Error converting MJML: {ex.Message}</p>";
        }

        // Update HTML output editor if visible
        if (_showHtmlOutput && _htmlOutputEditor != null)
        {
            await _htmlOutputEditor.SetValue(_htmlOutput);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleSaveAsync()
    {
        IsSaving = true;
        StateHasChanged();

        try
        {
            // Save current culture's editor content first
            if (_mjmlEditor != null && _variablesEditor != null)
            {
                var currentData = TranslationsByCulture[SelectedCulture];
                currentData.MjmlContent = await _mjmlEditor.GetValue();
                currentData.Variables = await _variablesEditor.GetValue();
            }

            // Save all translations
            foreach (var kvp in TranslationsByCulture)
            {
                var culture = kvp.Key;
                var data = kvp.Value;

                if (string.IsNullOrWhiteSpace(data.MjmlContent))
                {
                    continue; // Skip empty translations
                }

                var command = new SaveSingleTranslationCommand
                {
                    Id = data.TranslationId,
                    DataSetId = Optional<Guid?>.Of(Content.TranslationSetId),
                    CultureName = Optional<string?>.Of(culture),
                    ContentTemplate = Optional<string?>.Of(data.MjmlContent),
                    InternalGroupName1 = Optional<string?>.Of("Email"),
                    ResourceName = Optional<string>.Of(Content.ResourceName ?? "EmailTemplate"),
                    TranslationName = Optional<string>.Of(Content.TranslationName ?? "Email"),
                    LayoutId = Optional<Guid?>.Of(SelectedTemplate?.Id)
                };

                var result = await RequestSender.SendAsync(command);

                // Update the translation ID if it was a new translation
                if (!data.TranslationId.HasValue)
                {
                    data.TranslationId = result;
                }
            }

            ToastService.ShowSuccess("Email translations saved successfully");

            // Notify parent of data change
            Content.OnDataChanged?.Invoke();

            // Close the panel
            await Dialog!.CloseAsync(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save email translations");
            ToastService.ShowError($"Failed to save: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task HandleCancelAsync()
    {
        await Dialog!.CancelAsync();
    }

    public class MjmlResult
    {
        [JsonPropertyName("html")]
        public string Html { get; set; } = string.Empty;
        [JsonPropertyName("errors")]
        public object[] Errors { get; set; } = Array.Empty<object>();
    }
}

public class EmailEditorPanelParameters
{
    public Guid? TranslationId { get; set; }
    public Guid? TranslationSetId { get; set; }
    public string? ResourceName { get; set; }
    public string? TranslationName { get; set; }
    public Action? OnDataChanged { get; set; }
}
