using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Components.ContentEditor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Translations;

public partial class TranslationPanel : IDialogContentComponent<TranslationPanelParameters>
{
    [Parameter]
    public TranslationPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [Inject]
    private IRequestSender RequestSender { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private bool IsSaving { get; set; }
    private bool IsDeleting { get; set; }
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }

    private IEnumerable<ContentEditorItem> ContentItems { get; set; } = [new() {Title = "title", Content = "content"}];

    private List<string> AvailableCultures { get; set; } = new();
    private List<TranslationDto> RelatedTranslations { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Load available cultures
            AvailableCultures = await RequestSender.SendAsync<List<string>>(new GetAvailableCulturesQuery());

            // If we have a translation ID, load the translation and related translations
            if (Content.TranslationId.HasValue)
            {
                var result = await RequestSender.SendAsync<TranslationWithRelatedDto>(
                    new GetTranslationWithRelatedQuery(Content.TranslationId.Value));

                Content.Translation = result.MainTranslation;
                RelatedTranslations = result.RelatedTranslations;
                Content.IsEditMode = true;
            }
            else
            {
                // Creating a new translation
                Content.Translation = new TranslationDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsCurrentVersion = true
                };
                Content.IsEditMode = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load translation data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private List<Option<Guid?>> DataSetSelectItems
    {
        get
        {
            var items = new List<Option<Guid?>>
            {
                new Option<Guid?> { Value = null, Text = "-- None --" }
            };
            
            if (Content?.AvailableDataSets != null)
            {
                items.AddRange(Content.AvailableDataSets
                    .Select(ds => new Option<Guid?> 
                    { 
                        Value = ds.Id, 
                        Text = ds.Name 
                    }));
            }
            
            return items;
        }
    }
    
    private List<Option<Guid?>> LayoutSelectItems
    {
        get
        {
            var items = new List<Option<Guid?>>
            {
                new Option<Guid?> { Value = null, Text = "-- None --" }
            };
            
            if (Content?.AvailableLayouts != null)
            {
                items.AddRange(Content.AvailableLayouts
                    .Where(t => t.Id != Content.Translation?.Id) // Exclude self
                    .Select(t => new Option<Guid?> 
                    { 
                        Value = t.Id, 
                        Text = $"{t.TranslationName} ({t.ResourceName})"
                    }));
            }
            
            return items;
        }
    }
    
    private async Task HandleKeyDownAsync(FluentKeyCodeEventArgs args)
    {
        // Ctrl+S to save
        if (args.CtrlKey && args.Key == KeyCode.KeyS)
        {
            await HandleSubmitAsync(closeAfterSave: false);
        }
    }
    
    private async Task HandleSubmitAsync(bool closeAfterSave = true)
    {
        if (Content?.Translation == null) return;
        
        try
        {
            IsSaving = true;
            ErrorMessage = null;
            
            await RequestSender.SendAsync(new SaveTranslationCommand
            {
                Id = Content.IsEditMode ? Content.Translation.Id : null,
                InternalGroupName1 = Content.Translation.InternalGroupName1,
                InternalGroupName2 = Content.Translation.InternalGroupName2,
                ResourceName = Content.Translation.ResourceName,
                TranslationName = Content.Translation.TranslationName,
                CultureName = Content.Translation.CultureName,
                Content = Content.Translation.Content,
                ContentTemplate = Content.Translation.ContentTemplate,
                DataSetId = Content.Translation.DataSetId,
                LayoutId = Content.Translation.LayoutId,
                SourceId = Content.Translation.SourceId
            });

            var successMessage = Content.IsEditMode
                ? $"Translation '{Content.Translation.TranslationName}' updated successfully"
                : $"Translation '{Content.Translation.TranslationName}' created for all cultures successfully";

            ToastService.ShowSuccess(successMessage);
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            if (closeAfterSave)
            {
                await Dialog!.CloseAsync(DialogResult.Cancel(Content.Translation));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to {(Content.IsEditMode ? "update" : "create")} translation: {ex.Message}";
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
        if (Content?.Translation == null) return;
        
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete translation '{Content.Translation.TranslationName}'?",
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

            var command = new DeleteTranslationCommand { Id = Content.Translation.Id };
            await RequestSender.SendAsync(command);
            
            ToastService.ShowSuccess($"Translation '{Content.Translation.TranslationName}' deleted successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            await Dialog!.CloseAsync(Content.Translation);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete translation: {ex.Message}";
        }
        finally
        {
            IsDeleting = false;
        }
    }
}

public class TranslationPanelParameters
{
    /// <summary>
    /// The ID of the translation to edit. If null, a new translation will be created.
    /// </summary>
    public Guid? TranslationId { get; set; }

    public TranslationDto Translation { get; set; } = null!;

    public bool IsEditMode { get; set; }

    public List<DataSetDto> AvailableDataSets { get; set; } = new();
    public List<TranslationDto> AvailableLayouts { get; set; } = new();
    public Func<Task>? OnDataChanged { get; set; }
}
