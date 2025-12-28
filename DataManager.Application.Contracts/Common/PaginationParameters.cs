namespace DataManager.Application.Contracts.Common;

public record PaginationParameters
{
    public PaginationParameters(int skip, int take)
    {
        Skip = skip;
        PageSize = take;
        PageNumber = (Skip / PageSize) + 1;
    }

    public PaginationParameters() : this(0, 20)
    {
        
    }
    
    /// <summary>
    /// Page number (1-based). Setting this will calculate Skip automatically.
    /// </summary>
    public int PageNumber { get; private set; }
    
    /// <summary>
    /// Number of items to skip (0-based offset). Setting this will calculate PageNumber automatically.
    /// </summary>
    public int Skip { get; private set; }
    
    public int PageSize { get; private set; } = 20;


    /// <summary>
    /// Creates pagination parameters for fetching all items
    /// </summary>
    public static PaginationParameters AllItems() => new(0, int.MaxValue);
    
    public static PaginationParameters ByPage(int pageNumber, int pageSize) => new(pageNumber * pageSize - pageSize, pageSize);
    
    public static PaginationParameters ByPage(int pageNumber) => ByPage(pageNumber, 20);
    
    public static PaginationParameters ByOffset(int skip, int pageSize) => new(skip, pageSize);
}
