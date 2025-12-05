using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Components;

/// <summary>
///     Partial class containing all settings-related functionality for PaginatedDataGrid
/// </summary>
public partial class PaginatedDataGrid<TItem>
{
    [Parameter]
    public string? SettingsStorageKey { get; set; }

    private AppDataGridSettings? GridSettings { get; set; }

    protected DataGridSettings DataGridSettings { get; set; } = new DataGridSettings();

    /// <summary>
    ///     Opens the settings panel for column customization
    /// </summary>
    private async Task OpenSettingsPanelAsync()
    {
        // Create a copy of column states to allow canceling changes
        var settings = GridSettings ?? new AppDataGridSettings();
        settings = settings with
        {
            Columns = []
        };

        foreach (var column in _dataGrid.ColumnsCollection)
        {
            var existingColumnState = GridSettings?.Columns.FirstOrDefault(c => c.UniqueID == column.UniqueID);

            settings.Columns.Add(new ColumnSettings
            {
                Title = column.Title,
                UniqueID = column.UniqueID,
                Visible = existingColumnState?.Visible ?? column.Visible,
                OrderIndex = existingColumnState?.OrderIndex ?? column.OrderIndex ?? 0,
                Width = existingColumnState?.Width ?? column.Width
            });
        }
        

        var parameters = new DataGridSettingsPanelParameters
        {
            Columns = settings.Columns
        };

        var dialog = await DialogService.ShowPanelAsync<DataGridSettingsPanel>(parameters, new DialogParameters
        {
            Title = "Grid Settings",
            Width = "400px",
            TrapFocus = false,
            Modal = false,
            Id = $"settings-panel-{Guid.NewGuid()}"
        });

        var result = await dialog.Result;

        // Only apply changes if saved (not cancelled)
        if (!result.Cancelled && result.Data is List<ColumnSettings> updatedColumns)
        {
            GridSettings ??= new AppDataGridSettings();
            GridSettings.Columns = updatedColumns;

            // Apply changes to DataGrid via settings
            await HandleSettingsChanged();

            // Save settings to local storage
            await SaveSettingsAsync();
        }
    }

    /// <summary>
    ///     Handles DataGrid settings changed event from Radzen
    /// </summary>
    private Task SettingsChanged(DataGridSettings arg)
    {
        DataGridSettings = arg;

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Applies column state changes to the DataGrid settings
    /// </summary>
    private async Task HandleSettingsChanged()
    {
        // Create a new DataGridSettings instance by copying from original
        var newSettings = new DataGridSettings();

        // Copy existing column settings to preserve properties like Width
        var newColumns = new List<DataGridColumnSettings>();

        if (DataGridSettings.Columns != null)
        {
            foreach (var existingCol in DataGridSettings.Columns)
            {
                // Create a copy of each column settings
                var newCol = new DataGridColumnSettings
                {
                    UniqueID = existingCol.UniqueID,
                    Visible = existingCol.Visible,
                    OrderIndex = existingCol.OrderIndex,
                    Width = existingCol.Width
                };

                // Override with our saved width if available
                var appColumn = GridSettings.Columns.FirstOrDefault(c => c.UniqueID == existingCol.UniqueID);
                if (appColumn != null && !String.IsNullOrEmpty(appColumn.Width))
                {
                    newCol.Width = appColumn.Width;
                }

                newColumns.Add(newCol);
            }
        }

        // Update only our managed properties (Visible and OrderIndex)
        foreach (var columnState in GridSettings.Columns)
        {
            var existingColumn = newColumns.FirstOrDefault(c => c.UniqueID == columnState.UniqueID);

            if (existingColumn != null)
            {
                // Column already exists in settings, skip it - we'll update via grid column
                continue;
            }

            // Find the actual grid column
            var gridColumn = _dataGrid.ColumnsCollection.FirstOrDefault(c => c.UniqueID == columnState.UniqueID);

            if (gridColumn != null)
            {
                // Create settings from the grid column, prefer saved width
                newColumns.Add(new DataGridColumnSettings
                {
                    UniqueID = columnState.UniqueID,
                    Visible = columnState.Visible,
                    OrderIndex = columnState.OrderIndex,
                    Width = !String.IsNullOrEmpty(columnState.Width) ? columnState.Width : gridColumn.Width
                });
            }
        }

        // Now update the column settings with our states
        foreach (var columnState in GridSettings.Columns)
        {
            var columnSettings = newColumns.FirstOrDefault(c => c.UniqueID == columnState.UniqueID);
            if (columnSettings != null)
            {
                columnSettings.Visible = columnState.Visible;
                columnSettings.OrderIndex = columnState.OrderIndex;

                // Apply saved width if available
                if (!String.IsNullOrEmpty(columnState.Width))
                {
                    columnSettings.Width = columnState.Width;
                }
            }
        }

        newSettings.Columns = newColumns;

        // Replace the settings to trigger grid refresh
        DataGridSettings = newSettings;

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    ///     Loads settings when the DataGrid requests them
    /// </summary>
    private void LoadRadzenDataGridSettings(DataGridLoadSettingsEventArgs obj)
    {
        if (GridSettings == null)
        {
            return;
        }

        var columnsToSet = (obj.Settings?.Columns ?? []).ToList();
        
        foreach (var columnAppSettings in GridSettings.Columns)
        {
            var gridColumn = columnsToSet.FirstOrDefault(c => c.UniqueID == columnAppSettings.UniqueID);
            var isNewColumn = gridColumn == null;

            gridColumn ??= new DataGridColumnSettings();
            gridColumn.UniqueID = columnAppSettings.UniqueID;
            gridColumn.Width = columnAppSettings.Width;
            gridColumn.OrderIndex = columnAppSettings.OrderIndex;
            gridColumn.Visible = columnAppSettings.Visible;
            
            if (isNewColumn)
            {
                columnsToSet.Add(gridColumn);
            }
        }
        
        obj.Settings = new DataGridSettings
        {
            CurrentPage = obj.Settings?.CurrentPage,
            Groups = obj.Settings?.Groups,
            PageSize = obj.Settings?.PageSize,
            Columns = columnsToSet
        };
    }

    /// <summary>
    ///     Handles column resize events and persists the new width
    /// </summary>
    private async Task ColumnResized(DataGridColumnResizedEventArgs<TItem> arg)
    {
        GridSettings ??= new AppDataGridSettings();
        
        // Update the column width in our settings
        var columnState = GridSettings.Columns.FirstOrDefault(c => c.UniqueID == arg.Column.UniqueID);
        
        if(columnState == null)
        {
            columnState = new ColumnSettings
            {
                UniqueID = arg.Column.UniqueID,
                Visible = arg.Column.Visible,
                OrderIndex = arg.Column.OrderIndex ?? 0
            };
            GridSettings.Columns.Add(columnState);
        }

        columnState.Width = $"{(int)(arg.Width)}px";
        await SaveSettingsAsync();
    }

    /// <summary>
    ///     Saves current settings to local storage
    /// </summary>
    private async Task SaveSettingsAsync()
    {
        if (!String.IsNullOrEmpty(SettingsStorageKey))
        {
            await LocalStorage.SetItemAsync(SettingsStorageKey, GridSettings);
        }
    }

    /// <summary>
    ///     Loads settings from local storage and initializes column states
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        GridSettings = await GetSettingsAsync();
        _ = 1;
        //
        //
        // // Try to load settings from local storage
        // if (!String.IsNullOrEmpty(SettingsStorageKey))
        // {
        //     var savedSettings = await LocalStorage.GetItemAsync<AppDataGridSettings>(SettingsStorageKey);
        //     if (savedSettings != null && savedSettings.Columns.Any())
        //     {
        //         GridSettings = savedSettings;
        //
        //         // Apply loaded settings to the grid
        //         await HandleSettingsChanged();
        //     }
        // }
        //
        // // Initialize default settings if not loaded
        // if (!GridSettings.Columns.Any())
        // {
        //     GridSettings.Columns = _dataGrid.ColumnsCollection.Select((col, index) => new ColumnSettings
        //     {
        //         Title = col.Title,
        //         UniqueID = col.UniqueID,
        //         Visible = col.Visible,
        //         OrderIndex = index,
        //         Width = col.Width
        //     }).ToList();
        // }
    }

    private async Task<AppDataGridSettings?> GetSettingsAsync()
    {
        if (SettingsStorageKey == null)
        {
            return null;
        }

        return await LocalStorage.GetItemAsync<AppDataGridSettings>(SettingsStorageKey);
    }
}