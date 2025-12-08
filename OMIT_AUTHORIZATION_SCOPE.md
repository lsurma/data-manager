# OmitAuthorizationScope Usage Guide

## Overview

The `OmitAuthorizationScope` provides a way to temporarily bypass authorization checks within a specific scope. This is useful when authorization has already been performed in an outer query/command and you want to avoid redundant authorization checks for better performance.

## How It Works

The implementation uses `AsyncLocal<bool>` to maintain authorization state across async operations within the same logical call context. When you create an `OmitAuthorizationScope`, all authorization service calls within that scope will return as if the user has root access.

## Basic Usage

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

## Real-World Example

### Scenario: Fetching Translations with Related Data

Suppose you have a command handler that needs to fetch translations and then fetch related entities based on those translations. The outer query already checks authorization, so the inner queries don't need to re-check.

```csharp
public class GetTranslationsWithDetailsQueryHandler : IRequestHandler<GetTranslationsWithDetailsQuery, TranslationsWithDetailsDto>
{
    private readonly TranslationsQueryService _translationsQueryService;
    private readonly TranslationsSetsQueryService _translationsSetsQueryService;

    public async Task<TranslationsWithDetailsDto> Handle(
        GetTranslationsWithDetailsQuery request, 
        CancellationToken cancellationToken)
    {
        // First query: Authorization is checked
        var options = new QueryOptions<Translation, Guid>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering
        };

        var query = await _translationsQueryService.PrepareQueryAsync(
            options: options, 
            cancellationToken: cancellationToken);
        
        var translations = await query.ToListAsync(cancellationToken);

        // Get unique TranslationsSet IDs from the authorized translations
        var translationsSetIds = translations
            .Where(t => t.TranslationsSetId.HasValue)
            .Select(t => t.TranslationsSetId!.Value)
            .Distinct()
            .ToList();

        // Fetch TranslationsSets WITHOUT re-checking authorization
        // We already know the user has access because they have access to the translations
        List<TranslationsSet> translationsSets;
        using (new OmitAuthorizationScope())
        {
            translationsSets = await _translationsSetsQueryService.GetByIdsAsync(
                translationsSetIds,
                options: new QueryOptions<TranslationsSet, Guid> { AsNoTracking = true },
                cancellationToken: cancellationToken);
        }

        return new TranslationsWithDetailsDto
        {
            Translations = translations.Select(t => t.ToDto()).ToList(),
            TranslationsSets = translationsSets.Select(ds => ds.ToDto()).ToList()
        };
    }
}
```

## Advanced Example: Hierarchy Traversal

The `TranslationsQueryService` has methods like `GetTranslationsFromHierarchyAsync` that are marked as "CORE methods that omit authorization". However, if you want to call these from a handler that has already performed authorization, you can wrap them in an `OmitAuthorizationScope`:

```csharp
public class MaterializeTranslationsCommandHandler : IRequestHandler<MaterializeTranslationsCommand, int>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly TranslationsQueryService _translationsQueryService;

    public async Task<int> Handle(
        MaterializeTranslationsCommand request, 
        CancellationToken cancellationToken)
    {
        // First, check if user has access to the root TranslationsSet
        if (!await _authorizationService.CanAccessTranslationsSetAsync(
            request.RootTranslationsSetId, 
            cancellationToken))
        {
            throw new UnauthorizedAccessException("User does not have access to this TranslationsSet");
        }

        // Now materialize translations without re-checking authorization
        // We already verified access above
        using (new OmitAuthorizationScope())
        {
            return await _translationsQueryService.MaterializeTranslationsFromHierarchyAsync(
                request.RootTranslationsSetId,
                cancellationToken);
        }
    }
}
```

## Important Notes

1. **Use Sparingly**: Only use `OmitAuthorizationScope` when you're certain authorization has already been performed.

2. **Security Implications**: Improper use can lead to security vulnerabilities. Always ensure the outer operation has validated access.

3. **Async-Safe**: The scope is maintained across async/await boundaries within the same logical call context.

4. **Nested Scopes**: You can nest scopes, and they will properly restore the previous state:
   ```csharp
   // Authorization is ON
   using (new OmitAuthorizationScope())
   {
       // Authorization is OFF
       using (new OmitAuthorizationScope())
       {
           // Authorization is still OFF
       }
       // Authorization is still OFF
   }
   // Authorization is ON again
   ```

5. **Thread-Safe**: Each async context has its own scope state, so different requests won't interfere with each other.

## Performance Benefits

By omitting redundant authorization checks, you can improve performance:
- Reduces database queries for permission checks
- Eliminates repeated calls to `GetAccessibleTranslationsSetsIdsAsync`
- Especially beneficial in operations that involve multiple related queries

## When NOT to Use

- Do not use when the operation involves user input that hasn't been validated
- Do not use in API endpoints or controllers (authorization should happen there)
- Do not use when querying different entities than those already authorized
