using MediatR;

namespace DataManager.Application.Contracts.Modules.ProjectInstance;

public class GetProjectInstanceByIdQuery : IRequest<ProjectInstanceDto?>
{
    public GetProjectInstanceByIdQuery(Guid id)
    {
        Id = id;
    }
    
    public Guid Id { get; }
}
