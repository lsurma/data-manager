/* eslint-disable react-refresh/only-export-components */
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider, createRouter, createRootRoute, createRoute, Outlet, Link } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { ConditionalAuthProvider } from './auth/ConditionalAuthProvider'
import { NuqsAdapter } from 'nuqs/adapters/tanstack-router'
import { NuqsDemo } from './routes/NuqsDemo'
import { LayerManagementDemo } from './routes/LayerManagementDemo'

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
        <div className="container mx-auto px-4 py-4 flex items-center gap-6">
          <h1 className="text-xl font-bold">DataManager</h1>
          <div className="flex gap-4">
            <Link 
              to="/" 
              className="text-sm hover:text-blue-600 [&.active]:font-bold [&.active]:text-blue-600"
            >
              Home
            </Link>
            <Link 
              to="/nuqs-demo" 
              className="text-sm hover:text-blue-600 [&.active]:font-bold [&.active]:text-blue-600"
            >
              nuqs Demo
            </Link>
            <Link 
              to="/layer-demo" 
              className="text-sm hover:text-blue-600 [&.active]:font-bold [&.active]:text-blue-600"
            >
              Layer Management
            </Link>
          </div>
        </div>
      </nav>
      <main className="container mx-auto px-4 py-8">
        <NuqsAdapter>
          <Outlet />
        </NuqsAdapter>
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
            <li>nuqs for URL state management</li>
            <li>Zustand for state management</li>
          </ul>
        </div>
        
        <div className="p-6 bg-green-50 dark:bg-green-950 rounded-lg border border-green-200 dark:border-green-800">
          <h2 className="text-2xl font-semibold mb-4">Try nuqs!</h2>
          <p className="mb-4">
            Check out the interactive demo to see how nuqs manages state in the URL.
          </p>
          <Link 
            to="/nuqs-demo" 
            className="inline-block px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md font-medium"
          >
            Go to nuqs Demo →
          </Link>
        </div>
        
        <div className="p-6 bg-purple-50 dark:bg-purple-950 rounded-lg border border-purple-200 dark:border-purple-800">
          <h2 className="text-2xl font-semibold mb-4">Try Layer Management!</h2>
          <p className="mb-4">
            Test the reusable layer management system with drawers, modals, and dialogs. See how ESC key handling works with multiple overlays.
          </p>
          <Link 
            to="/layer-demo" 
            className="inline-block px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-md font-medium"
          >
            Go to Layer Demo →
          </Link>
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

const nuqsDemoRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/nuqs-demo',
  component: NuqsDemo,
})

const layerDemoRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/layer-demo',
  component: LayerManagementDemo,
})

const routeTree = rootRoute.addChildren([indexRoute, nuqsDemoRoute, layerDemoRoute])

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
