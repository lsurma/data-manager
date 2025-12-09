using System.Linq.Expressions;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;

namespace DataManager.Application.Core.Modules.Translations.Filters;

public class DataSetIdFilterHandler : IFilterHandler<Translation, DataSetIdFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(DataSetIdFilter filter, CancellationToken cancellationToken = default)
    {
        var translationSetId = filter.Value!.Value; // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t => t.DataSetId == translationSetId;
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
        bool includeCurrent = filter.IncludeCurrentVersions ?? false;
        bool includeDraft = filter.IncludeDraftVersions ?? false;
        bool includeOld = filter.IncludeOldVersions ?? false;

        // Build a single expression that checks all conditions
        Expression<Func<Translation, bool>> expression = t =>
            (includeCurrent && t.IsCurrentVersion) ||
            (includeDraft && t.IsDraftVersion) ||
            (includeOld && t.IsOldVersion);

        return Task.FromResult(expression);
    }
}

public class InternalGroupName1FilterHandler : IFilterHandler<Translation, InternalGroupName1Filter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(InternalGroupName1Filter filter, CancellationToken cancellationToken = default)
    {
        var groupName = filter.Value!; // We know it has value because IsActive() was checked
        Expression<Func<Translation, bool>> expression = t => t.InternalGroupName1 == groupName;
    }
}

public class NotFilledFilterHandler : IFilterHandler<Translation, NotFilledFilter>
{
    public Task<Expression<Func<Translation, bool>>> GetFilterExpressionAsync(NotFilledFilter filter, CancellationToken cancellationToken = default)
    {
        // Filter for translations where Content equals TranslationName
        // These are auto-created translations that haven't been filled with actual content yet
        Expression<Func<Translation, bool>> expression = t => t.Content == t.TranslationName;
        return Task.FromResult(expression);
    }
}
