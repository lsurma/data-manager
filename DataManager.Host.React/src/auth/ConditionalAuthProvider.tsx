import { type ReactNode } from 'react'
import { MsalProvider } from '@azure/msal-react'
import { PublicClientApplication } from '@azure/msal-browser'
import { msalConfig, isAuthEnabled } from './authConfig'

interface ConditionalAuthProviderProps {
  children: ReactNode
}

/**
 * Conditionally wraps children with MsalProvider based on VITE_ENABLE_AUTH setting.
 * If authentication is disabled, children are rendered directly without MSAL.
 */
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
