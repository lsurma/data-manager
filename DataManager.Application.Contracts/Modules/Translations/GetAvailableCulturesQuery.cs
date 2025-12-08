using MediatR;

namespace DataManager.Application.Contracts.Modules.Translations;

public record GetAvailableCulturesQuery : IRequest<List<string>>;
