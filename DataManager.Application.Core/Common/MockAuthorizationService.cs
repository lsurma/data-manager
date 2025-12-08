using DataManager.Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Common;

/// <summary>
/// Mock implementation of authorization service for development/testing
/// Returns a predefined subset of DataSet IDs to simulate user access restrictions
/// </summary>
public class MockAuthorizationService : IAuthorizationService
{
    private readonly DataManagerDbContext _context;

    public MockAuthorizationService(DataManagerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// For mock purposes, always returns false (no root access)
    /// </summary>
    public Task<bool> HasRootAccessAsync(CancellationToken cancellationToken = default)
    {
        // If authorization is omitted in the current scope, treat as root access
        if (OmitAuthorizationScope.ShouldOmitAuthorization)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// For mock purposes, returns true for the first 2 datasets, false for others
    /// </summary>
    public async Task<bool> CanAccessDataSetAsync(Guid dataSetId, CancellationToken cancellationToken = default)
    {
        var (allAccessible, accessibleIds) = await GetAccessibleDataSetsIdsAsync(cancellationToken);
        return allAccessible || accessibleIds.Contains(dataSetId);
    }

    /// <summary>
    /// Returns a subset of existing DataSet IDs to simulate authorization.
    /// In a real implementation, this would query user permissions/roles.
    /// Returns a tuple where:
    /// - AllAccessible: true if user has access to ALL datasets (no filtering needed), false otherwise
    /// - AccessibleIds: list of accessible dataset IDs (empty if AllAccessible is true)
    /// </summary>
    public async Task<(bool AllAccessible, List<Guid> AccessibleIds)> GetAccessibleDataSetsIdsAsync(CancellationToken cancellationToken = default)
    {
        // For mock purposes, return the first 2 datasets from the database
        // In production, this would check user claims, roles, or permissions
        // Pull just IDs to avoid issues with converted collections
        var accessibleTranslationSetsIds = await _context.DataSets
            .OrderBy(ds => ds.CreatedAt)
            .Take(2)
            .Select(ds => ds.Id)
            .ToListAsync(cancellationToken);

        // If no datasets exist yet, return empty list (no access)
        return (AllAccessible: false, AccessibleIds: accessibleTranslationSetsIds);
    }
}
