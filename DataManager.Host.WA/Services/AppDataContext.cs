using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSet;

namespace DataManager.Host.WA.Services;

/// <summary>
/// Application data context that stores reusable data across pages.
/// This reduces redundant API calls by caching frequently used data like DataSets.
/// </summary>
public class AppDataContext
{
    private readonly IRequestSender _requestSender;
    
    public AppDataContext(IRequestSender requestSender)
    {
        _requestSender = requestSender;
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
        if (IsLoading)
        {
            return;
        }
        
        IsLoading = true;
        
        try
        {
            // Load all DataSets
            var dataSetsResult = await _requestSender.SendAsync(GetDataSetsQuery.AllItems());
            DataSets = dataSetsResult.Items;
            
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load app data: {ex.Message}");
            throw;
        }
        finally
        {
            IsLoading = false;
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
