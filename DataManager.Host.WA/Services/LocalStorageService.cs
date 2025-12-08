using System.Text.Json;
using DataManager.Application.Contracts;
using Microsoft.JSInterop;

namespace DataManager.Host.WA.Services;

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json, DataManagerJsonSerializerOptions.Default);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, DataManagerJsonSerializerOptions.Default);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // Silently fail - local storage might be full or disabled
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Silently fail
        }
    }
}
