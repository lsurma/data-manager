using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace DataManager.Application.Core.Common;

public class FilterHandlerRegistry : IFilterHandlerRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IFilterHandler> _filterHandlers;
    private readonly ConcurrentDictionary<Type, Dictionary<Type, object>> _handlerCache = new();

    public FilterHandlerRegistry(
        IServiceProvider serviceProvider,
        IEnumerable<IFilterHandler> filterHandlers
    )
    {
        _serviceProvider = serviceProvider;
        _filterHandlers = filterHandlers;
    }

    public Dictionary<Type, object> GetHandlersForEntity<TEntity>()
    {
        var entityType = typeof(TEntity);
        return _handlerCache.GetOrAdd(entityType, _ => BuildHandlersForEntity<TEntity>());
    }

    private Dictionary<Type, object> BuildHandlersForEntity<TEntity>()
    {
        var handlers = new Dictionary<Type, object>();
        var filterHandlerType = typeof(IFilterHandler<,>);

        foreach (var handler in _filterHandlers)
        {
            var handlerType = handler.GetType();
            var matchingInterfaces = handlerType
                .GetInterfaces()
                .Where(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == filterHandlerType &&
                    interfaceType.GetGenericArguments()[0] == typeof(TEntity)
                )
                .ToList();

            if (!matchingInterfaces.Any())
            {
                continue;
            }

            var filterType = matchingInterfaces.First().GetGenericArguments()[1];
            var handlerInstance = _serviceProvider.GetService(handlerType) ?? Activator.CreateInstance(handlerType);

            if (handlerInstance != null)
            {
                handlers[filterType] = handlerInstance;
            }
        }

        return handlers;
    }
}
