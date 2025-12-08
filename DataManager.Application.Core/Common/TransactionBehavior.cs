using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataManager.Application.Core.Common;

/// <summary>
/// MediatR pipeline behavior that wraps command handlers in a database transaction.
/// Ensures all operations within a handler are executed atomically.
/// Only applies to commands (requests that return a value), not queries.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly DataManagerDbContext _context;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        DataManagerDbContext context,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip transaction for queries (by convention, queries don't modify data)
        // We identify queries by checking if the request type name contains "Query"
        var requestName = typeof(TRequest).Name;
        if (requestName.Contains("Query", StringComparison.OrdinalIgnoreCase))
        {
            return await next();
        }

        // Check if we're already in a transaction (nested behavior calls)
        if (_context.Database.CurrentTransaction != null)
        {
            return await next();
        }

        // Start a new transaction
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogDebug(
                    "Starting transaction for {RequestName}",
                    requestName);

                var response = await next();

                await _context.SaveChangesAsync(true, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogDebug(
                    "Committed transaction for {RequestName}",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Transaction failed for {RequestName}, rolling back",
                    requestName);

                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
