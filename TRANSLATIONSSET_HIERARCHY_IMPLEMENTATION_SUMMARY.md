# Dataset Hierarchy Query Helper - Implementation Summary

## Overview
This implementation adds helper methods to query and traverse dataset hierarchies in the DataManager application. Datasets can include other datasets, forming a tree-like structure, and these helpers allow fetching the entire hierarchy in correct breadth-first order.

## Problem Statement
Datasets can have hierarchical relationships through the `DataSetInclude` join table:
- Dataset "Final" includes "GlobalData"
- Dataset "GlobalData" includes "A" and "B"

When fetching "Final", we need the full hierarchy: **Final → GlobalData → A → B**

## Solution

### Core Implementation: DataSetsQueryService

Three methods were added to `DataManager.Application.Core/Modules/TranslationsSet/DataSetsQueryService.cs`:

#### 1. GetDataSetHierarchyIdsAsync (Public)
```csharp
public async Task<List<Guid>> GetDataSetHierarchyIdsAsync(
    Guid rootDataSetId,
    CancellationToken cancellationToken = default)
```
Returns a list of dataset IDs in hierarchical order (breadth-first).

#### 2. GetDataSetHierarchyAsync (Public)
```csharp
public async Task<List<TranslationsSet>> GetDataSetHierarchyAsync(
    Guid rootDataSetId,
    CancellationToken cancellationToken = default)
```
Returns a list of dataset entities with full details in hierarchical order.

#### 3. GetDataSetHierarchyInternalAsync (Private)
```csharp
private async Task<(List<Guid> hierarchyIds, Dictionary<Guid, TranslationsSet> dataSetLookup)> 
    GetDataSetHierarchyInternalAsync(
        Guid rootDataSetId,
        CancellationToken cancellationToken = default)
```
Internal method that performs the actual traversal and returns both IDs and entities. This avoids duplicate database queries when both public methods are used.

### MediatR Integration

For easy consumption through the CQRS pattern:

1. **GetDataSetHierarchyQuery.cs** - Request contract
2. **DataSetHierarchyDto.cs** - Response DTO
3. **GetDataSetHierarchyQueryHandler.cs** - Handler implementation

### Algorithm Details

**Breadth-First Search (BFS):**
1. Load all accessible datasets from the database (single query with authorization)
2. Start with the root dataset
3. Use a queue to process datasets level by level
4. Track visited datasets with a HashSet to prevent circular references
5. Return results in order of traversal

**Complexity:**
- Time: O(V + E) where V is vertices (datasets) and E is edges (includes)
- Space: O(V) for the visited set and result list

### Key Features

1. **Single Database Query**: All data loaded once, then processed in-memory
2. **Authorization-Aware**: Respects user permissions via `ApplyAuthorizationAsync()`
3. **Circular Reference Safe**: HashSet prevents infinite loops
4. **Breadth-First Order**: Results organized level by level
5. **Optimized DTO Mapping**: Uses `List<TranslationsSet>.ToDto()` extension for efficient mapping

### Files Modified/Created

**Core Project:**
- `DataManager.Application.Core/Modules/TranslationsSet/DataSetsQueryService.cs` - Added hierarchy methods
- `DataManager.Application.Core/Modules/TranslationsSet/Handlers/GetDataSetHierarchyQueryHandler.cs` - New handler

**Contracts Project:**
- `DataManager.Application.Contracts/Modules/TranslationsSet/GetDataSetHierarchyQuery.cs` - New query
- `DataManager.Application.Contracts/Modules/TranslationsSet/DataSetHierarchyDto.cs` - New DTO

**Documentation:**
- `DATASET_HIERARCHY_HELPER.md` - Technical documentation
- `DATASET_HIERARCHY_USAGE_EXAMPLES.md` - Usage examples and patterns
- `DATASET_HIERARCHY_IMPLEMENTATION_SUMMARY.md` - This file

## Usage Example

```csharp
// Using the query service directly
var dataSetIds = await _dataSetsQueryService.GetDataSetHierarchyIdsAsync(
    rootDataSetId, 
    cancellationToken);

// Using MediatR
var query = new GetDataSetHierarchyQuery { RootDataSetId = rootDataSetId };
var result = await _requestSender.SendAsync<DataSetHierarchyDto>(query);
```

## Testing

- All code compiles successfully
- Solution builds with no errors
- CodeQL security scan: 0 vulnerabilities
- Code review feedback addressed

## Performance Considerations

- **Efficient**: Single database query with in-memory traversal
- **Scalable**: Works well for typical dataset hierarchies (dozens to hundreds of datasets)
- **Authorization**: Pre-filtering at database level reduces memory overhead

## Future Enhancements

Potential improvements if needed:
- Add depth limit parameter to prevent extremely deep hierarchies
- Add option for depth-first traversal
- Add caching for frequently accessed hierarchies
- Add metrics/logging for hierarchy depth and complexity

## Related Documentation

- See `DATASET_HIERARCHY_HELPER.md` for API details
- See `DATASET_HIERARCHY_USAGE_EXAMPLES.md` for comprehensive usage patterns
- See `CLAUDE.md` for architecture overview
