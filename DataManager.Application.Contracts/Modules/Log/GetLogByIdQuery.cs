using MediatR;

namespace DataManager.Application.Contracts.Modules.Log;

public record GetLogByIdQuery(Guid Id) : IRequest<LogDto?>;
