using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.TranslationSet;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.TranslationSets;

public partial class TranslationSetPanel : IDialogContentComponent<TranslationSetPanelParameters>, IAsyncDisposable
{
    [Parameter]
    public TranslationSetPanelParameters Content { get; set; } = null!;
    
    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }
    
    [Inject] 
    private IRequestSender RequestSender { get; set; } = null!;
    
    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private IKeyboardShortcutsService KeyboardShortcuts { get; set; } = null!;

    private bool IsSaving { get; set; }
    private bool IsDeleting { get; set; }
    private string? ErrorMessage { get; set; }
    private HashSet<Guid> SelectedIncludeIds { get; set; } = new();
    private string AllowedIdentityIdsText { get; set; } = string.Empty;
    private HashSet<string> SelectedCultures { get; set; } = new();
    private List<string> AvailableCultures { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        // Initialize selected includes from the TranslationSet
        if (Content?.TranslationSet?.IncludedTranslationSetIds != null)
        {
            SelectedIncludeIds = new HashSet<Guid>(Content.TranslationSet.IncludedTranslationSetIds);
        }

        // Initialize AllowedIdentityIds text from TranslationSet
        if (Content?.TranslationSet?.AllowedIdentityIds != null && Content.TranslationSet.AllowedIdentityIds.Any())
        {
            AllowedIdentityIdsText = string.Join(Environment.NewLine, Content.TranslationSet.AllowedIdentityIds);
        }

        // Load available cultures from the system
        try
        {
            AvailableCultures = await RequestSender.SendAsync<List<string>>(new GetAvailableCulturesQuery());
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to load available cultures. Please try again.";
            AvailableCultures = new List<string>();
        }

        // Initialize selected cultures from the TranslationSet
        if (Content?.TranslationSet?.AvailableCultures != null)
        {
            SelectedCultures = new HashSet<string>(Content.TranslationSet.AvailableCultures);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await KeyboardShortcuts.RegisterSaveShortcutAsync(() => HandleSubmitAsync(closeAfterSave: false));
        }
    }

    /// <summary>
    /// Parses the AllowedIdentityIdsText into a list of trimmed, non-empty identity IDs.
    /// Supports newlines, semicolons, and commas as separators.
    /// </summary>
    private List<string> ParseAllowedIdentityIds()
    {
        if (string.IsNullOrWhiteSpace(AllowedIdentityIdsText))
        {
            return new List<string>();
        }

        var separators = new[] { '\n', '\r', ';', ',' };

        return AllowedIdentityIdsText
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct()
            .ToList();
    }
    
    private void HandleIncludeChanged(Guid translationSetId, bool isSelected)
    {
        if (isSelected)
        {
            SelectedIncludeIds.Add(translationSetId);
        }
        else
        {
            SelectedIncludeIds.Remove(translationSetId);
        }
    }

    private void HandleCultureChanged(string cultureCode, bool isSelected)
    {
        if (isSelected)
        {
            SelectedCultures.Add(cultureCode);
        }
        else
        {
            SelectedCultures.Remove(cultureCode);
        }
    }

    private async Task HandleSubmitAsync(bool closeAfterSave = true)
    {
        if (Content?.TranslationSet == null) return;
        
        try
        {
            IsSaving = true;
            ErrorMessage = null;
            
            await RequestSender.SendAsync(new SaveTranslationSetCommand
            {
                Id = Content.IsEditMode ? Content.TranslationSet.Id : null,
                Name = Content.TranslationSet.Name,
                Description = Content.TranslationSet.Description,
                Notes = Content.TranslationSet.Notes,
                AllowedIdentityIds = ParseAllowedIdentityIds(),
                // Null means all cultures are available, empty list means none
                AvailableCultures = SelectedCultures.Any() ? SelectedCultures.ToList() : null,
                IncludedTranslationSetIds = SelectedIncludeIds.ToList()
            });
            
            ToastService.ShowSuccess($"Data Set '{Content.TranslationSet.Name}' {(Content.IsEditMode ? "updated" : "created")} successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            if (closeAfterSave)
            {
                await Dialog!.CloseAsync(DialogResult.Cancel(Content.TranslationSet));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to {(Content.IsEditMode ? "update" : "create")} data set: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    private async Task HandleCancelAsync()
    {
        await Dialog!.CancelAsync();
    }
    
    private async Task HandleDeleteClickAsync()
    {
        if (Content?.TranslationSet == null) return;
        
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete '{Content.TranslationSet.Name}'?",
            "Yes",
            "No",
            "Confirm");
        
        var result = await dialog.Result;
        
        if (result.Cancelled)
        {
            return;
        }
        
        try
        {
            IsDeleting = true;
            ErrorMessage = null;

            var command = new DeleteTranslationSetCommand { Id = Content.TranslationSet.Id };
            await RequestSender.SendAsync(command);
            
            ToastService.ShowSuccess($"Data Set '{Content.TranslationSet.Name}' deleted successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            await Dialog!.CloseAsync(Content.TranslationSet);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete data set: {ex.Message}";
        }
        finally
        {
            IsDeleting = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await KeyboardShortcuts.UnregisterAsync();
    }
}

public class TranslationSetPanelParameters
{
    public TranslationSetDto TranslationSet { get; set; } = null!;
    public bool IsEditMode { get; set; }
    public List<TranslationSetDto> AvailableTranslationSets { get; set; } = new();
    public Func<Task>? OnDataChanged { get; set; }
}
