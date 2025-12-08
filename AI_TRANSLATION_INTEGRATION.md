# AI Translation Integration

## Overview

This project integrates with [OpenRouter AI](https://openrouter.ai/) to provide AI-powered translation capabilities. The integration allows translating text from one language to multiple target languages using free or paid AI models.

**Key Features:**
- **Single API request** for multiple target languages (efficient and cost-effective)
- **Prompt injection protection** to prevent malicious input manipulation
- Separator-based response parsing for reliable multi-language results

## Architecture

The AI integration follows the CQRS pattern used throughout the DataManager application:

- **DataManager.AI.Core**: Separate project containing AI services and handlers
- **DataManager.Application.Contracts**: Contains the `TranslateTextCommand` and `TranslateTextResult` DTOs
- **OpenRouter Integration**: HTTP-based service communicating with OpenRouter API

## Setup

### 1. Get an OpenRouter API Key

1. Visit [OpenRouter](https://openrouter.ai/)
2. Sign up for an account
3. Navigate to the API Keys section
4. Generate a new API key

### 2. Configure the Application

Create or update `DataManager.Host.AzFuncAPI/local.settings.json`:

```json
{
  "OpenRouter": {
    "ApiKey": "YOUR_OPENROUTER_API_KEY_HERE",
    "Model": "google/gemma-2-9b-it:free"
  }
}
```

**Note**: The application uses free models by default (`google/gemma-2-9b-it:free`). You can change this to any model supported by OpenRouter.

### 3. Available Free Models

OpenRouter provides several free models:
- `google/gemma-2-9b-it:free` (default)
- `meta-llama/llama-3.2-3b-instruct:free`
- `qwen/qwen-2-7b-instruct:free`

Check [OpenRouter Models](https://openrouter.ai/models) for the latest list of available models.

## Usage

### API Command

```csharp
var command = new TranslateTextCommand
{
    Text = "Welcome to our store!",
    SourceCulture = "en-US",
    TargetCultures = new List<string> { "pl-PL", "de-DE", "es-ES" },
    Context = "ecommerce" // Optional: helps improve translation quality
};

var result = await _mediator.Send(command);

// result.Translations contains a dictionary of culture codes to translated text
// e.g., { "pl-PL": "Witamy w naszym sklepie!", "de-DE": "...", "es-ES": "..." }
```

### Test Page

A test page is available at `/ai-translation-test` (visible in dev mode) where you can:
1. Enter text to translate
2. Specify source culture (e.g., "en-US")
3. Specify target cultures (comma-separated, e.g., "pl-PL, de-DE")
4. Optionally provide context (e.g., "ecommerce", "technical", "casual")
5. Click "Translate" to see results

## Context Parameter

The optional `Context` parameter helps the AI understand the domain:
- `"ecommerce"` (default): For online store, product descriptions, marketing
- `"technical"`: For technical documentation, software UI
- `"casual"`: For informal communication
- Custom contexts: Any text describing your use case

## Features

- **Single API Request**: All target languages are processed in one request (cost-effective and efficient)
- **Multi-line Text Support**: Preserves all line breaks, newlines, and formatting exactly as in the original text
- **Prompt Injection Protection**: Sanitizes inputs to prevent malicious prompt manipulation
- **HTML Preservation**: HTML tags and attributes are not translated, only text content between tags
- **Template Variable Preservation**: Content in curly braces (e.g., `{{ data.name }}`, `{{ user.email }}`) is preserved as-is
- **Separator-Based Parsing**: Uses `---TRANSLATION_SEPARATOR---` control line between translations for reliable parsing
- **Fallback Mechanisms**: Multiple parsing strategies for compatibility
- **Context-Aware**: Provide optional context to improve translation quality
- **Free Models**: Uses free OpenRouter models by default
- **Automatic Routing**: Commands are auto-discovered via the CQRS request registry

## Translation Rules

The AI translator follows these rules:
1. **HTML Tags**: Not translated - kept exactly as they are (e.g., `<div>`, `<span class="...">``)
2. **Template Variables**: Content inside `{{ }}` is preserved (e.g., `{{ data.name }}`, `{{ user.email }}`)
3. **Text Content**: Only actual text between HTML tags and outside template variables is translated
4. **Formatting**: ALL line breaks, newlines, spacing, and special characters are preserved exactly
5. **Multi-line Structure**: If the input has multiple lines, the translation will maintain the same line structure

### Example - Simple Text

**Input (English):**
```html
<div class="welcome">Welcome {{ user.name }}! You have {{ notification.count }} new messages.</div>
```

**Output (Polish):**
```html
<div class="welcome">Witamy {{ user.name }}! Masz {{ notification.count }} nowych wiadomości.</div>
```

### Example - Multi-line Text

**Input (English):**
```html
<div>
  <h1>Welcome Back!</h1>
  <p>Hello {{ user.name }},</p>
  <p>You have been away for {{ days.count }} days.</p>
</div>
```

**Output (Polish):**
```html
<div>
  <h1>Witamy z powrotem!</h1>
  <p>Cześć {{ user.name }},</p>
  <p>Byłeś nieobecny przez {{ days.count }} dni.</p>
</div>
```

Note: All HTML tags, template variables, and line structure remain unchanged.

## Security

The service includes prompt injection protection:
- Removes common injection patterns (`ignore previous instructions`, `disregard`, etc.)
- Strips role indicators (`system:`, `assistant:`, `user:`)
- Limits input length to 5000 characters per field
- Sanitizes all user inputs (text, cultures, context)

## Implementation Details

### Project Structure

```
DataManager.AI.Core/
├── Extensions/
│   └── ServiceCollectionExtensions.cs    # DI registration
├── Handlers/
│   └── TranslateTextCommandHandler.cs    # MediatR command handler
└── Services/
    ├── IOpenRouterService.cs             # Service interface
    └── OpenRouterService.cs              # OpenRouter API client
```

### Service Registration

The AI services are registered in `Program.cs`:

```csharp
builder.Services.AddAIServices();
```

This registers:
- `IOpenRouterService` with HttpClient
- MediatR handlers from the AI.Core assembly

### Error Handling

The service includes comprehensive error handling:
- If translation fails, the original text is returned for all cultures
- Errors are logged using `ILogger<T>` for proper monitoring
- Network issues are caught and handled gracefully
- Incomplete AI responses are handled with fallback parsing strategies

### Response Parsing

The AI response is parsed using multiple strategies to ensure reliability:
1. **Primary**: Split by `---TRANSLATION_SEPARATOR---` control line (on its own line)
2. **Fallback 1**: Split by legacy `---` separator (for backwards compatibility)
3. **Fallback 2**: Split by triple newlines (`\n\n\n`)

**Important**: The parsing preserves multi-line text structure by only trimming leading/trailing carriage returns and newlines, while maintaining all internal formatting, line breaks, and whitespace exactly as returned by the AI.

This ensures reliable parsing even if the AI doesn't follow the exact format, while maintaining the integrity of multi-line translations.

## Future Enhancements

Possible improvements:
- Translation caching to reduce API calls
- Model selection per request
- Translation quality metrics
- Support for more AI providers
- Configurable input sanitization rules
