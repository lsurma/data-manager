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

    private AppDataGridSettings? DataGridInternalSettings { get; set; }

    protected DataGridSettings? RadzenDataGridSettings { get; set; }
    
    /// <summary>
    ///     Flag to prevent LoadSettings from triggering grid refresh during column resize
    /// </summary>
    private bool _isHandlingColumnResize;

    /// <summary>
    ///     Opens the settings panel for column customization
    /// </summary>
    private async Task OpenSettingsPanelAsync()
    {
        // Copy for editing
        var settings = DataGridInternalSettings ?? new AppDataGridSettings();
        settings = settings with
        {
            Columns = []
        };

        foreach (var column in _dataGrid.ColumnsCollection)
        {
            var existingColumnState = DataGridInternalSettings?.Columns.FirstOrDefault(c => c.UniqueID == column.UniqueID);

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
        if (!result.Cancelled && result.Data is DataGridSettingsPanelParameters dialogParams)
        {
            DataGridInternalSettings ??= new AppDataGridSettings();
            DataGridInternalSettings.Columns = dialogParams.Columns;

            // Save
            await SaveSettingsAsync();
            
            // Apply
            await HandleSettingsChanged();
        }
    }

    /// <summary>
    ///     Handles DataGrid settings changed event from Radzen
    /// </summary>
    private Task SettingsChanged(DataGridSettings arg)
    {
        RadzenDataGridSettings = arg;

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Applies column state changes to the DataGrid settings
    /// </summary>
    private async Task HandleSettingsChanged()
    {
        await LoadSettingsAsync();
        RadzenDataGridSettings = CreateDataGridSettings();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    ///     Loads settings when the DataGrid requests them
    /// </summary>
    private void LoadRadzenDataGridSettings(DataGridLoadSettingsEventArgs obj)
    {
        Console.WriteLine("Loading Radzen DataGrid settings");

        if (DataGridInternalSettings == null)
        {
            return;
        }
        
        // Skip applying settings if we're in the middle of handling a column resize
        // to prevent triggering LoadData and creating an infinite loop
        if (_isHandlingColumnResize)
        {
            return;
        }

        var dataGridColumns = (obj.Settings?.Columns ?? []).ToList();
        var anyChange = false;
        
        foreach (var columnAppSettings in DataGridInternalSettings.Columns)
        {
            var gridColumn = dataGridColumns.FirstOrDefault(c => c.UniqueID == columnAppSettings.UniqueID);
            var isNewColumn = gridColumn == null;

            gridColumn ??= new DataGridColumnSettings();
            gridColumn.UniqueID = columnAppSettings.UniqueID;

            if (gridColumn.Width != columnAppSettings.Width)
            {
                gridColumn.Width = columnAppSettings.Width;
                anyChange = true;
            }

            if (gridColumn.OrderIndex != columnAppSettings.OrderIndex)
            {
                gridColumn.OrderIndex = columnAppSettings.OrderIndex;
                anyChange = true;
            }

            if (gridColumn.Visible != columnAppSettings.Visible)
            {
                gridColumn.Visible = columnAppSettings.Visible;
                anyChange = true;
            }
            
            if (isNewColumn)
            {
                dataGridColumns.Add(gridColumn);
                anyChange = true;
            }
        }

        if (anyChange)
        {
            obj.Settings = new DataGridSettings
            {
                CurrentPage = obj.Settings?.CurrentPage,
                PageSize = obj.Settings?.PageSize,
                Groups = obj.Settings?.Groups,
                Columns = dataGridColumns
            };
        }
    }
    
    private DataGridSettings CreateDataGridSettings()
    {
        Console.WriteLine("Loading Radzen DataGrid settings");

        if (DataGridInternalSettings == null)
        {
            return new DataGridSettings();
        }
        
        var obj = new DataGridSettings();

        var dataGridColumns = (obj.Columns ?? []).ToList();
        var anyChange = false;
        
        foreach (var columnAppSettings in DataGridInternalSettings.Columns)
        {
            var gridColumn = dataGridColumns.FirstOrDefault(c => c.UniqueID == columnAppSettings.UniqueID);
            var isNewColumn = gridColumn == null;

            gridColumn ??= new DataGridColumnSettings();
            gridColumn.UniqueID = columnAppSettings.UniqueID;

            if (gridColumn.Width != columnAppSettings.Width)
            {
                gridColumn.Width = columnAppSettings.Width;
                anyChange = true;
            }

            if (gridColumn.OrderIndex != columnAppSettings.OrderIndex)
            {
                gridColumn.OrderIndex = columnAppSettings.OrderIndex;
                anyChange = true;
            }

            if (gridColumn.Visible != columnAppSettings.Visible)
            {
                gridColumn.Visible = columnAppSettings.Visible;
                anyChange = true;
            }
            
            if (isNewColumn)
            {
                dataGridColumns.Add(gridColumn);
                anyChange = true;
            }
        }
        
        obj.Columns = dataGridColumns;

        return obj;
    }

    /// <summary>
    ///     Handles column resize events and persists the new width
    /// </summary>
    private async Task ColumnResized(DataGridColumnResizedEventArgs<TItem> arg)
    {
        // Set flag to prevent LoadSettings from triggering grid refresh
        _isHandlingColumnResize = true;
        
        try
        {
            DataGridInternalSettings ??= new AppDataGridSettings();
            
            // Update the column width in our settings
            var columnState = DataGridInternalSettings.Columns.FirstOrDefault(c => c.UniqueID == arg.Column.UniqueID);
            
            if(columnState == null)
            {
                columnState = new ColumnSettings
                {
                    UniqueID = arg.Column.UniqueID,
                    Visible = arg.Column.Visible,
                    OrderIndex = arg.Column.OrderIndex ?? 0
                };
                DataGridInternalSettings.Columns.Add(columnState);
            }

            columnState.Width = $"{(int)(arg.Width)}px";
            await SaveSettingsAsync();
        }
        finally
        {
            // Always reset the flag, even if an exception occurs
            _isHandlingColumnResize = false;
        }
    }

    /// <summary>
    ///     Saves current settings to local storage
    /// </summary>
    private async Task SaveSettingsAsync()
    {
        if (!String.IsNullOrEmpty(SettingsStorageKey))
        {
            await LocalStorage.SetItemAsync(SettingsStorageKey, DataGridInternalSettings);
        }
    }

    /// <summary>
    ///     Loads settings from local storage and initializes column states
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        DataGridInternalSettings = await GetSettingsAsync();
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