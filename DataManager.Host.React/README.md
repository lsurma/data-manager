# DataManager.Host.React

React-based frontend for DataManager application.

## Tech Stack

- **React 19** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **Tailwind CSS** - Utility-first CSS framework
- **shadcn/ui** - Re-usable component library
- **MSAL.JS** - Microsoft Authentication Library for Azure AD
- **TanStack Router** - Type-safe routing
- **TanStack Query** - Data fetching and caching
- **nuqs** - Type-safe URL state management
- **Zustand** - State management for UI layers

## Getting Started

### Prerequisites

- Node.js 20+ and npm

### Installation

```bash
npm install
```

### Configuration

Copy `.env.example` to `.env` and configure your environment variables:

```bash
cp .env.example .env
```

#### Required Variables

- `VITE_ENABLE_AUTH` - Enable/disable MSAL authentication (default: `false`)
- `VITE_API_BASE_URL` - Backend API URL (default: http://localhost:7233/api)

#### Authentication Variables (only required if `VITE_ENABLE_AUTH=true`)

- `VITE_AZURE_CLIENT_ID` - Your Azure AD application client ID
- `VITE_AZURE_TENANT_ID` - Your Azure AD tenant ID
- `VITE_REDIRECT_URI` - Redirect URI after login (default: http://localhost:5173)
- `VITE_POST_LOGOUT_REDIRECT_URI` - Redirect URI after logout (default: http://localhost:5173)
- `VITE_API_SCOPE` - API scope for token acquisition (default: api://your-api-id/.default)

#### Running Without Authentication

To run the application without authentication (useful for development or internal deployments):

```bash
# In .env file
VITE_ENABLE_AUTH=false
```

When authentication is disabled, the app will run without MSAL integration, and no Azure AD configuration is required.

### Development

```bash
npm run dev
```

The app will be available at http://localhost:5173

### Build

```bash
npm run build
```

### Preview Production Build

```bash
npm run preview
```

## Project Structure

```
src/
├── auth/              # MSAL authentication configuration
├── components/        # React components
│   └── ui/           # shadcn/ui components
├── hooks/            # Custom React hooks
├── lib/              # Utility functions
├── routes/           # TanStack Router routes
├── stores/           # Zustand stores
├── index.css         # Global styles and Tailwind directives
└── main.tsx          # Application entry point
```

## Key Features

### Layer Management System

The application includes a robust layer management system for handling multiple overlays (modals, drawers, dialogs). Key features:

- Stack-based layer tracking (LIFO)
- ESC key closes only the topmost layer
- Easy integration with the `useLayer` hook
- Automatic cleanup on component unmount

See [LAYER_MANAGEMENT.md](./LAYER_MANAGEMENT.md) for full documentation and examples.

**Demo:** Visit `/layer-demo` to see the system in action.

## Adding shadcn/ui Components

To add new shadcn/ui components:

```bash
npx shadcn@latest add <component-name>
```

For example:
```bash
npx shadcn@latest add button
npx shadcn@latest add card
```

## Authentication

The app supports optional MSAL.js authentication for Azure AD. You can enable or disable authentication based on your deployment needs.

### Enabling Authentication

Set `VITE_ENABLE_AUTH=true` in your `.env` file and configure your Azure AD app registration with:
- Redirect URI: http://localhost:5173
- Front-channel logout URL: http://localhost:5173

### Disabling Authentication

Set `VITE_ENABLE_AUTH=false` in your `.env` file to run the application without authentication. This is useful for:
- Local development without Azure AD setup
- Internal deployments where authentication is handled by other means
- Testing scenarios

## API Integration

API calls should use TanStack Query with MSAL token acquisition. Example:

```typescript
import { useMsal } from '@azure/msal-react';
import { useQuery } from '@tanstack/react-query';
import { apiRequest } from '@/auth/authConfig';

function useData() {
  const { instance, accounts } = useMsal();
  
  return useQuery({
    queryKey: ['data'],
    queryFn: async () => {
      const response = await instance.acquireTokenSilent({
        ...apiRequest,
        account: accounts[0],
      });
      
      const result = await fetch(`${import.meta.env.VITE_API_BASE_URL}/endpoint`, {
        headers: {
          'Authorization': `Bearer ${response.accessToken}`,
        },
      });
      
      return result.json();
    },
  });
}
```

## URL State Management with nuqs

The application uses [nuqs](https://nuqs.47ng.com) for type-safe URL state management. This allows you to store component state in the URL query parameters, making it easy to share application state via URLs.

### Basic Usage

```typescript
import { useQueryState, parseAsInteger, parseAsString } from 'nuqs'

function MyComponent() {
  // String state in URL
  const [name, setName] = useQueryState('name', parseAsString.withDefault(''))
  
  // Integer state in URL
  const [count, setCount] = useQueryState('count', parseAsInteger.withDefault(0))
  
  return (
    <div>
      <input value={name} onChange={(e) => setName(e.target.value)} />
      <button onClick={() => setCount(c => c + 1)}>Count: {count}</button>
    </div>
  )
}
```

### Available Parsers

- `parseAsString` - String values
- `parseAsInteger` - Integer numbers
- `parseAsFloat` - Floating point numbers
- `parseAsBoolean` - Boolean values
- `parseAsTimestamp` - Date objects (Unix timestamp)
- `parseAsIsoDateTime` - Date objects (ISO 8601)
- `parseAsArrayOf` - Arrays of values
- `parseAsJson` - JSON objects
- `parseAsStringEnum` - String enums
- `parseAsStringLiteral` - String literals
- `parseAsNumberLiteral` - Number literals

### Demo

Visit `/nuqs-demo` in the application to see an interactive demonstration of nuqs functionality.

### Benefits

- **Type-safe**: Built-in parsers ensure type safety
- **Simple API**: Works like `useState` but persists in URL
- **Shareable**: Copy URL to share exact application state
- **Browser history**: Use back/forward buttons to navigate state
- **Framework agnostic**: Works with TanStack Router, Next.js, Remix, and more
