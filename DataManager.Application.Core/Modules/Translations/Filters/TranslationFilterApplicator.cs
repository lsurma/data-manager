using System.Linq.Expressions;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;

namespace DataManager.Application.Core.Modules.Translations.Filters;

public class DataSetIdFilterHandler : IFilterHandler<Translation, DataSetIdFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(DataSetIdFilter filter, CancellationToken cancellationToken = default)
    {
        var dataSetId = filter.Value!.Value; // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t => t.DataSetId == dataSetId;
        return Task.FromResult(expression);
    }
}

public class CultureNameFilterHandler : IFilterHandler<Translation, CultureNameFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(CultureNameFilter filter, CancellationToken cancellationToken = default)
    {
        var cultureName = filter.Value!; // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t => t.CultureName == cultureName;
        return Task.FromResult(expression);
    }
}

public class TranslationSearchFilterHandler : IFilterHandler<Translation, SearchFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(SearchFilter filter, CancellationToken cancellationToken = default)
    {
        var searchTerm = filter.SearchTerm!.ToLower(); // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t =>
            (t.InternalGroupName1 != null && t.InternalGroupName1.ToLower().Contains(searchTerm)) ||
            (t.InternalGroupName2 != null && t.InternalGroupName2.ToLower().Contains(searchTerm)) ||
            t.ResourceName.ToLower().Contains(searchTerm) ||
            t.TranslationName.ToLower().Contains(searchTerm) ||
            t.Content.ToLower().Contains(searchTerm);
        return Task.FromResult(expression);
    }
}

public class BaseTranslationFilterHandler : IFilterHandler<Translation, BaseTranslationFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(BaseTranslationFilter filter, CancellationToken cancellationToken = default)
    {
        var cultureName = filter.CultureName;
        
        // Filter for base translations (SourceId is null) that match the specified culture or have null culture
        Expression<Func<Translation, bool>> expression = t => 
            t.SourceId == null && 
            (t.CultureName == null || t.CultureName == cultureName);
        
        return Task.FromResult(expression);
    }
}

public class VersionStatusFilterHandler : IFilterHandler<Translation, VersionStatusFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(VersionStatusFilter filter, CancellationToken cancellationToken = default)
    {
        // Build expression based on which versions to include
        // If none specified, include nothing (should not happen due to IsActive check)
        Expression<Func<Translation, bool>> expression = t => false;

        bool includeCurrent = filter.IncludeCurrentVersions ?? false;
        bool includeDraft = filter.IncludeDraftVersions ?? false;
        bool includeOld = filter.IncludeOldVersions ?? false;

        // Build the expression to include specified version types
        if (includeCurrent && includeDraft && includeOld)
        {
            // Include all - no filtering needed
            expression = t => true;
        }
        else if (includeCurrent && includeDraft)
        {
            expression = t => t.IsCurrentVersion || t.IsDraftVersion;
        }
        else if (includeCurrent && includeOld)
        {
            expression = t => t.IsCurrentVersion || t.IsOldVersion;
        }
        else if (includeDraft && includeOld)
        {
            expression = t => t.IsDraftVersion || t.IsOldVersion;
        }
        else if (includeCurrent)
        {
            expression = t => t.IsCurrentVersion;
        }
        else if (includeDraft)
        {
            expression = t => t.IsDraftVersion;
        }
        else if (includeOld)
        {
            expression = t => t.IsOldVersion;
        }

        return Task.FromResult(expression);
    }
}
