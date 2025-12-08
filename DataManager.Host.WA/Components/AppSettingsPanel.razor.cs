using DataManager.Host.WA.Models;
using DataManager.Host.WA.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Components;

public partial class AppSettingsPanel : IDialogContentComponent<AppSettingsPanelParameters>
{
    [Parameter]
    public AppSettingsPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    [Inject]
    private ISettingsService SettingsService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private bool IsSaving { get; set; }
    
    private string PrimaryColor { get; set; } = "#1060ff";
    private string SelectedThemeName { get; set; } = "Default";
    private List<string> ThemeNames { get; set; } = PredefinedThemes.GetThemeNames();
    
    protected override async Task OnInitializedAsync()
    {
        var settings = await SettingsService.GetSettingsAsync();
        PrimaryColor = settings.Theme.PrimaryColor;
        SelectedThemeName = settings.Theme.SelectedTheme;
    }
    
    private async Task OnThemeChangedAsync()
    {
        // When theme changes, update the color and apply preview
        if (!string.IsNullOrEmpty(SelectedThemeName) && 
            PredefinedThemes.Themes.ContainsKey(SelectedThemeName))
        {
            var theme = PredefinedThemes.Themes[SelectedThemeName];
            PrimaryColor = theme.PrimaryColor;
            
            // Apply theme for live preview
            await SettingsService.ApplyThemeAsync(new ThemeSettings
            {
                PrimaryColor = PrimaryColor,
                SelectedTheme = SelectedThemeName
            });
        }
    }
    
    private async Task OnColorChangedAsync(string color)
    {
        PrimaryColor = color;
        
        // Apply color change for live preview
        await SettingsService.ApplyThemeAsync(new ThemeSettings
        {
            PrimaryColor = PrimaryColor,
            SelectedTheme = "Custom"
        });
        
        // Set theme to Custom when manually changing color
        SelectedThemeName = "Custom";
    }

    private async Task HandleSaveAsync()
    {
        IsSaving = true;
        StateHasChanged();

        try
        {
            var settings = new AppSettings
            {
                Theme = new ThemeSettings
                {
                    PrimaryColor = PrimaryColor,
                    SelectedTheme = SelectedThemeName
                }
            };

            await SettingsService.SaveSettingsAsync(settings);
            
            ToastService.ShowSuccess("Settings saved successfully");
            
            if (Dialog != null)
            {
                await Dialog.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to save settings: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task HandleRestoreDefaultsAsync()
    {
        IsSaving = true;
        StateHasChanged();

        try
        {
            await SettingsService.ResetToDefaultAsync();
            
            var settings = await SettingsService.GetSettingsAsync();
            PrimaryColor = settings.Theme.PrimaryColor;
            SelectedThemeName = settings.Theme.SelectedTheme;
            
            ToastService.ShowSuccess("Settings restored to defaults");
            
            if (Dialog != null)
            {
                await Dialog.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to restore defaults: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task HandleCancelAsync()
    {
        // Restore original theme before closing
        var settings = await SettingsService.GetSettingsAsync();
        await SettingsService.ApplyThemeAsync(settings.Theme);
        
        if (Dialog != null)
        {
            await Dialog.CancelAsync();
        }
    }
}

public record AppSettingsPanelParameters;
