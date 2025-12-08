using System.Linq;
using Microsoft.AspNetCore.Components;

namespace DataManager.Host.WA.Components;

/// <summary>
/// Reusable grid select filter component for dropdown filters with string options
/// </summary>
public partial class GridSelectFilter : ComponentBase
{
    /// <summary>
    /// List of items to display in the dropdown
    /// </summary>
    [Parameter]
    public IEnumerable<string> Items { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// The currently selected value
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
    /// Optional custom template for rendering options
    /// </summary>
    [Parameter]
    public RenderFragment<string>? OptionTemplate { get; set; }

    /// <summary>
    /// Placeholder text when no value is selected
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

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

    /// <summary>
    /// Internal value property for two-way binding with FluentSelect
    /// </summary>
    private string? InternalValue
    {
        get => Value;
        set
        {
            if (Value != value)
            {
                Value = value;
                _ = OnValueChangedInternal();
            }
        }
    }

    private async Task OnValueChangedInternal()
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
