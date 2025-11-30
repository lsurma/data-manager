using Microsoft.JSInterop;

namespace DataManager.Host.WA.Services;

public class CookieService : ICookieService
{
    private readonly IJSRuntime _jsRuntime;

    public CookieService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> HasCookieAsync(string name)
    {
        var cookie = await GetCookieAsync(name);
        return !string.IsNullOrEmpty(cookie);
    }

    public async Task<string?> GetCookieAsync(string name)
    {
        try
        {
            var allCookies = await _jsRuntime.InvokeAsync<string>("cookieUtils.getCookies");
            
            if (string.IsNullOrEmpty(allCookies))
            {
                return null;
            }

            var cookies = allCookies.Split(';');
            foreach (var cookie in cookies)
            {
                var parts = cookie.Trim().Split('=', 2);
                if (parts[0].Trim() == name)
                {
                    return parts.Length > 1 ? parts[1] : string.Empty;
                }
            }

            return null;
        }
        catch (JSException)
        {
            // JavaScript interop failed (e.g., during prerendering)
            return null;
        }
        catch (InvalidOperationException)
        {
            // JSRuntime not available yet
            return null;
        }
    }
}
