using Microsoft.AspNetCore.Components;

namespace DataManager.Host.WA.Components.ContentEditor;

public class ContentEditorItem
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public EventCallback<string> OnContentChanged { get; set; }
}