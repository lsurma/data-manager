namespace DataManager.Host.WA.Services;

public interface ICookieService
{
    Task<bool> HasCookieAsync(string name);
    Task<string?> GetCookieAsync(string name);
}
