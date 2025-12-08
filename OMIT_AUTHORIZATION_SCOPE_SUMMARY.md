# OmitAuthorizationScope Implementation Summary

## Problem Statement

The application needed a way to omit authorization in query services when it has already been done in another query/command, to improve performance by avoiding redundant database queries.

## Solution

Implemented `OmitAuthorizationScope` using `AsyncLocal<bool>` to provide a thread-safe, async-context-aware way to temporarily bypass authorization checks.

## Implementation Details

### Core Components

1. **OmitAuthorizationScope** (`DataManager.Application.Core/Common/OmitAuthorizationScope.cs`)
   - Implements `IDisposable` for automatic state restoration
   - Uses `AsyncLocal<bool>` to maintain state across async/await boundaries
   - Provides static `ShouldOmitAuthorization` property for checking current state
   - Supports nested scopes with proper state restoration

2. **AuthorizationService** (Updated)
   - Modified `HasRootAccessAsync()` to check `OmitAuthorizationScope.ShouldOmitAuthorization`
   - When scope is active, returns `true` (treating as root access)
   - Maintains backward compatibility - no changes to interface

3. **MockAuthorizationService** (Updated)
   - Updated `HasRootAccessAsync()` for consistency
   - Ensures test/development environments behave the same way

## Usage

```csharp
using DataManager.Application.Core.Common;

// Authorization is normally performed
var result1 = await _translationsQueryService.PrepareQueryAsync(...);

// Within this scope, authorization is omitted
using (new OmitAuthorizationScope())
{
    var result2 = await _translationsQueryService.PrepareQueryAsync(...);
    // No authorization checks are performed here
}

// Authorization is restored after the scope
var result3 = await _translationsQueryService.PrepareQueryAsync(...);
```

## Testing

Comprehensive manual testing verified:

1. ✅ **Basic Scope Behavior**
   - Correctly enables authorization omission within scope
   - Correctly restores after scope disposal

2. ✅ **Nested Scopes**
   - Inner scopes maintain the omit state
   - Proper restoration when all scopes are disposed

3. ✅ **Async Context Preservation**
   - State is maintained across `await` boundaries
   - Works correctly with async/await patterns

4. ✅ **Parallel Task Isolation**
   - Different async contexts are properly isolated
   - One task's scope doesn't affect another task

## Performance Benefits

- Reduces redundant database queries for permission checks
- Eliminates repeated calls to `GetAccessibleTranslationsSetsIdsAsync()`
- Particularly beneficial in operations involving multiple related queries
- Example: When fetching translations and then fetching their related TranslationsSets, authorization only needs to be checked once

## Security Considerations

- ⚠️ **Use with caution**: Only use when authorization has already been validated
- ⚠️ **Not for API endpoints**: Should be used internally, not at controller level
- ⚠️ **Document usage**: Always add comments explaining why authorization is omitted
- ✅ **Thread-safe**: Uses AsyncLocal for proper async context isolation
- ✅ **No side effects**: Automatically restores previous state

## Code Quality

- ✅ **Build**: All projects build successfully with no new warnings
- ✅ **Code Review**: Passed automated code review with no comments
- ✅ **Security Scan**: Passed CodeQL security analysis with 0 alerts
- ✅ **Documentation**: Comprehensive docs in `OMIT_AUTHORIZATION_SCOPE.md` and updated `CLAUDE.md`

## Files Changed

1. `DataManager.Application.Core/Common/OmitAuthorizationScope.cs` (NEW)
   - Core implementation with AsyncLocal and IDisposable

2. `DataManager.Application.Core/Common/AuthorizationService.cs` (MODIFIED)
   - Added scope check at the beginning of `HasRootAccessAsync()`

3. `DataManager.Application.Core/Common/MockAuthorizationService.cs` (MODIFIED)
   - Added scope check for consistency

4. `OMIT_AUTHORIZATION_SCOPE.md` (NEW)
   - Comprehensive usage guide with examples

5. `CLAUDE.md` (MODIFIED)
   - Added authorization system section with OmitAuthorizationScope documentation

## Future Enhancements

Potential improvements for future consideration:

1. **Metrics/Logging**: Add optional logging to track when authorization is omitted
2. **Debugging Support**: Add debug mode to help identify incorrect usage
3. **Configuration**: Allow disabling the feature in production if needed
4. **Audit Trail**: Track authorization omissions for security audits

## Example Use Cases

### Use Case 1: Fetching Related Data
```csharp
// Check authorization for main query
var translations = await _translationsQueryService.PrepareQueryAsync(...);
var translationsSetIds = translations.Select(t => t.TranslationsSetId).Distinct();

// Fetch related data without re-checking authorization
using (new OmitAuthorizationScope())
{
    var translationsSets = await _translationsSetsQueryService.GetByIdsAsync(translationsSetIds);
}
```

### Use Case 2: Hierarchy Traversal
```csharp
// Verify user has access to root
if (!await _authorizationService.CanAccessTranslationsSetAsync(rootId))
    throw new UnauthorizedAccessException();

// Traverse hierarchy without re-checking each node
using (new OmitAuthorizationScope())
{
    await _translationsQueryService.MaterializeTranslationsFromHierarchyAsync(rootId);
}
```

### Use Case 3: Batch Operations
```csharp
// Validate access to all items first
var accessibleIds = await ValidateAccessToAllItemsAsync(itemIds);

// Process items without re-validating each one
using (new OmitAuthorizationScope())
{
    foreach (var id in accessibleIds)
    {
        await ProcessItemAsync(id);
    }
}
```

## Conclusion

The `OmitAuthorizationScope` implementation successfully addresses the performance issue of redundant authorization checks while maintaining security through careful design and comprehensive documentation. The use of `AsyncLocal<bool>` ensures thread-safety and proper async context isolation, making it safe to use in modern async/await patterns.
