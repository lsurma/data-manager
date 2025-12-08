# AI Translation Test Page - UI Overview

## Page Layout

```
┌─────────────────────────────────────────────────────────────┐
│                                                               │
│  AI Translation Test                                          │
│  Test OpenRouter AI integration for translating text         │
│  between cultures.                                            │
│                                                               │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━    │
│                                                               │
│  Source Text                                                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ Welcome to our online store! Browse our latest       │  │
│  │ products and enjoy free shipping on orders over $50. │  │
│  │                                                       │  │
│  │                                                       │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
│  Source Culture          Context (optional)                  │
│  ┌─────────────────┐    ┌─────────────────────────────┐     │
│  │ en-US           │    │ ecommerce                   │     │
│  └─────────────────┘    └─────────────────────────────┘     │
│                                                               │
│  Target Cultures (comma-separated)                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ pl-PL, de-DE                                          │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌─────────────┐                                             │
│  │  Translate  │                                             │
│  └─────────────┘                                             │
│                                                               │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━    │
│                                                               │
│  Translation Results                                          │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ pl-PL                                                 │  │
│  │                                                       │  │
│  │ Witamy w naszym sklepie internetowym! Przeglądaj     │  │
│  │ nasze najnowsze produkty i ciesz się darmową          │  │
│  │ dostawą przy zamówieniach powyżej 50 USD.            │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ de-DE                                                 │  │
│  │                                                       │  │
│  │ Willkommen in unserem Online-Shop! Durchstöbern Sie  │  │
│  │ unsere neuesten Produkte und genießen Sie kostenlosen│  │
│  │ Versand bei Bestellungen über 50 $.                  │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Features

- **Text Input Area**: Multi-line text area for entering source text
- **Source Culture**: Text field for entering source language culture code (e.g., "en-US")
- **Context**: Optional text field for providing translation context (e.g., "ecommerce", "technical")
- **Target Cultures**: Comma-separated list of target culture codes (e.g., "pl-PL, de-DE, es-ES")
- **Translate Button**: Triggers the translation process (shows loading indicator while processing)
- **Results Display**: Shows translated text for each target culture in separate cards

## Navigation

- Available in the left navigation menu under "AI Translation Test"
- Only visible when dev pages are enabled (set `_dev` cookie)

## Error Handling

- Validates required fields before sending request
- Displays error messages if translation fails
- Shows user-friendly feedback for network or API errors
