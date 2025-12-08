using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Host.WA.Components.ContentEditor;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Translations;

public partial class TranslationPanel : IDialogContentComponent<TranslationPanelParameters>, IAsyncDisposable
{
    [Parameter]
    public TranslationPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [CascadingParameter]
    public AppDataContext? CascadingAppDataContext { get; set; }

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
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }

    private List<ContentEditorItem> ContentItems { get; set; } = new();

    private List<TranslationDto> RelatedTranslations { get; set; } = new();

    private Dictionary<string, string> TranslationContents { get; set; } = new();

    protected TranslationDto? Model { get; set; }
    
    protected bool IsEditMode { get; set; }
    
    protected DataSetDto DataSet { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await KeyboardShortcuts.RegisterSaveShortcutAsync(() => HandleSubmitAsync(closeAfterSave: false));
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            // If we have a translation ID, load the translation and related translations
            if (Content.TranslationId.HasValue)
            {
                IsEditMode = true;
                var result = await RequestSender.SendAsync(
                    new GetTranslationWithRelatedQuery(Content.TranslationId.Value)
                );

                Model = result.MainTranslation;
                RelatedTranslations = result.RelatedTranslations;

                if (Dialog != null)
                {
                    Dialog.Instance.Parameters.Title = $"Edit Translation - {Model.TranslationName}";
                    Dialog.TogglePrimaryActionButton(true);
                }
                
            }
            else
            {
                if(Content.DataSetId == null)
                {
                    throw new InvalidOperationException("DataSetId must be provided when creating a new translation.");
                }
                
                // Creating a new translation
                Model = new TranslationDto
                {
                    Id = Guid.NewGuid(),
                    IsCurrentVersion = true,
                    DataSetId = Content.DataSetId
                };
            }
            
            DataSet = CascadingAppDataContext?.DataSets.FirstOrDefault(x => x.Id == Content.DataSetId)!;

            if(DataSet == null)
            {
                throw new InvalidOperationException("Related DataSet not found in AppDataContext.");
            }

            // Build ContentItems from AvailableCultures
            BuildContentItems();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load translation data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void BuildContentItems()
    {
        ContentItems.Clear();
        TranslationContents.Clear();

        if (DataSet?.AvailableCultures == null || !DataSet.AvailableCultures.Any())
        {
            return;
        }

        foreach (var culture in DataSet.AvailableCultures.OrderBy(c => c))
        {
            // Find existing translation content for this culture
            var existingTranslation = RelatedTranslations.FirstOrDefault(t => t.CultureName == culture);
            var content = existingTranslation?.Content ?? string.Empty;

            // Store content in dictionary
            TranslationContents[culture] = content;

            // Create ContentEditorItem
            var item = new ContentEditorItem
            {
                Key = culture,
                Title = culture,
                Content = content,
                OnContentChanged = EventCallback.Factory.Create<string>(this, newContent =>
                {
                    HandleContentChanged(culture, newContent);
                })
            };

            ContentItems.Add(item);
        }
    }

    private void HandleContentChanged(string culture, string newContent)
    {
        TranslationContents[culture] = newContent;
    }
    
    
    // TODO - Email editor
    // private List<Option<Guid?>> LayoutSelectItems
    // {
    //     get
    //     {
    //         var items = new List<Option<Guid?>>
    //         {
    //             new Option<Guid?> { Value = null, Text = "-- None --" }
    //         };
    //         
    //         if (Content?.AvailableLayouts != null)
    //         {
    //             items.AddRange(Content.AvailableLayouts
    //                 .Where(t => t.Id != Content.Translation?.Id) // Exclude self
    //                 .Select(t => new Option<Guid?> 
    //                 { 
    //                     Value = t.Id, 
    //                     Text = $"{t.TranslationName} ({t.ResourceName})"
    //                 }));
    //         }
    //         
    //         return items;
    //     }
    // }
    
    private async Task HandleSubmitAsync(bool closeAfterSave = true)
    {
        if (Model == null)
        {
            ErrorMessage = "No translation data to save.";
            return;
        }
        
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var id = await RequestSender.SendAsync(new SaveTranslationCommand
            {
                Id = IsEditMode ? Model.Id : null,
                ResourceName = Model.ResourceName,
                TranslationName = Model.TranslationName,
                DataSetId = Model.DataSetId,
                Translations = TranslationContents
            });
            
            var successMessage = IsEditMode
                ? $"Translation '{Model.TranslationName}' updated successfully"
                : $"Translation '{Model.TranslationName}' created for all cultures successfully";

            ToastService.ShowSuccess(successMessage);
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            if (closeAfterSave)
            {
                await Dialog!.CloseAsync(DialogResult.Cancel(id));
            }
            
            Content.TranslationId = id;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to {(IsEditMode ? "update" : "create")} translation: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
        
        await InvokeAsync(StateHasChanged);
    }
    
    private async Task HandleCancelAsync()
    {
        await Dialog!.CancelAsync();
    }
    
    private async Task HandleDeleteClickAsync()
    {
        if(Model == null)
        {
            ErrorMessage = "Cannot delete a translation that hasn't been created yet.";
            return;
        }
        
        var dialog = await DialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete translation '{Model.TranslationName}'?",
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

            var command = new DeleteTranslationCommand { Id = Model.Id };
            await RequestSender.SendAsync(command);
            
            ToastService.ShowSuccess($"Translation '{Model.TranslationName}' deleted successfully");
            
            if (Content.OnDataChanged != null)
            {
                await Content.OnDataChanged.Invoke();
            }
            
            await Dialog!.CloseAsync(Model.Id);
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

    public async ValueTask DisposeAsync()
    {
        await KeyboardShortcuts.UnregisterAsync();
    }
}

public class TranslationPanelParameters
{
    /// <summary>
    /// The ID of the translation to edit. If null, a new translation will be created.
    /// </summary>
    public Guid? TranslationId { get; set; }

    public Guid? DataSetId { get; set; }
    
    public Func<Task>? OnDataChanged { get; set; }
}
