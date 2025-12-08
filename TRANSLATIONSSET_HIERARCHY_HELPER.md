# Dataset Hierarchy Helper

## Overview

The `DataSetsQueryService` now includes helper methods to fetch datasets in hierarchical order. This is useful when you need to retrieve all datasets that are included in a given dataset, following the full chain of includes.

## Use Case

Datasets can include other datasets, forming a hierarchy. For example:
- Dataset "Final" includes "GlobalData"
- Dataset "GlobalData" includes "A" and "B"

When you need to retrieve all datasets for "Final", you want them in the correct order:
**Final → GlobalData → A → B**

## API Methods

### 1. `GetDataSetHierarchyIdsAsync`

Gets all dataset IDs in hierarchical order for a given root dataset.

```csharp
public async Task<List<Guid>> GetDataSetHierarchyIdsAsync(
    Guid rootDataSetId,
    CancellationToken cancellationToken = default)
```

**Returns:** List of dataset IDs in breadth-first traversal order, starting with the root dataset.

**Example:**
```csharp
var hierarchyIds = await _dataSetsQueryService.GetDataSetHierarchyIdsAsync(
    finalDataSetId, 
    cancellationToken);
// Result: [FinalId, GlobalDataId, AId, BId]
```

### 2. `GetDataSetHierarchyAsync`

Gets all datasets with full details in hierarchical order for a given root dataset.

```csharp
public async Task<List<TranslationsSet>> GetDataSetHierarchyAsync(
    Guid rootDataSetId,
    CancellationToken cancellationToken = default)
```

**Returns:** List of `TranslationsSet` entities in breadth-first traversal order, starting with the root dataset.

**Example:**
```csharp
var datasets = await _dataSetsQueryService.GetDataSetHierarchyAsync(
    finalDataSetId, 
    cancellationToken);

foreach (var dataset in datasets)
{
    Console.WriteLine($"{dataset.Name} (ID: {dataset.Id})");
}
// Output:
// Final (ID: ...)
// GlobalData (ID: ...)
// A (ID: ...)
// B (ID: ...)
```

## Features

### Authorization-Aware
Both methods respect user authorization. Only datasets the current user has access to will be included in the hierarchy.

### Circular Reference Protection
The implementation uses a `HashSet` to track visited datasets, preventing infinite loops if circular references exist.

### Breadth-First Traversal
The hierarchy is traversed breadth-first, ensuring that datasets at the same level are grouped together.

### In-Memory Processing
All datasets are fetched from the database in one query and then traversed in memory for optimal performance.

## Usage Example in Handler

Here's how you might use these methods in a MediatR handler:

```csharp
public class GetDataSetWithHierarchyQueryHandler : IRequestHandler<GetDataSetWithHierarchyQuery, DataSetHierarchyDto>
{
    private readonly DataSetsQueryService _queryService;

    public GetDataSetWithHierarchyQueryHandler(DataSetsQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<DataSetHierarchyDto> Handle(
        GetDataSetWithHierarchyQuery request, 
        CancellationToken cancellationToken)
    {
        // Get all datasets in the hierarchy
        var datasets = await _queryService.GetDataSetHierarchyAsync(
            request.RootDataSetId, 
            cancellationToken);

        // Map to DTOs
        return new DataSetHierarchyDto
        {
            RootDataSetId = request.RootDataSetId,
            TranslationsSets = datasets.Select(ds => ds.ToDto()).ToList()
        };
    }
}
```

## Implementation Details

1. **Single Database Query**: All datasets with their includes are loaded in one query
2. **Authorization Pre-filtering**: Only accessible datasets are loaded from the database
3. **Queue-based BFS**: Uses a queue to implement breadth-first search
4. **Visited Tracking**: HashSet prevents revisiting datasets (handles circular references)
5. **Order Preservation**: Results maintain the order of traversal

## Performance Considerations

- The helper loads all datasets with includes into memory, which is efficient for small to medium-sized datasets
- For very large datasets (thousands), consider pagination or filtering strategies
- Authorization filtering happens at the database level, reducing memory overhead
