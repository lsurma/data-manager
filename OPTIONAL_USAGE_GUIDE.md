# Optional<T> Class Usage Guide

## Overview

The `Optional<T>` class provides a way to distinguish between:
- A property that was explicitly set to a value (including null)
- A property that was not provided at all

This is particularly useful when working with PATCH/UPDATE operations where you need to know which fields the client wants to update vs. fields they didn't mention.

## Why Use Optional<T>?

Regular nullable types (`string?`, `int?`, etc.) cannot distinguish between:
- Property not provided: `{}`
- Property explicitly set to null: `{"Name": null}`

Both cases would result in the property being `null`. With `Optional<T>`, you can tell the difference:
- Property not provided: `Optional<T>.IsSet = false`
- Property explicitly set to null: `Optional<T>.IsSet = true`, `Value = null`
- Property set to a value: `Optional<T>.IsSet = true`, `Value = "something"`

## Basic Usage

### 1. Define a DTO with Optional Properties

```csharp
using DataManager.Application.Contracts.Common;

public class UpdateUserCommand
{
    // Regular required field
    public Guid Id { get; set; }
    
    // Optional fields - only update if provided
    public Optional<string> Name { get; set; }
    
    public Optional<string?> Email { get; set; }
    
    public Optional<int> Age { get; set; }
}
```

**Note:** The `OptionalJsonConverterFactory` is registered globally in the application, so you **do not need** to add the `[JsonConverter]` attribute to each property. The converter will be applied automatically during JSON serialization/deserialization.

### 2. Deserialize JSON

```csharp
var json = """
{
    "Id": "123e4567-e89b-12d3-a456-426614174000",
    "Name": "John Doe",
    "Email": null
}
""";

var command = JsonSerializer.Deserialize<UpdateUserCommand>(json);

// Check which fields were provided
command.Id                    // Always available: Guid value
command.Name.IsSet            // true (was provided)
command.Name.Value            // "John Doe"
command.Email.IsSet           // true (was provided, but as null)
command.Email.Value           // null
command.Age.IsSet             // false (was NOT provided)
```

### 3. Using Optional Values in Command Handlers

```csharp
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(request.Id);
        
        // Only update fields that were provided
        if (request.Name.IsSet)
        {
            user.Name = request.Name.Value;
        }
        
        if (request.Email.IsSet)
        {
            user.Email = request.Email.Value; // Can be null
        }
        
        if (request.Age.IsSet)
        {
            user.Age = request.Age.Value;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

## API Reference

### Properties

- **`IsSet`**: `bool` - Indicates whether the value has been set (even if set to null)
- **`Value`**: `T?` - Gets the value if set, otherwise throws `InvalidOperationException`

### Methods

- **`GetValueOrDefault()`**: Returns the value if set, otherwise returns `default(T)`
- **`GetValueOrDefault(T defaultValue)`**: Returns the value if set, otherwise returns the provided default
- **`Unset()`**: Static method that creates an unset Optional

### Operators

- **Implicit conversion from `T` to `Optional<T>`**: You can assign a value directly
  ```csharp
  Optional<string> name = "John"; // Implicit conversion
  ```

- **Explicit conversion from `Optional<T>` to `T`**: Use cast to get the value
  ```csharp
  string name = (string)optionalName; // May throw if not set
  ```

## Examples

### Example 1: Basic Usage

```csharp
public class Input
{
    public Optional<string> Name { get; set; }
}

// Deserialize with value
var json1 = """{"Name": "test"}""";
var obj1 = JsonSerializer.Deserialize<Input>(json1);
Console.WriteLine(obj1.Name.IsSet);   // True
Console.WriteLine(obj1.Name.Value);   // "test"

// Deserialize without value
var json2 = """{}""";
var obj2 = JsonSerializer.Deserialize<Input>(json2);
Console.WriteLine(obj2.Name.IsSet);   // False
```

### Example 2: Handling Null Values

```csharp
public class UserProfile
{
    public Optional<string?> Bio { get; set; }
}

// User explicitly clears their bio (sets to null)
var json1 = """{"Bio": null}""";
var profile1 = JsonSerializer.Deserialize<UserProfile>(json1);
profile1.Bio.IsSet    // true
profile1.Bio.Value    // null

// User doesn't update their bio (field not provided)
var json2 = """{}""";
var profile2 = JsonSerializer.Deserialize<UserProfile>(json2);
profile2.Bio.IsSet    // false
```

### Example 3: Selective Updates

```csharp
public record UpdateTranslationCommand
{
    public Guid Id { get; set; }
    
    public Optional<string> Content { get; set; }
    
    public Optional<string?> ContentTemplate { get; set; }
    
    public Optional<Guid?> LayoutId { get; set; }
}

// Only update Content, leave ContentTemplate and LayoutId unchanged
var json = """
{
    "Id": "...",
    "Content": "Updated content"
}
""";

var command = JsonSerializer.Deserialize<UpdateTranslationCommand>(json);
// command.Content.IsSet == true
// command.ContentTemplate.IsSet == false
// command.LayoutId.IsSet == false
```

## Best Practices

1. **The converter is registered globally**: No attributes needed on properties. The `OptionalJsonConverterFactory` is automatically applied during JSON serialization/deserialization.

2. **Check IsSet before accessing Value**: Always check `IsSet` before accessing `Value` to avoid exceptions:
   ```csharp
   if (optional.IsSet)
   {
       var value = optional.Value;
       // Use value
   }
   ```

3. **Use GetValueOrDefault for safety**: If you want a default value when unset:
   ```csharp
   var name = optional.GetValueOrDefault("Unknown");
   ```

4. **Use Optional<T?> for nullable types**: When the actual value can be null, use `Optional<string?>` or `Optional<int?>`:
   ```csharp
   public Optional<string?> OptionalNullableString { get; set; }
   ```

5. **Don't use Optional for required fields**: Only use Optional for fields that are truly optional in your API. Required fields should use regular types.

## Troubleshooting

### Optional.Value throws InvalidOperationException

**Problem**: Accessing `Value` when `IsSet` is false throws an exception.

**Solution**: Always check `IsSet` first, or use `GetValueOrDefault()`:
```csharp
// Bad
var value = optional.Value; // May throw

// Good
if (optional.IsSet)
{
    var value = optional.Value;
}

// Or use
var value = optional.GetValueOrDefault();
```

### JSON deserialization doesn't work

**Problem**: Optional properties are always unset after deserialization.

**Solution**: The converter is registered globally. If deserialization doesn't work, check that the JsonSerializerConfig is being used:
- For backend API controllers: Ensure `JsonSerializerConfig.Default` is used (automatically configured in all controllers)
- For custom code: Use `JsonSerializer.Deserialize<T>(json, JsonSerializerConfig.Default)`
- For frontend: Use `JsonSerializerConfig.Default` from `DataManager.Host.WA.Services`

```csharp
// Example usage
var obj = JsonSerializer.Deserialize<MyClass>(json, JsonSerializerConfig.Default);
```

### All Optional properties serialize as null

**Problem**: When serializing objects with Optional properties, unset values appear as `null` in JSON.

**Solution**: This is expected behavior. The JSON serializer includes all properties. If you want to omit unset properties, you can configure JsonSerializerOptions:
```csharp
var options = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
};
```

Note: This will omit all properties with default values, not just unset Optionals.
