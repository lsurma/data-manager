# React Host - Conditional MSAL Authentication

## Overview

The React host (`DataManager.Host.React`) now supports conditional MSAL (Microsoft Authentication Library) authentication. This allows you to run the application with or without Azure AD authentication based on your deployment needs.

## Configuration

### Environment Variable

The authentication mode is controlled by the `VITE_ENABLE_AUTH` environment variable in your `.env` file:

```bash
# Disable authentication (development mode)
VITE_ENABLE_AUTH=false

# Enable authentication (production mode)
VITE_ENABLE_AUTH=true
```

### Complete Configuration Examples

#### Without Authentication

```bash
# .env
VITE_ENABLE_AUTH=false
VITE_API_BASE_URL=http://localhost:7233/api
```

This is useful for:
- Local development without Azure AD setup
- Internal deployments where authentication is handled by other means
- Testing and demo scenarios
- Simplified development workflow

#### With Authentication

```bash
# .env
VITE_ENABLE_AUTH=true

# Azure AD Configuration
VITE_AZURE_CLIENT_ID=your-client-id-here
VITE_AZURE_TENANT_ID=your-tenant-id-here
VITE_REDIRECT_URI=http://localhost:5173
VITE_POST_LOGOUT_REDIRECT_URI=http://localhost:5173

# API Configuration
VITE_API_SCOPE=api://your-api-id/.default
VITE_API_BASE_URL=http://localhost:7233/api
```

## Implementation Details

### ConditionalAuthProvider Component

The `ConditionalAuthProvider` component (`src/auth/ConditionalAuthProvider.tsx`) conditionally wraps the application with `MsalProvider`:

```typescript
export function ConditionalAuthProvider({ children }: ConditionalAuthProviderProps) {
  if (!isAuthEnabled) {
    // Authentication is disabled - render children directly without MSAL
    return <>{children}</>
  }

  // Authentication is enabled - wrap with MsalProvider
  const msalInstance = new PublicClientApplication(msalConfig)
  
  return (
    <MsalProvider instance={msalInstance}>
      {children}
    </MsalProvider>
  )
}
```

### Authentication Status Logging

The application logs its authentication status in the browser console:

- **Authentication Disabled**: `ℹ️ Authentication is DISABLED`
- **Authentication Enabled**: `✅ Authentication is ENABLED`

This makes it easy to verify which mode the application is running in during development.

### Component Usage

Components that need authentication should check if authentication is enabled:

```typescript
import { useMsal } from '@azure/msal-react';
import { isAuthEnabled } from '@/auth/authConfig';

function MyComponent() {
  const { instance, accounts } = useMsal();
  
  if (!isAuthEnabled) {
    // Handle unauthenticated mode
    return <div>Running without authentication</div>;
  }
  
  // Handle authenticated mode
  if (accounts.length === 0) {
    return <button onClick={() => instance.loginRedirect()}>Sign In</button>;
  }
  
  return <div>Welcome, {accounts[0].name}</div>;
}
```

## API Calls

### With Authentication

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

### Without Authentication

```typescript
import { useQuery } from '@tanstack/react-query';
import { isAuthEnabled } from '@/auth/authConfig';

function useData() {
  return useQuery({
    queryKey: ['data'],
    queryFn: async () => {
      const headers: HeadersInit = {};
      
      if (!isAuthEnabled) {
        // Optionally add API key or other authentication headers
        headers['X-API-Key'] = 'your-api-key';
      }
      
      const result = await fetch(`${import.meta.env.VITE_API_BASE_URL}/endpoint`, {
        headers,
      });
      
      return result.json();
    },
  });
}
```

## Development Workflow

### Quick Start (No Authentication)

1. Create `.env` file:
   ```bash
   cp .env.example .env
   ```

2. Set authentication to false:
   ```bash
   echo "VITE_ENABLE_AUTH=false" > .env
   ```

3. Start the dev server:
   ```bash
   npm run dev
   ```

### Production Setup (With Authentication)

1. Create `.env` file:
   ```bash
   cp .env.example .env
   ```

2. Configure environment variables:
   ```bash
   VITE_ENABLE_AUTH=true
   VITE_AZURE_CLIENT_ID=<your-client-id>
   VITE_AZURE_TENANT_ID=<your-tenant-id>
   # ... other Azure AD settings
   ```

3. Build and deploy:
   ```bash
   npm run build
   ```

## Security Considerations

1. **Never commit `.env` files** - The `.env` file is in `.gitignore` to prevent accidental commits
2. **Use environment-specific settings** - Different environments should have different `.env` files
3. **API Security** - When authentication is disabled, ensure your API has alternative authentication (API keys, IP restrictions, etc.)
4. **Production Deployments** - Consider enabling authentication for production deployments unless there's a specific reason not to

## Troubleshooting

### Authentication Not Working

1. Check console for authentication status message
2. Verify `VITE_ENABLE_AUTH=true` in your `.env` file
3. Ensure Azure AD credentials are correctly configured
4. Check that redirect URIs match in Azure AD app registration

### Application Running Without Authentication When It Shouldn't

1. Check `.env` file for `VITE_ENABLE_AUTH` setting
2. Restart the dev server after changing `.env` files
3. Clear browser cache and sessionStorage

### MSAL Errors

If you see MSAL initialization errors:
1. Verify `VITE_AZURE_CLIENT_ID` and `VITE_AZURE_TENANT_ID` are set correctly
2. Check that the client ID and tenant ID match your Azure AD app registration
3. Ensure redirect URIs are configured in Azure AD

## Files Modified

- `src/auth/ConditionalAuthProvider.tsx` - New component for conditional authentication
- `src/auth/authConfig.ts` - Added `isAuthEnabled` flag and conditional validation
- `src/main.tsx` - Replaced `MsalProvider` with `ConditionalAuthProvider`
- `.env.example` - Added `VITE_ENABLE_AUTH` configuration
- `.gitignore` - Added `.env` to prevent accidental commits
- `README.md` - Updated documentation

## Related Documentation

- [MSAL.js Documentation](https://github.com/AzureAD/microsoft-authentication-library-for-js)
- [Azure AD App Registration Guide](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
- [Vite Environment Variables](https://vitejs.dev/guide/env-and-mode.html)
