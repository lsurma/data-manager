using Microsoft.AspNetCore.Components;

namespace DataManager.Host.WA.Components;

/// <summary>
/// Reusable grid text filter component for text input filters
/// </summary>
public partial class GridTextFilter : ComponentBase
{
    /// <summary>
    /// The current text value
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Event callback when value changes
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Event callback after value changes
    /// </summary>
    [Parameter]
    public EventCallback OnValueChangedCallback { get; set; }

    /// <summary>
    /// Placeholder text
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Optional label text
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Custom CSS style
    /// </summary>
    [Parameter]
    public string Style { get; set; } = "width: 250px;";

    /// <summary>
    /// Whether the filter is visible
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    private async Task OnValueChanged()
    {
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(Value);
        }

        if (OnValueChangedCallback.HasDelegate)
        {
            await OnValueChangedCallback.InvokeAsync();
        }
    }
}
