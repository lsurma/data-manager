# Dataset Hierarchy Query Helper - Usage Example

## Overview

This document provides practical examples of how to use the dataset hierarchy query helper methods.

## Scenario

Let's say you have the following dataset hierarchy:
```
Final (ID: f1f1f1f1-0000-0000-0000-000000000001)
  ├── GlobalData (ID: a2a2a2a2-0000-0000-0000-000000000002)
  │   ├── Dataset A (ID: b3b3b3b3-0000-0000-0000-000000000003)
  │   └── Dataset B (ID: c4c4c4c4-0000-0000-0000-000000000004)
  └── SpecialData (ID: d5d5d5d5-0000-0000-0000-000000000005)
```

## Method 1: Using GetDataSetHierarchyIdsAsync (IDs Only)

This method is useful when you only need the dataset IDs for filtering or lookups.

```csharp
// In your service or handler
public class TranslationService
{
    private readonly DataSetsQueryService _dataSetsQueryService;
    private readonly DataManagerDbContext _context;

    public async Task<List<Translation>> GetAllTranslationsForDataSetAsync(
        Guid dataSetId, 
        CancellationToken cancellationToken)
    {
        // Get all dataset IDs in the hierarchy
        var dataSetIds = await _dataSetsQueryService.GetDataSetHierarchyIdsAsync(
            dataSetId, 
            cancellationToken);

        // Result: [f1f1f1f1..., a2a2a2a2..., b3b3b3b3..., c4c4c4c4..., d5d5d5d5...]

        // Use these IDs to query translations from all datasets in the hierarchy
        var translations = await _context.Translations
            .Where(t => dataSetIds.Contains(t.DataSetId))
            .ToListAsync(cancellationToken);

        return translations;
    }
}
```

## Method 2: Using GetDataSetHierarchyAsync (Full Entities)

This method returns complete dataset entities with all properties.

```csharp
public class DataSetAnalysisService
{
    private readonly DataSetsQueryService _dataSetsQueryService;

    public async Task<DataSetAnalysisReport> AnalyzeDataSetHierarchyAsync(
        Guid rootDataSetId, 
        CancellationToken cancellationToken)
    {
        // Get all datasets in the hierarchy with full details
        var datasets = await _dataSetsQueryService.GetDataSetHierarchyAsync(
            rootDataSetId, 
            cancellationToken);

        // Process each dataset in order
        var report = new DataSetAnalysisReport
        {
            RootDataSetName = datasets.First().Name,
            TotalDataSets = datasets.Count,
            DataSetNames = datasets.Select(ds => ds.Name).ToList()
        };

        // Example output:
        // RootDataSetName: "Final"
        // TotalDataSets: 5
        // DataSetNames: ["Final", "GlobalData", "Dataset A", "Dataset B", "SpecialData"]

        return report;
    }
}
```

## Method 3: Using the MediatR Query (Recommended)

The cleanest approach is to use the pre-built `GetDataSetHierarchyQuery` and handler.

### In your controller or service:

```csharp
public class DataSetController
{
    private readonly IRequestSender _requestSender;

    public async Task<DataSetHierarchyDto> GetDataSetHierarchy(
        Guid rootDataSetId,
        CancellationToken cancellationToken)
    {
        var query = new GetDataSetHierarchyQuery
        {
            RootDataSetId = rootDataSetId
        };

        var result = await _requestSender.SendAsync<DataSetHierarchyDto>(
            query, 
            cancellationToken);

        return result;
    }
}
```

### In Blazor Frontend:

```csharp
@inject IRequestSender RequestSender

@code {
    private List<DataSetDto> hierarchyDataSets = new();
    
    private async Task LoadDataSetHierarchy(Guid rootId)
    {
        var query = new GetDataSetHierarchyQuery 
        { 
            RootDataSetId = rootId 
        };
        
        var result = await RequestSender.SendAsync<DataSetHierarchyDto>(query);
        hierarchyDataSets = result.TranslationsSets;
        
        // Display the hierarchy in order
        foreach (var dataset in hierarchyDataSets)
        {
            Console.WriteLine($"Dataset: {dataset.Name}");
        }
    }
}
```

## Advanced Example: Building a Merged Configuration

A common use case is merging configurations from all datasets in a hierarchy:

```csharp
public class DataSetConfigurationService
{
    private readonly DataSetsQueryService _dataSetsQueryService;

    public async Task<MergedConfiguration> GetMergedConfigurationAsync(
        Guid rootDataSetId, 
        CancellationToken cancellationToken)
    {
        // Get datasets in hierarchy order (root first, then children)
        var datasets = await _dataSetsQueryService.GetDataSetHierarchyAsync(
            rootDataSetId, 
            cancellationToken);

        var mergedConfig = new MergedConfiguration();

        // Process in order: values from earlier datasets can be overridden by later ones
        // Or reverse the order if you want child values to have lower priority
        foreach (var dataset in datasets)
        {
            // Merge cultures (union of all available cultures)
            if (dataset.AvailableCultures != null)
            {
                mergedConfig.AvailableCultures.UnionWith(dataset.AvailableCultures);
            }

            // Merge allowed identities (union of all allowed users)
            mergedConfig.AllowedIdentityIds.UnionWith(dataset.AllowedIdentityIds);
        }

        return mergedConfig;
    }
}
```

## Testing the Implementation

Here's how you can manually test the hierarchy helper:

```csharp
// In a test controller or handler
public async Task<string> TestHierarchy()
{
    // Create test data
    var dataSetA = new TranslationsSet { Id = Guid.NewGuid(), Name = "Dataset A" };
    var dataSetB = new TranslationsSet { Id = Guid.NewGuid(), Name = "Dataset B" };
    var globalData = new TranslationsSet { Id = Guid.NewGuid(), Name = "GlobalData" };
    var final = new TranslationsSet { Id = Guid.NewGuid(), Name = "Final" };

    await _context.TranslationsSets.AddRangeAsync(dataSetA, dataSetB, globalData, final);

    // Set up relationships
    globalData.Includes.Add(new DataSetInclude 
    { 
        ParentDataSetId = globalData.Id, 
        IncludedDataSetId = dataSetA.Id,
        CreatedAt = DateTimeOffset.UtcNow
    });
    globalData.Includes.Add(new DataSetInclude 
    { 
        ParentDataSetId = globalData.Id, 
        IncludedDataSetId = dataSetB.Id,
        CreatedAt = DateTimeOffset.UtcNow
    });
    final.Includes.Add(new DataSetInclude 
    { 
        ParentDataSetId = final.Id, 
        IncludedDataSetId = globalData.Id,
        CreatedAt = DateTimeOffset.UtcNow
    });

    await _context.SaveChangesAsync();

    // Test the hierarchy helper
    var hierarchy = await _dataSetsQueryService.GetDataSetHierarchyAsync(
        final.Id, 
        CancellationToken.None);

    // Should return: [Final, GlobalData, Dataset A, Dataset B]
    return string.Join(" -> ", hierarchy.Select(ds => ds.Name));
}
```

## Key Features to Remember

1. **Authorization-Aware**: Only datasets the user has access to are included
2. **Circular Reference Safe**: Won't get stuck in infinite loops
3. **Breadth-First Order**: Datasets at the same level are grouped together
4. **Performance Optimized**: Single database query, in-memory traversal
5. **Empty Result Safe**: Returns empty list if root dataset not found or not accessible

## Error Handling

```csharp
var hierarchy = await _dataSetsQueryService.GetDataSetHierarchyAsync(
    rootDataSetId, 
    cancellationToken);

if (!hierarchy.Any())
{
    // Root dataset not found or user doesn't have access
    throw new NotFoundException($"Dataset {rootDataSetId} not found or not accessible");
}

if (hierarchy.Count == 1)
{
    // Root dataset exists but has no includes
    Console.WriteLine("Dataset has no child datasets");
}
```
