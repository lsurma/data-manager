import { useQueryState, parseAsInteger, parseAsBoolean, parseAsString } from 'nuqs'

export function NuqsDemo() {
  // String state in URL
  const [name, setName] = useQueryState('name', parseAsString.withDefault(''))
  
  // Integer state in URL
  const [count, setCount] = useQueryState('count', parseAsInteger.withDefault(0))
  
  // Boolean state in URL
  const [isDarkMode, setIsDarkMode] = useQueryState('darkMode', parseAsBoolean.withDefault(false))

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-4xl font-bold mb-4">nuqs State Management Demo</h1>
        <p className="text-lg text-muted-foreground mb-4">
          This page demonstrates URL-based state management using the nuqs library.
          All state changes are reflected in the URL query parameters!
        </p>
        <p className="text-sm text-muted-foreground">
          Try interacting with the controls below and watch the URL update in real-time.
        </p>
      </div>

      {/* Name Input Demo */}
      <div className="p-6 bg-card rounded-lg border space-y-4">
        <div>
          <h2 className="text-2xl font-semibold mb-2">String State Example</h2>
          <p className="text-sm text-muted-foreground mb-4">
            Type your name below. The value is stored in the URL as <code className="px-1 py-0.5 bg-muted rounded">?name=...</code>
          </p>
        </div>
        
        <div className="flex items-center gap-4">
          <label htmlFor="name" className="font-medium">Your Name:</label>
          <input
            id="name"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value || null)}
            placeholder="Enter your name"
            className="flex-1 px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        
        <div className="p-4 bg-muted rounded-md">
          <p className="font-mono text-sm">
            Current name: <span className="font-bold">{name || '(empty)'}</span>
          </p>
        </div>
      </div>

      {/* Counter Demo */}
      <div className="p-6 bg-card rounded-lg border space-y-4">
        <div>
          <h2 className="text-2xl font-semibold mb-2">Integer State Example</h2>
          <p className="text-sm text-muted-foreground mb-4">
            Click the buttons to change the counter. The value is stored in the URL as <code className="px-1 py-0.5 bg-muted rounded">?count=...</code>
          </p>
        </div>
        
        <div className="flex items-center gap-4">
          <button
            onClick={() => setCount((c) => c - 1)}
            className="px-4 py-2 bg-red-500 hover:bg-red-600 text-white rounded-md font-medium"
          >
            Decrement
          </button>
          
          <div className="flex-1 text-center">
            <span className="text-4xl font-bold">{count}</span>
          </div>
          
          <button
            onClick={() => setCount((c) => c + 1)}
            className="px-4 py-2 bg-green-500 hover:bg-green-600 text-white rounded-md font-medium"
          >
            Increment
          </button>
          
          <button
            onClick={() => setCount(0)}
            className="px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded-md font-medium"
          >
            Reset
          </button>
        </div>
      </div>

      {/* Boolean Toggle Demo */}
      <div className="p-6 bg-card rounded-lg border space-y-4">
        <div>
          <h2 className="text-2xl font-semibold mb-2">Boolean State Example</h2>
          <p className="text-sm text-muted-foreground mb-4">
            Toggle the switch below. The value is stored in the URL as <code className="px-1 py-0.5 bg-muted rounded">?darkMode=true</code> or <code className="px-1 py-0.5 bg-muted rounded">?darkMode=false</code>
          </p>
        </div>
        
        <div className="flex items-center gap-4">
          <label htmlFor="darkMode" className="font-medium">Dark Mode:</label>
          <button
            id="darkMode"
            onClick={() => setIsDarkMode(!isDarkMode)}
            className={`relative inline-flex h-8 w-14 items-center rounded-full transition-colors ${
              isDarkMode ? 'bg-blue-600' : 'bg-gray-300'
            }`}
          >
            <span
              className={`inline-block h-6 w-6 transform rounded-full bg-white transition-transform ${
                isDarkMode ? 'translate-x-7' : 'translate-x-1'
              }`}
            />
          </button>
          <span className="font-mono text-sm">
            {isDarkMode ? 'Enabled' : 'Disabled'}
          </span>
        </div>
        
        <div className={`p-4 rounded-md ${isDarkMode ? 'bg-gray-800 text-white' : 'bg-gray-100 text-black'}`}>
          <p className="text-sm">
            This box changes color based on the dark mode state!
          </p>
        </div>
      </div>

      {/* Info Box */}
      <div className="p-6 bg-blue-50 dark:bg-blue-950 rounded-lg border border-blue-200 dark:border-blue-800">
        <h3 className="text-xl font-semibold mb-3">Why nuqs?</h3>
        <ul className="space-y-2 text-sm">
          <li>✅ <strong>Type-safe:</strong> Built-in parsers for common types (string, number, boolean, Date, etc.)</li>
          <li>✅ <strong>Simple API:</strong> Works just like React's useState</li>
          <li>✅ <strong>URL as source of truth:</strong> State is synchronized with the browser URL</li>
          <li>✅ <strong>Shareable:</strong> Copy the URL to share the exact application state</li>
          <li>✅ <strong>Browser history:</strong> Use back/forward buttons to navigate state changes</li>
          <li>✅ <strong>Framework support:</strong> Works with Next.js, Remix, React Router, TanStack Router, and more</li>
        </ul>
      </div>
    </div>
  )
}
