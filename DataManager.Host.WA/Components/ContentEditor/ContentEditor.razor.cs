using Microsoft.AspNetCore.Components;
using DataManager.Host.WA.Services;

namespace DataManager.Host.WA.Components.ContentEditor;

public partial class ContentEditor : ComponentBase
{
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    private const string EditorTypeStorageKey = "ContentEditor_SelectedEditorType";

    protected override async Task OnInitializedAsync()
    {
        // Load saved editor type from local storage
        if (!EditorType.HasValue)
        {
            var savedType = await LocalStorage.GetItemAsync<ContentEditorType?>(EditorTypeStorageKey);
            if (savedType.HasValue)
            {
                EditorTypeInternal = savedType.Value;
            }
        }
    }

    private async Task SaveEditorTypeAsync(ContentEditorType type)
    {
        await LocalStorage.SetItemAsync(EditorTypeStorageKey, type);
    }
}