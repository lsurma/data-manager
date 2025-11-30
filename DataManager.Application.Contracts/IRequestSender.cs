using MediatR;

namespace DataManager.Application.Contracts;

public interface IRequestSender
{
    public Task<TResponse> SendAsync<TResponse>(object request, CancellationToken cancellationToken = default);

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    
}