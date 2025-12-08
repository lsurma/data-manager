using DataManager.Host.WA.Models;
using Microsoft.JSInterop;

namespace DataManager.Host.WA.Services;

public interface ISettingsService
{
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
    Task ApplyThemeAsync(ThemeSettings theme);
    Task ResetToDefaultAsync();
}

public class SettingsService : ISettingsService
{
    private const string SettingsStorageKey = "app-settings";
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;
    private AppSettings? _cachedSettings;

    public SettingsService(ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
        {
            return _cachedSettings;
        }

        var settings = await _localStorage.GetItemAsync<AppSettings>(SettingsStorageKey);
        _cachedSettings = settings ?? new AppSettings();
        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _cachedSettings = settings;
        await _localStorage.SetItemAsync(SettingsStorageKey, settings);
        await ApplyThemeAsync(settings.Theme);
    }

    public async Task ApplyThemeAsync(ThemeSettings theme)
    {
        try
        {
            // Calculate hover and active colors (slightly darker)
            var hoverColor = await AdjustColorBrightnessAsync(theme.PrimaryColor, -10);
            var activeColor = await AdjustColorBrightnessAsync(theme.PrimaryColor, -15);

            // Apply CSS custom properties
            await _jsRuntime.InvokeVoidAsync("eval", $@"
                document.documentElement.style.setProperty('--accent-fill-rest', '{theme.PrimaryColor}');
                document.documentElement.style.setProperty('--accent-fill-hover', '{hoverColor}');
                document.documentElement.style.setProperty('--accent-fill-active', '{activeColor}');
                document.documentElement.style.setProperty('--accent-fill-focus', '{activeColor}');
            ");
        }
        catch
        {
            // Silently fail if JS is not available yet
        }
    }

    public async Task ResetToDefaultAsync()
    {
        var defaultSettings = new AppSettings
        {
            Theme = PredefinedThemes.GetDefaultTheme()
        };
        await SaveSettingsAsync(defaultSettings);
    }

    private async Task<string> AdjustColorBrightnessAsync(string hexColor, int adjustment)
    {
        try
        {
            // Use JavaScript to adjust color brightness
            var adjustedColor = await _jsRuntime.InvokeAsync<string>("eval", $@"
                (function() {{
                    const hex = '{hexColor}'.replace('#', '');
                    const r = parseInt(hex.substring(0, 2), 16);
                    const g = parseInt(hex.substring(2, 4), 16);
                    const b = parseInt(hex.substring(4, 6), 16);
                    
                    const newR = Math.max(0, Math.min(255, r + {adjustment}));
                    const newG = Math.max(0, Math.min(255, g + {adjustment}));
                    const newB = Math.max(0, Math.min(255, b + {adjustment}));
                    
                    return '#' + 
                        newR.toString(16).padStart(2, '0') + 
                        newG.toString(16).padStart(2, '0') + 
                        newB.toString(16).padStart(2, '0');
                }})()
            ");
            return adjustedColor;
        }
        catch
        {
            return hexColor;
        }
    }
}
