# Optional<T> Pattern Documentation

## Overview

The `Optional<T>` type is a struct that wraps a value and tracks whether it was explicitly specified. This is useful for PATCH-style operations where you want to distinguish between:
- A value that wasn't provided (not specified)
- A value that was explicitly set to null
- A value that was set to a non-null value

## Basic Usage

### Creating Optional Values

```csharp
using DataManager.Application.Contracts.Common;

// Unspecified (default value)
var opt1 = default(Optional<string>);
var opt2 = Optional<string>.Unspecified();
Console.WriteLine(opt1.IsSpecified); // false

// Explicitly null
var opt3 = Optional<string>.Null();
Console.WriteLine(opt3.IsSpecified); // true
Console.WriteLine(opt3.Value); // null

// With a value
var opt4 = Optional<string>.Of("hello");
Console.WriteLine(opt4.IsSpecified); // true
Console.WriteLine(opt4.Value); // "hello"

// Implicit conversion
Optional<string> opt5 = "world";
Console.WriteLine(opt5.IsSpecified); // true
Console.WriteLine(opt5.Value); // "world"
```

### Using Optional in Commands

```csharp
// Create a command with only some fields specified
var command = new SaveSingleTranslationCommand
{
    Id = existingId,
    Content = "Updated content",  // Will update
    ResourceName = default,        // Won't update (unspecified)
    TranslationName = default      // Won't update (unspecified)
};
```

### Handling Optional in Handlers

```csharp
public async Task<Guid> Handle(SaveSingleTranslationCommand request, CancellationToken cancellationToken)
{
    var entity = await GetEntityAsync(request.Id);
    
    // Only update specified properties
    if (request.Content.IsSpecified)
        entity.Content = request.Content.Value;
    
    if (request.ResourceName.IsSpecified)
        entity.ResourceName = request.ResourceName.Value;
    
    // For creation, require certain properties
    if (entity is null)
    {
        if (!request.ResourceName.IsSpecified || string.IsNullOrEmpty(request.ResourceName.Value))
            throw new ArgumentException("ResourceName is required.");
        
        entity = new Entity
        {
            ResourceName = request.ResourceName.Value,
            Content = request.Content.GetValueOrDefault("default content")
        };
    }
    
    return entity.Id;
}
```

## JSON Serialization

The `Optional<T>` type includes automatic JSON serialization support via `OptionalJsonConverter`:

```csharp
var command = new SaveSingleTranslationCommand
{
    ResourceName = "Test",
    Content = "Hello"
    // Other properties left unspecified
};

var json = JsonSerializer.Serialize(command);
// Properties are included in JSON with null for unspecified values

var deserialized = JsonSerializer.Deserialize<SaveSingleTranslationCommand>(json);
// IsSpecified will be true for properties present in JSON
```

## Key Methods

### `IsSpecified`
Returns `true` if the value was explicitly set (even if set to null), `false` if unspecified.

### `Value`
Gets the wrapped value. Throws `InvalidOperationException` if not specified.

### `GetValueOrDefault(T? defaultValue = default)`
Returns the wrapped value if specified, otherwise returns the default value.

### Static Factory Methods
- `Optional<T>.Unspecified()` - Creates an unspecified optional
- `Optional<T>.Null()` - Creates an optional with null value
- `Optional<T>.Of(T? value)` - Creates an optional with the specified value

## Use Cases

### Partial Updates (PATCH)
When updating an entity, only modify fields that were explicitly provided:

```csharp
// Client sends only the fields they want to update
var updateCommand = new SaveSingleTranslationCommand
{
    Id = translationId,
    Content = "New content",  // Only update this field
    // All other fields unspecified - won't be touched
};
```

### Distinguishing Null from Unset
Sometimes you need to know if the user wants to clear a field (set to null) vs. not change it:

```csharp
// Clear the field
var clearCommand = new SaveSingleTranslationCommand
{
    Id = translationId,
    ContentTemplate = Optional<string?>.Null()  // Explicitly clear
};

// Don't touch the field
var keepCommand = new SaveSingleTranslationCommand
{
    Id = translationId,
    ContentTemplate = default  // Leave as-is
};
```

## Best Practices

1. **Use for update commands**: Optional<T> is most useful for commands that update existing entities
2. **Keep required fields simple**: For create operations, consider validation that checks `IsSpecified`
3. **Document behavior**: Clearly document whether unspecified fields retain their current values
4. **Implicit conversions**: Take advantage of implicit conversions for cleaner code
5. **GetValueOrDefault**: Use this method for safe access when a default makes sense

## Example: SaveSingleTranslationCommand

The `SaveSingleTranslationCommand` uses Optional<T> for all properties except `Id`:

```csharp
public class SaveSingleTranslationCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }  // Not Optional - determines create vs update
    
    public Optional<string?> InternalGroupName1 { get; set; }
    public Optional<string?> InternalGroupName2 { get; set; }
    public Optional<string> ResourceName { get; set; }
    public Optional<string> TranslationName { get; set; }
    public Optional<string> CultureName { get; set; }
    public Optional<string> Content { get; set; }
    public Optional<string?> ContentTemplate { get; set; }
    public Optional<Guid?> TranslationsSetId { get; set; }
    public Optional<Guid?> LayoutId { get; set; }
    public Optional<Guid?> SourceId { get; set; }
    public Optional<bool> IsDraftVersion { get; set; }
}
```

This allows for true partial updates where only the specified fields are modified.

## Security Considerations

- **Validation**: Always validate that required fields are specified when creating new entities
- **Authorization**: Check permissions before allowing updates, even for partial updates
- **Audit**: Consider logging which fields were updated for audit purposes
