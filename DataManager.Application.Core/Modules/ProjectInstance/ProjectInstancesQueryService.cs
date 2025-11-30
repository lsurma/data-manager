using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;

namespace DataManager.Application.Core.Modules.ProjectInstance;

/// <summary>
/// Query service for ProjectInstance entities
/// </summary>
public class ProjectInstancesQueryService : QueryService<ProjectInstance, Guid>
{
    private readonly DataManagerDbContext _context;

    public ProjectInstancesQueryService(
        DataManagerDbContext context,
        IFilterHandlerRegistry filterHandlerRegistry)
        : base(filterHandlerRegistry)
    {
        _context = context;
    }

    protected override IQueryable<ProjectInstance> DefaultQuery => _context.ProjectInstances;
}
