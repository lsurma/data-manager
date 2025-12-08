using Microsoft.AspNetCore.Components;

namespace DataManager.Host.WA.Components;

/// <summary>
/// Reusable grid check filter component for two-state toggle button filters
/// </summary>
public partial class GridCheckFilter : ComponentBase
{
    /// <summary>
    /// Whether the filter is checked/active
    /// </summary>
    [Parameter]
    public bool IsChecked { get; set; }

    /// <summary>
    /// Event callback when checked state changes
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsCheckedChanged { get; set; }

    /// <summary>
    /// Event callback after checked state changes
    /// </summary>
    [Parameter]
    public EventCallback OnValueChangedCallback { get; set; }

    /// <summary>
    /// Default label (used when CheckedLabel/UncheckedLabel not set)
    /// </summary>
    [Parameter]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Label to show when checked
    /// </summary>
    [Parameter]
    public string? CheckedLabel { get; set; }

    /// <summary>
    /// Label to show when unchecked
    /// </summary>
    [Parameter]
    public string? UncheckedLabel { get; set; }

    /// <summary>
    /// Custom CSS style
    /// </summary>
    [Parameter]
    public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Whether the filter is visible
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    private async Task OnToggleAsync()
    {
        IsChecked = !IsChecked;

        if (IsCheckedChanged.HasDelegate)
        {
            await IsCheckedChanged.InvokeAsync(IsChecked);
        }

        if (OnValueChangedCallback.HasDelegate)
        {
            await OnValueChangedCallback.InvokeAsync();
        }
    }
}
