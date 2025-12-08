using DataManager.Application.Contracts.Common;
using MediatR;

namespace DataManager.Application.Contracts;

public interface IRequestSender
{
    public Task<TResponse> SendAsync<TResponse>(object request, CancellationToken cancellationToken = default);

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from the API and returns the file content with metadata (MIME type, filename)
    /// </summary>
    public Task<DownloadedFile> DownloadFileAsync(object request, CancellationToken cancellationToken = default);
}