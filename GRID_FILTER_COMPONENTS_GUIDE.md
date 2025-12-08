# Reusable Grid Filter Components - Usage Guide

This document explains how to use the new reusable filter components: `GridSelectFilter`, `GridTextFilter`, and `GridCheckFilter`.

## Overview

These components make it easy to add consistent, reusable filters to any grid page. They all follow the same pattern with common parameters like `Visible`, `OnValueChangedCallback`, and `Style`.

## GridSelectFilter

A dropdown/select filter for string-based options.

### Basic Usage

```razor
<GridSelectFilter Items="@AvailableOptions"
                  @bind-Value="@SelectedValue"
                  OnValueChangedCallback="@OnFilterChanged"
                  Placeholder="Select an option"
                  Visible="true"
                  Style="width: 250px;" />
```

### With Custom Template

```razor
<GridSelectFilter Items="@(new List<string> { "" }.Concat(Cultures).ToList())"
                  @bind-Value="@CultureFilter"
                  OnValueChangedCallback="@OnCultureFilterChanged"
                  Placeholder="All Cultures"
                  Visible="@IsFilterVisible("culture")"
                  Style="width: 250px;">
    <OptionTemplate Context="culture">
        @if (string.IsNullOrEmpty(culture))
        {
            <text>All Cultures</text>
        }
        else
        {
            <text>@culture</text>
        }
    </OptionTemplate>
</GridSelectFilter>
```

### Parameters

- **Items** (IEnumerable<string>) - List of options to display
- **Value** (string?) - Currently selected value (two-way bindable)
- **ValueChanged** (EventCallback<string?>) - Event when value changes
- **OnValueChangedCallback** (EventCallback) - Additional callback after value changes (for filter refresh logic)
- **OptionTemplate** (RenderFragment<string>?) - Optional custom template for rendering options
- **Placeholder** (string) - Placeholder text when no option is selected
- **Style** (string) - Custom CSS style (default: "width: 250px;")
- **Visible** (bool) - Whether the filter is visible (default: true)

### Code-Behind Example

```csharp
private string? CultureFilter { get; set; }
private List<string> Cultures { get; set; } = new();

protected override async Task OnInitializedAsync()
{
    Cultures = await LoadCulturesAsync();
}

private void OnCultureFilterChanged()
{
    // Rebuild query with new filter
    _currentQuery = BuildQuery();
    RefreshToken = Guid.NewGuid().ToString();
}

private bool IsFilterVisible(string filterId)
{
    return FilterSettings.Filters.FirstOrDefault(f => f.Id == filterId)?.Visible ?? false;
}
```

## GridTextFilter

A text input filter for free-text search.

### Basic Usage

```razor
<GridTextFilter @bind-Value="@ResourceNameFilter"
                OnValueChangedCallback="@OnResourceFilterChanged"
                Placeholder="Filter by resource name..."
                Visible="@IsFilterVisible("resource")"
                Style="width: 250px;" />
```

### With Label

```razor
<GridTextFilter @bind-Value="@SearchText"
                OnValueChangedCallback="@OnSearchChanged"
                Placeholder="Search..."
                Label="Search"
                Visible="true"
                Style="width: 300px;" />
```

### Parameters

- **Value** (string?) - Current text value (two-way bindable)
- **ValueChanged** (EventCallback<string?>) - Event when value changes
- **OnValueChangedCallback** (EventCallback) - Additional callback after value changes
- **Placeholder** (string) - Placeholder text
- **Label** (string?) - Optional label text
- **Style** (string) - Custom CSS style (default: "width: 250px;")
- **Visible** (bool) - Whether the filter is visible (default: true)

### Code-Behind Example

```csharp
private string? ResourceNameFilter { get; set; }

private void OnResourceFilterChanged()
{
    // Add text filter to query
    var filters = new List<IQueryFilter>();
    
    if (!string.IsNullOrWhiteSpace(ResourceNameFilter))
    {
        filters.Add(new ResourceNameFilter { Value = ResourceNameFilter });
    }
    
    _currentQuery.Filtering = new FilteringParameters { QueryFilters = filters };
    RefreshToken = Guid.NewGuid().ToString();
}
```

## GridCheckFilter

A two-state toggle button filter using FluentButton with Accent/Neutral appearance.

### Basic Usage

```razor
<GridCheckFilter @bind-IsChecked="@ShowDraftsOnly"
                 OnValueChangedCallback="@OnDraftFilterChanged"
                 Label="Show Drafts Only"
                 Visible="@IsFilterVisible("drafts")"
                 Style="" />
```

### With Different Labels for States

```razor
<GridCheckFilter @bind-IsChecked="@ShowPublished"
                 OnValueChangedCallback="@OnPublishedFilterChanged"
                 CheckedLabel="Published ✓"
                 UncheckedLabel="All Status"
                 Visible="true" />
```

### Parameters

- **IsChecked** (bool) - Whether the filter is active/checked (two-way bindable)
- **IsCheckedChanged** (EventCallback<bool>) - Event when checked state changes
- **OnValueChangedCallback** (EventCallback) - Additional callback after state changes
- **Label** (string) - Default label (used when CheckedLabel/UncheckedLabel not set)
- **CheckedLabel** (string?) - Label to show when checked
- **UncheckedLabel** (string?) - Label to show when unchecked
- **Style** (string) - Custom CSS style
- **Visible** (bool) - Whether the filter is visible (default: true)

### Code-Behind Example

```csharp
private bool ShowDraftsOnly { get; set; } = false;

private void OnDraftFilterChanged()
{
    // Add boolean filter to query
    var filters = new List<IQueryFilter>();
    
    if (ShowDraftsOnly)
    {
        filters.Add(new VersionStatusFilter { IncludeDraftVersions = true });
    }
    
    _currentQuery.Filtering = new FilteringParameters { QueryFilters = filters };
    RefreshToken = Guid.NewGuid().ToString();
}
```

## Complete Example: Multiple Filters

Here's a complete example showing multiple filters working together:

### Razor Markup

```razor
<PaginatedDataGrid TItem="TranslationDto"
                   Items="@AllTranslations"
                   TotalItems="@TotalItems"
                   PageSize="@PageSize"
                   SearchPlaceholder="Search translations..."
                   @bind-SearchTerm="@SearchTerm"
                   OnSearchChanged="@OnSearchChanged"
                   LoadData="@OnLoadData">
    
    <AdditionalFilters>
        <!-- Culture dropdown filter -->
        <GridSelectFilter Items="@(new List<string> { "" }.Concat(AvailableCultures).ToList())"
                          @bind-Value="@CultureFilter"
                          OnValueChangedCallback="@OnFiltersChanged"
                          Placeholder="All Cultures"
                          Visible="@IsFilterVisible("culture")"
                          Style="width: 200px;">
            <OptionTemplate Context="culture">
                @if (string.IsNullOrEmpty(culture))
                {
                    <text>All Cultures</text>
                }
                else
                {
                    @culture
                }
            </OptionTemplate>
        </GridSelectFilter>

        <!-- Resource name text filter -->
        <GridTextFilter @bind-Value="@ResourceFilter"
                        OnValueChangedCallback="@OnFiltersChanged"
                        Placeholder="Filter by resource..."
                        Visible="@IsFilterVisible("resource")"
                        Style="width: 200px;" />

        <!-- Draft status toggle filter -->
        <GridCheckFilter @bind-IsChecked="@ShowDraftsOnly"
                         OnValueChangedCallback="@OnFiltersChanged"
                         CheckedLabel="Drafts Only ✓"
                         UncheckedLabel="All Versions"
                         Visible="@IsFilterVisible("drafts")" />
    </AdditionalFilters>
    
    <Columns>
        <!-- Your columns here -->
    </Columns>
</PaginatedDataGrid>
```

### Code-Behind

```csharp
public partial class TranslationsGrid : ComponentBase
{
    private List<string> AvailableCultures { get; set; } = new();
    private string? CultureFilter { get; set; }
    private string? ResourceFilter { get; set; }
    private bool ShowDraftsOnly { get; set; }
    
    private AppFilterSettings FilterSettings { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Load available cultures
        AvailableCultures = await LoadCulturesAsync();
        
        // Load filter settings from local storage
        await LoadFilterSettingsAsync();
    }
    
    private void OnFiltersChanged()
    {
        // Build query with all active filters
        var filters = new List<IQueryFilter>();
        
        if (!string.IsNullOrWhiteSpace(CultureFilter))
        {
            filters.Add(new CultureNameFilter { Value = CultureFilter });
        }
        
        if (!string.IsNullOrWhiteSpace(ResourceFilter))
        {
            filters.Add(new ResourceNameFilter { Value = ResourceFilter });
        }
        
        if (ShowDraftsOnly)
        {
            filters.Add(new VersionStatusFilter { IncludeDraftVersions = true });
        }
        
        _currentQuery = new GetTranslationsQuery
        {
            Filtering = new FilteringParameters { QueryFilters = filters },
            Pagination = new PaginationParameters { Skip = 0, PageSize = PageSize }
        };
        
        // Trigger refresh
        RefreshToken = Guid.NewGuid().ToString();
    }
    
    private bool IsFilterVisible(string filterId)
    {
        return FilterSettings.Filters.FirstOrDefault(f => f.Id == filterId)?.Visible ?? false;
    }
    
    private async Task LoadFilterSettingsAsync()
    {
        var savedSettings = await LocalStorage.GetItemAsync<AppFilterSettings>("my-filter-settings");
        if (savedSettings != null)
        {
            FilterSettings = savedSettings;
        }
        else
        {
            // Initialize default settings
            FilterSettings = new AppFilterSettings
            {
                Filters = new List<FilterSettings>
                {
                    new FilterSettings { Id = "culture", Label = "Culture", Visible = true, OrderIndex = 0 },
                    new FilterSettings { Id = "resource", Label = "Resource", Visible = true, OrderIndex = 1 },
                    new FilterSettings { Id = "drafts", Label = "Drafts", Visible = false, OrderIndex = 2 }
                }
            };
        }
    }
}
```

## Integration with FilterPanel

The `Visible` parameter of each filter component is designed to work with the `FilterPanel` component:

1. Store filter visibility settings in `AppFilterSettings`
2. Use `IsFilterVisible(filterId)` to control filter visibility
3. Users can show/hide filters via the FilterPanel
4. Settings persist to local storage

## Best Practices

1. **Consistent Styling**: Use the same width for all filters in a group for visual consistency
2. **Clear Placeholders**: Use descriptive placeholders that indicate what the filter does
3. **Empty State**: For select filters, include an empty option (like "All Cultures") to clear the filter
4. **Combine Callbacks**: Use `OnValueChangedCallback` to trigger a single refresh method for all filters
5. **Filter Visibility**: Always respect the `IsFilterVisible()` check for user-controlled visibility
6. **Loading State**: Load filter options (like cultures) in `OnInitializedAsync` or `OnAfterRenderAsync`

## Advanced: Custom Templates

GridSelectFilter supports custom OptionTemplate for complex rendering:

```razor
<GridSelectFilter Items="@AvailableStatuses"
                  @bind-Value="@StatusFilter"
                  OnValueChangedCallback="@OnStatusFilterChanged"
                  Placeholder="All Statuses"
                  Visible="true">
    <OptionTemplate Context="status">
        <div style="display: flex; align-items: center; gap: 8px;">
            @if (status == "published")
            {
                <span style="color: green;">✓</span>
            }
            else if (status == "draft")
            {
                <span style="color: orange;">◐</span>
            }
            <span>@status</span>
        </div>
    </OptionTemplate>
</GridSelectFilter>
```

## Future Enhancements

Potential additions to these components:

1. **GridDateRangeFilter**: For date range filtering
2. **GridMultiSelectFilter**: For multiple selection
3. **GridNumericRangeFilter**: For min/max numeric filtering
4. **Generic GridSelectFilter<T>**: Support for non-string types (requires more complex implementation)

## Migration Guide

If you have existing inline filters, here's how to migrate:

### Before (Inline FluentSelect)

```razor
@if (IsFilterVisible("culture"))
{
    <FluentSelect TOption="string"
                  Items="@Cultures"
                  @bind-Value="@CultureFilter"
                  @bind-Value:after="OnCultureFilterChanged"
                  Placeholder="All Cultures"
                  Style="width: 250px;">
        <OptionTemplate Context="culture">
            @culture
        </OptionTemplate>
    </FluentSelect>
}
```

### After (GridSelectFilter)

```razor
<GridSelectFilter Items="@Cultures"
                  @bind-Value="@CultureFilter"
                  OnValueChangedCallback="@OnCultureFilterChanged"
                  Placeholder="All Cultures"
                  Visible="@IsFilterVisible("culture")"
                  Style="width: 250px;" />
```

**Benefits:**
- Less boilerplate code
- Consistent styling across filters
- Easier to read and maintain
- Visible property built-in
- Standardized callback pattern
