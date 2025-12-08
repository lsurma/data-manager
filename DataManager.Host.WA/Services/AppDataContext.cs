using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSets;
using Microsoft.Extensions.Logging;

namespace DataManager.Host.WA.Services;

/// <summary>
/// Application data context that stores reusable data across pages.
/// This reduces redundant API calls by caching frequently used data like DataSets.
/// </summary>
public class AppDataContext
{
    private readonly IRequestSender _requestSender;
    private readonly ILogger<AppDataContext> _logger;
    private readonly SemaphoreSlim _loadingSemaphore = new(1, 1);
    
    public AppDataContext(IRequestSender requestSender, ILogger<AppDataContext> logger)
    {
        _requestSender = requestSender;
        _logger = logger;
    }
    
    /// <summary>
    /// Cached list of all available DataSets
    /// </summary>
    public List<DataSetDto> DataSets { get; private set; } = new();
    
    /// <summary>
    /// Indicates whether data has been loaded at least once
    /// </summary>
    public bool IsLoaded { get; private set; }
    
    /// <summary>
    /// Indicates whether data is currently being loaded
    /// </summary>
    public bool IsLoading { get; private set; }
    
    /// <summary>
    /// Event raised when data is refreshed
    /// </summary>
    public event Action? OnDataRefreshed;
    
    /// <summary>
    /// Loads all reusable data from the API
    /// </summary>
    public async Task LoadDataAsync()
    {
        // Use semaphore to prevent concurrent loading
        if (!await _loadingSemaphore.WaitAsync(0))
        {
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var translationSetsResult = await _requestSender.SendAsync(GetDataSetsQuery.AllItems());
            DataSets = translationSetsResult.Items;
            
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load app data");
            throw;
        }
        finally
        {
            IsLoading = false;
            _loadingSemaphore.Release();
        }
    }
    
    /// <summary>
    /// Refreshes all cached data and notifies subscribers
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadDataAsync();
        OnDataRefreshed?.Invoke();
    }
}
