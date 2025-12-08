using System.Reflection;
using System.Text.RegularExpressions;
using MediatR;

namespace DataManager.Host.AzFuncAPI.Services;

public class RequestRegistry
{
    private readonly Dictionary<string, Type> _requestTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Type> _genericRequestTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Assembly _contractsAssembly;
    
    // Suffixes to try when looking up request types (in order of priority)
    private static readonly string[] SuffixVariations = { "", "Query", "Command" };
    
    private const string QuerySuffix = "Query";
    private const string CommandSuffix = "Command";
    private const string PaginatedQueryTypeName = "PaginatedQuery";

    public RequestRegistry()
    {
        _contractsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "DataManager.Application.Contracts")
            ?? throw new InvalidOperationException("DataManager.Application.Contracts assembly not found");

        ScanForRequests();
    }

    private void ScanForRequests()
    {
        var requestTypes = _contractsAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));

        foreach (var type in requestTypes)
        {
            if (type.IsGenericTypeDefinition)
            {
                // Store open generic types (e.g., GetTranslationsQuery<>)
                var genericTypeName = type.Name;
                var backtickIndex = genericTypeName.IndexOf('`');
                if (backtickIndex > 0)
                {
                    genericTypeName = genericTypeName.Substring(0, backtickIndex);
                }
                _genericRequestTypes[genericTypeName] = type;
            }
            else
            {
                // Store concrete types (e.g., GetTranslationsQuery)
                _requestTypes[type.Name] = type;
            }
        }
    }

    public Type? GetRequestType(string requestName)
    {
        // Try direct lookup for non-generic types with suffix variations
        foreach (var suffix in SuffixVariations)
        {
            if (_requestTypes.TryGetValue(requestName + suffix, out var type))
            {
                return type;
            }
        }

        // Check if it's a generic type request (format: "TypeName<Arg1,Arg2>")
        var match = Regex.Match(requestName, @"^(\w+)<(.+)>$");
        if (!match.Success)
        {
            return null;
        }

        var genericTypeName = match.Groups[1].Value;
        var genericArgsString = match.Groups[2].Value;

        // Try to find the open generic type with suffix variations
        Type? openGenericType = null;
        foreach (var suffix in SuffixVariations)
        {
            if (_genericRequestTypes.TryGetValue(genericTypeName + suffix, out openGenericType))
            {
                break;
            }
        }

        if (openGenericType == null)
        {
            return null;
        }

        // Parse generic type arguments
        var genericArgNames = genericArgsString.Split(',').Select(s => s.Trim()).ToArray();

        // Resolve generic type arguments from the Contracts assembly
        var genericArgs = new Type[genericArgNames.Length];
        for (int i = 0; i < genericArgNames.Length; i++)
        {
            var argType = _contractsAssembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(genericArgNames[i], StringComparison.OrdinalIgnoreCase));

            if (argType == null)
            {
                return null; // Generic argument type not found
            }

            genericArgs[i] = argType;
        }

        // Construct the closed generic type
        return openGenericType.MakeGenericType(genericArgs);
    }

    public IEnumerable<string> GetAllRequestNames()
    {
        var names = new List<string>();
        names.AddRange(_requestTypes.Keys);
        names.AddRange(_genericRequestTypes.Keys.Select(name => $"{name}<...>"));
        return names;
    }

    /// <summary>
    /// Determines if a request type is a Command (ends with "Command")
    /// </summary>
    public bool IsCommandType(Type requestType)
    {
        var typeName = GetBaseTypeName(requestType);
        return typeName.EndsWith(CommandSuffix);
    }

    /// <summary>
    /// Determines if a request type is a Query (ends with "Query" or is a PaginatedQuery)
    /// </summary>
    public bool IsQueryType(Type requestType)
    {
        var typeName = GetBaseTypeName(requestType);
        return typeName.EndsWith(QuerySuffix) || typeName == PaginatedQueryTypeName;
    }

    /// <summary>
    /// Gets the base type name without generic arity marker for generic types
    /// </summary>
    private static string GetBaseTypeName(Type requestType)
    {
        var typeName = requestType.Name;
        if (requestType.IsGenericType)
        {
            var backtickIndex = typeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                typeName = typeName.Substring(0, backtickIndex);
            }
        }
        return typeName;
    }
}
