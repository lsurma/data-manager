# Reusable Filter Components - Visual Summary

## Problem Solved

Previously, adding filters required duplicating the same FluentUI component markup with slight variations. This made the code harder to maintain and less consistent.

## Solution: Three Reusable Components

### 1. GridSelectFilter - Dropdown for String Options

**Visual Appearance:**
```
┌──────────────────────────┐
│ All Cultures         ▼   │  ← Dropdown with placeholder
└──────────────────────────┘
```

**When opened:**
```
┌──────────────────────────┐
│ All Cultures         ▼   │
├──────────────────────────┤
│ All Cultures             │  ← Empty option to clear filter
│ en-US                    │
│ fr-FR                    │
│ de-DE                    │
│ es-ES                    │
└──────────────────────────┘
```

**Code:**
```razor
<GridSelectFilter Items="@Cultures"
                  @bind-Value="@CultureFilter"
                  OnValueChangedCallback="@OnFilterChanged"
                  Placeholder="All Cultures"
                  Visible="true"
                  Style="width: 250px;" />
```

---

### 2. GridTextFilter - Text Input

**Visual Appearance:**
```
┌──────────────────────────────────────────┐
│ Filter by resource name...               │  ← Text input with placeholder
└──────────────────────────────────────────┘
```

**When user types:**
```
┌──────────────────────────────────────────┐
│ EmailTemplate█                           │  ← User's input
└──────────────────────────────────────────┘
```

**Code:**
```razor
<GridTextFilter @bind-Value="@ResourceFilter"
                OnValueChangedCallback="@OnFilterChanged"
                Placeholder="Filter by resource name..."
                Visible="true"
                Style="width: 250px;" />
```

---

### 3. GridCheckFilter - Two-State Toggle Button

**Visual Appearance (Unchecked):**
```
┌──────────────────┐
│  All Versions    │  ← Neutral appearance (gray)
└──────────────────┘
```

**Visual Appearance (Checked):**
```
┌──────────────────┐
│  Drafts Only ✓   │  ← Accent appearance (blue)
└──────────────────┘
```

**Code:**
```razor
<GridCheckFilter @bind-IsChecked="@ShowDraftsOnly"
                 OnValueChangedCallback="@OnFilterChanged"
                 CheckedLabel="Drafts Only ✓"
                 UncheckedLabel="All Versions"
                 Visible="true" />
```

---

## Complete Example: Multiple Filters Together

**Visual Layout:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Translations                                                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│ [Create] [Import] [Export]                                                  │
│                                                                              │
│ ┌──────────────┐ ┌─────────────────────┐ ┌──────────────┐  [Search...]  [⬚] [⚙]
│ │ en-US    ▼   │ │ Filter by resource..│ │ Drafts Only ✓│                │
│ └──────────────┘ └─────────────────────┘ └──────────────┘                 │
│    Culture          Resource Name           Status                          │
│    Filter           Filter                  Filter                          │
├─────────────────────────────────────────────────────────────────────────────┤
│ Culture │ Resource       │ Key           │ Content                         │
├─────────┼────────────────┼───────────────┼─────────────────────────────────┤
│ en-US   │ EmailTemplate  │ WelcomeEmail  │ Welcome to our platform...     │
│ en-US   │ EmailTemplate  │ ResetPassword │ Reset your password...         │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Code:**
```razor
<PaginatedDataGrid TItem="TranslationDto" Items="@Translations">
    <AdditionalFilters>
        <!-- Culture Select Filter -->
        <GridSelectFilter Items="@Cultures"
                          @bind-Value="@CultureFilter"
                          OnValueChangedCallback="@OnFiltersChanged"
                          Placeholder="All Cultures"
                          Visible="@IsFilterVisible("culture")"
                          Style="width: 150px;" />

        <!-- Resource Text Filter -->
        <GridTextFilter @bind-Value="@ResourceFilter"
                        OnValueChangedCallback="@OnFiltersChanged"
                        Placeholder="Filter by resource..."
                        Visible="@IsFilterVisible("resource")"
                        Style="width: 200px;" />

        <!-- Draft Status Toggle Filter -->
        <GridCheckFilter @bind-IsChecked="@ShowDraftsOnly"
                         OnValueChangedCallback="@OnFiltersChanged"
                         CheckedLabel="Drafts Only ✓"
                         UncheckedLabel="All Versions"
                         Visible="@IsFilterVisible("drafts")" />
    </AdditionalFilters>
    
    <Columns>
        <!-- Grid columns -->
    </Columns>
</PaginatedDataGrid>
```

---

## Before vs After Comparison

### BEFORE: Inline FluentSelect (Verbose)

```razor
@if (IsFilterVisible("culture"))
{
    <FluentSelect TOption="string"
                  Items="@(new List<string> { "" }.Concat(AvailableCultures).ToList())"
                  @bind-Value="@CultureNameFilter"
                  @bind-Value:after="OnCultureFilterChanged"
                  Placeholder="All Cultures"
                  Style="width: 250px;">
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
    </FluentSelect>
}
```
**Line count: 17 lines**

### AFTER: GridSelectFilter (Concise)

```razor
<GridSelectFilter Items="@(new List<string> { "" }.Concat(AvailableCultures).ToList())"
                  @bind-Value="@CultureNameFilter"
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
            @culture
        }
    </OptionTemplate>
</GridSelectFilter>
```
**Line count: 15 lines**

**Benefits:**
- ✅ No need to wrap in `@if` - Visible property handles it
- ✅ Consistent `OnValueChangedCallback` pattern across all filters
- ✅ Component name clearly indicates purpose
- ✅ Less nesting and easier to read

---

## FilterPanel Integration

The `Visible` property of each filter integrates seamlessly with the FilterPanel component:

**FilterPanel UI:**
```
┌─────────────────────────┐
│ Filter Settings    [×]  │
├─────────────────────────┤
│                         │
│ Filters                 │
│ Drag to reorder, check  │
│ to show/hide            │
│                         │
│ ☰ [✓] Culture           │ ← User can toggle visibility
│ ☰ [✓] Resource          │
│ ☰ [ ] Drafts            │ ← Hidden filter
│                         │
├─────────────────────────┤
│        [Save]  [Close]  │
└─────────────────────────┘
```

**Result:**
- When "Drafts" is unchecked, `GridCheckFilter` with `Visible="@IsFilterVisible("drafts")"` will not render
- Changes persist to local storage
- Users can customize which filters they see

---

## Key Advantages

### 1. **Consistency**
All filter components share the same API:
- `Visible` property
- `OnValueChangedCallback` event
- `Style` parameter
- Two-way data binding

### 2. **Reusability**
Add a new filter in just a few lines:
```razor
<GridSelectFilter Items="@StatusOptions"
                  @bind-Value="@StatusFilter"
                  OnValueChangedCallback="@OnFiltersChanged"
                  Placeholder="All Statuses"
                  Visible="true" />
```

### 3. **Maintainability**
- Changes to filter behavior only need to be made in one place
- Easier to understand and debug
- Less code duplication

### 4. **Flexibility**
- Custom templates supported (GridSelectFilter)
- Different labels for checked/unchecked states (GridCheckFilter)
- Optional labels for text filters

---

## Component Specifications

### GridSelectFilter
| Parameter | Type | Purpose |
|-----------|------|---------|
| Items | IEnumerable<string> | List of options |
| Value | string? | Selected value (bindable) |
| ValueChanged | EventCallback<string?> | Value change event |
| OnValueChangedCallback | EventCallback | Additional callback |
| OptionTemplate | RenderFragment<string>? | Custom option rendering |
| Placeholder | string | Placeholder text |
| Style | string | Custom CSS |
| Visible | bool | Visibility control |

### GridTextFilter
| Parameter | Type | Purpose |
|-----------|------|---------|
| Value | string? | Text value (bindable) |
| ValueChanged | EventCallback<string?> | Value change event |
| OnValueChangedCallback | EventCallback | Additional callback |
| Placeholder | string | Placeholder text |
| Label | string? | Optional label |
| Style | string | Custom CSS |
| Visible | bool | Visibility control |

### GridCheckFilter
| Parameter | Type | Purpose |
|-----------|------|---------|
| IsChecked | bool | Checked state (bindable) |
| IsCheckedChanged | EventCallback<bool> | State change event |
| OnValueChangedCallback | EventCallback | Additional callback |
| Label | string | Default label |
| CheckedLabel | string? | Label when checked |
| UncheckedLabel | string? | Label when unchecked |
| Style | string | Custom CSS |
| Visible | bool | Visibility control |

---

## Future Component Ideas

Based on the same pattern, these could be added:

1. **GridDateRangeFilter** - Start/end date selection
2. **GridNumericRangeFilter** - Min/max numeric values
3. **GridMultiSelectFilter** - Multiple option selection with chips
4. **GridSliderFilter** - Numeric slider for ranges
5. **GridRadioFilter** - Radio button group for mutually exclusive options

All would follow the same consistent API pattern!

---

## Migration Path

To convert existing inline filters to reusable components:

1. Identify the filter type (select, text, checkbox)
2. Choose the appropriate Grid*Filter component
3. Move the Visible check from @if to the Visible parameter
4. Replace @bind-Value:after with OnValueChangedCallback
5. Keep any custom templates (GridSelectFilter)
6. Test the filter behavior

**Result:** Cleaner, more maintainable filter code! ✨
