namespace DataManager.Application.Contracts.Common;

public record PaginationParameters
{
    public PaginationParameters(int skip, int take)
    {
        Skip = skip;
        PageSize = take;
        PageNumber = (Skip / PageSize) + 1;
    }

    public PaginationParameters()
    {
        
    }
    
    /// <summary>
    /// Page number (1-based). Setting this will calculate Skip automatically.
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Number of items to skip (0-based offset). Setting this will calculate PageNumber automatically.
    /// </summary>
    public int Skip { get; set; }
    
    public int PageSize { get; set; } = 20;


    /// <summary>
    /// Creates pagination parameters for fetching all items
    /// </summary>
    public static PaginationParameters AllItems() => new(0, int.MaxValue);
}
