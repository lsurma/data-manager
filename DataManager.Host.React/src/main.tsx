import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider, createRouter, createRootRoute, createRoute, Outlet } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { ConditionalAuthProvider } from './auth/ConditionalAuthProvider'

// Create a new QueryClient instance
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 1,
    },
  },
})

// Root layout component
function RootComponent() {
  return (
    <div className="min-h-screen bg-background">
      <nav className="border-b">
        <div className="container mx-auto px-4 py-4">
          <h1 className="text-xl font-bold">DataManager</h1>
        </div>
      </nav>
      <main className="container mx-auto px-4 py-8">
        <Outlet />
      </main>
      <TanStackRouterDevtools />
    </div>
  )
}

// Index page component
function IndexComponent() {
  return (
    <div>
      <h1 className="text-4xl font-bold mb-4">DataManager React App</h1>
      <p className="text-lg mb-8">Welcome to the DataManager React application!</p>
      <div className="space-y-4">
        <div className="p-6 bg-blue-50 dark:bg-blue-950 rounded-lg border border-blue-200 dark:border-blue-800">
          <h2 className="text-2xl font-semibold mb-4">Features</h2>
          <ul className="list-disc list-inside space-y-2">
            <li>React with TypeScript</li>
            <li>Vite for fast development</li>
            <li>Tailwind CSS with shadcn/ui components</li>
            <li>MSAL.JS for Azure AD authentication</li>
            <li>TanStack Router for routing</li>
            <li>TanStack Query for data fetching</li>
          </ul>
        </div>
      </div>
    </div>
  )
}

// Create routes
const rootRoute = createRootRoute({
  component: RootComponent,
})

const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: IndexComponent,
})

const routeTree = rootRoute.addChildren([indexRoute])

// Create a new router instance
const router = createRouter({ routeTree })

// Register the router instance for type safety
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConditionalAuthProvider>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </ConditionalAuthProvider>
  </StrictMode>,
)
