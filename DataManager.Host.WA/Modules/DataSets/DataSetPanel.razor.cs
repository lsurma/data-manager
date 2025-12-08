using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.DataSets;

public partial class DataSetPanel : IDialogContentComponent<DataSetPanelParameters>, IAsyncDisposable
{
    [Parameter]
    public DataSetPanelParameters Content { get; set; } = null!;
    
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
    private IEnumerable<DataSetDto> SelectedIncludedDataSets { get; set; } = new List<DataSetDto>();
    private string AllowedIdentityIdsText { get; set; } = string.Empty;
    private string WebhookUrlsText { get; set; } = string.Empty;
    private IEnumerable<string> SelectedCultures { get; set; } = new List<string>();
    private List<string> AvailableCultures { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        // Initialize selected included DataSets from the DataSet
        if (Content?.DataSet?.IncludedDataSetIds != null && Content.DataSet.IncludedDataSetIds.Any())
        {
            var selectedIds = Content.DataSet.IncludedDataSetIds.ToHashSet();
            SelectedIncludedDataSets = Content.AvailableDataSets
                .Where(ts => selectedIds.Contains(ts.Id))
                .ToList();
        }

        // Initialize AllowedIdentityIds text from DataSet
        if (Content?.DataSet?.AllowedIdentityIds != null && Content.DataSet.AllowedIdentityIds.Any())
        {
            AllowedIdentityIdsText = string.Join(Environment.NewLine, Content.DataSet.AllowedIdentityIds);
        }

        // Initialize WebhookUrls text from DataSet
        if (Content?.DataSet?.WebhookUrls != null && Content.DataSet.WebhookUrls.Any())
        {
            WebhookUrlsText = string.Join(Environment.NewLine, Content.DataSet.WebhookUrls);
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

        // Initialize selected cultures from the DataSet
        if (Content?.DataSet?.AvailableCultures.Any() == true)
        {
            SelectedCultures = Content.DataSet.AvailableCultures.ToList();
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

    /// <summary>
    /// Parses the WebhookUrlsText into a list of trimmed URLs.
    /// Supports newlines, semicolons, and commas as separators.
    /// Note: URL format validation is performed in the handler.
    /// </summary>
    private List<string> ParseWebhookUrls()
    {
        if (string.IsNullOrWhiteSpace(WebhookUrlsText))
        {
            return new List<string>();
        }

        var separators = new[] { '\n', '\r', ';', ',' };

        return WebhookUrlsText
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url.Trim())
            .Distinct()
            .ToList();
    }

    private async Task HandleSubmitAsync(bool closeAfterSave = true)
    {
        if (Content?.DataSet == null) return;
        
        try
        {
            IsSaving = true;
            ErrorMessage = null;
            
            await RequestSender.SendAsync(new SaveDataSetCommand
            {
                Id = Content.IsEditMode ? Content.DataSet.Id : null,
                Name = Content.DataSet.Name,
                Description = Content.DataSet.Description,
                Notes = Content.DataSet.Notes,
                AllowedIdentityIds = ParseAllowedIdentityIds(),
                // Empty list means all cultures are available
                AvailableCultures = SelectedCultures.ToList(),
                SecretKey = Content.DataSet.SecretKey,
                WebhookUrls = ParseWebhookUrls(),
                IncludedDataSetIds = SelectedIncludedDataSets.Select(ts => ts.Id).ToList()
            });
            
            ToastService.ShowSuccess($"Data Set '{Content.DataSet.Name}' {(Content.IsEditMode ? "updated" : "created")} successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            if (closeAfterSave)
            {
                await Dialog!.CloseAsync(DialogResult.Cancel(Content.DataSet));
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
        if (Content?.DataSet == null) return;
        
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete '{Content.DataSet.Name}'?",
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

            var command = new DeleteDataSetCommand { Id = Content.DataSet.Id };
            await RequestSender.SendAsync(command);
            
            ToastService.ShowSuccess($"Data Set '{Content.DataSet.Name}' deleted successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            await Dialog!.CloseAsync(Content.DataSet);
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

public class DataSetPanelParameters
{
    public DataSetDto DataSet { get; set; } = null!;
    public bool IsEditMode { get; set; }
    public List<DataSetDto> AvailableDataSets { get; set; } = new();
    public Func<Task>? OnDataChanged { get; set; }
}
