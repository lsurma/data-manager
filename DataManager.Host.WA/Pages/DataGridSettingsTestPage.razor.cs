using Microsoft.AspNetCore.Components;
using Radzen;

namespace DataManager.Host.WA.Pages;

public class DataGridTestItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public System.DateTime CreatedDate { get; set; }
}

public partial class DataGridSettingsTestPage : ComponentBase
{
    private List<DataGridTestItem> Items { get; set; } = new();
    private int TotalItems { get; set; } = 100;
    private int PageSize { get; set; } = 10;
    private List<DataGridTestItem> AllData { get; set; } = new();

    protected override void OnInitialized()
    {
        GenerateSampleData();
        Items = AllData.Take(PageSize).ToList();
    }

    private void GenerateSampleData()
    {
        AllData = Enumerable.Range(1, TotalItems).Select(i => new DataGridTestItem
        {
            Id = i,
            Name = $"Item {i}",
            Description = $"This is the description for item {i}",
            CreatedDate = System.DateTime.Now.AddDays(-i)
        }).ToList();
    }

    private void OnLoadData(LoadDataArgs args)
    {
        var query = AllData.AsQueryable();

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            // Radzen's OrderBy is a string, so we can't use it directly with LINQ.
            // This is a simplified example. A real implementation would need a more robust solution.
            // For this test, we'll just handle paging.
        }

        Items = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();
        StateHasChanged();
    }
}