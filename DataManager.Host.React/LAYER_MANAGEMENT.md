# Layer Management System

This document describes the reusable layer management system implemented in the DataManager.Host.React application.

## Overview

The layer management system provides a robust solution for handling multiple overlays (modals, drawers, dialogs, panels) with proper ESC key behavior. When multiple layers are open, pressing ESC closes only the topmost layer, not all layers at once.

## Features

- **Stack-based layer management** - Tracks all open overlays in order (LIFO - Last In, First Out)
- **ESC key handling** - Pressing ESC closes only the topmost layer
- **Reusable hooks** - Easy integration with the `useLayer` hook
- **Zustand state management** - Lightweight and performant state management
- **Automatic cleanup** - Layers are automatically removed when components unmount
- **Type-safe** - Full TypeScript support

## Architecture

The system consists of two main parts:

### 1. Zustand Store (`useLayerStore`)

Located in `src/stores/useLayerStore.ts`, this store manages the layer stack:

```typescript
interface Layer {
  id: string;                              // Unique identifier
  onClose?: () => void;                    // Callback when layer should close
  metadata?: Record<string, unknown>;      // Optional metadata
}
```

**Store Methods:**
- `pushLayer(layer)` - Add a new layer to the stack
- `removeLayer(id)` - Remove a layer by ID
- `closeTopLayer()` - Close the topmost layer
- `getTopLayer()` - Get the topmost layer
- `hasLayer(id)` - Check if a layer exists
- `clearLayers()` - Remove all layers

### 2. React Hook (`useLayer`)

Located in `src/hooks/useLayer.ts`, this hook provides easy integration:

```typescript
interface UseLayerOptions {
  isOpen: boolean;                 // Whether the layer is visible
  onClose: () => void;             // Callback to close the layer
  id?: string;                     // Optional unique ID
  metadata?: Record<string, unknown>; // Optional metadata
  enableEscapeKey?: boolean;       // Enable ESC key (default: true)
}
```

## Usage

### Basic Example

```tsx
import { useState } from 'react';
import { useLayer } from '@/hooks/useLayer';

function MyModal() {
  const [isOpen, setIsOpen] = useState(false);
  
  // Register the layer
  useLayer({
    isOpen,
    onClose: () => setIsOpen(false),
    id: 'my-modal',
  });
  
  if (!isOpen) return null;
  
  return (
    <div className="modal">
      <h2>My Modal</h2>
      <button onClick={() => setIsOpen(false)}>Close</button>
    </div>
  );
}
```

### Drawer Example

```tsx
function MyDrawer({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) {
  useLayer({
    isOpen,
    onClose,
    id: 'my-drawer',
  });
  
  if (!isOpen) return null;
  
  return (
    <>
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black/50 z-40" onClick={onClose} />
      
      {/* Drawer */}
      <div className="fixed inset-y-0 right-0 w-96 bg-white z-50">
        <button onClick={onClose}>Close</button>
        <p>Drawer content...</p>
      </div>
    </>
  );
}
```

### Accessing the Layer Stack

You can access the layer stack directly from the store:

```tsx
import { useLayerStore } from '@/stores/useLayerStore';

function LayerDebugger() {
  const layers = useLayerStore((state) => state.layers);
  
  return (
    <div>
      <h3>Active Layers ({layers.length})</h3>
      <ul>
        {layers.map((layer) => (
          <li key={layer.id}>{layer.id}</li>
        ))}
      </ul>
    </div>
  );
}
```

## How ESC Key Handling Works

1. When a layer is registered with `useLayer`, it automatically sets up an ESC key listener
2. When ESC is pressed, the listener checks if the current layer is the topmost one
3. If it is the topmost layer, the `onClose` callback is executed
4. If it's not the topmost layer, the ESC key press is ignored
5. The listener uses capture phase (`true` parameter) to ensure it runs before other handlers

## Best Practices

### 1. Always Provide an ID for Important Layers

While the hook can auto-generate IDs, it's better to provide meaningful IDs for debugging:

```tsx
useLayer({
  isOpen,
  onClose,
  id: 'user-settings-drawer', // ✅ Good
  // id: undefined,            // ❌ Auto-generated, harder to debug
});
```

### 2. Keep onClose Simple

The `onClose` callback should just update state. Don't perform complex operations:

```tsx
// ✅ Good
useLayer({
  isOpen,
  onClose: () => setIsOpen(false),
});

// ❌ Bad - complex logic in onClose
useLayer({
  isOpen,
  onClose: () => {
    saveData();
    validateForm();
    setIsOpen(false);
  },
});
```

### 3. Use Metadata for Context

Store additional context in the metadata field:

```tsx
useLayer({
  isOpen,
  onClose,
  id: 'edit-form',
  metadata: {
    formType: 'user-profile',
    hasUnsavedChanges: true,
  },
});
```

### 4. Disable ESC Key When Needed

For critical confirmations, you might want to disable ESC:

```tsx
useLayer({
  isOpen,
  onClose,
  id: 'delete-confirmation',
  enableEscapeKey: false, // User must click a button
});
```

## Demo

A comprehensive demo is available at `/layer-demo` route. The demo showcases:

- Opening multiple layers (drawer → nested drawer → modal → dialog)
- ESC key behavior (closes topmost layer only)
- Visual indication of the layer stack
- Different layer types (side panel, centered modal, confirmation dialog)

## Implementation Notes

### Why useState for ID Generation?

The hook uses `useState` with a lazy initializer for ID generation to satisfy React 19's strict rules about pure components. This ensures the ID is generated only once, even across re-renders.

### Why useRef for onClose?

The `onClose` callback is stored in a `useRef` to prevent the effect from re-running every time `onClose` changes (which can happen on every render). This prevents infinite loops while keeping the callback up-to-date.

### Z-Index Management

The layer management system doesn't manage z-index values. It's recommended to use a consistent z-index scale:

- Backdrop: `z-40`
- First layer: `z-50`
- Second layer: `z-60`
- And so on...

Alternatively, use CSS to automatically stack layers:

```css
.layer {
  position: fixed;
  isolation: isolate;
}
```

## Troubleshooting

### ESC Key Not Working

1. Check if `enableEscapeKey` is set to `true` (default)
2. Verify the layer is the topmost one in the stack
3. Check browser console for errors
4. Ensure `isOpen` is `true`

### Layer Not Closing

1. Verify `onClose` callback is updating the state correctly
2. Check if the layer ID is unique
3. Ensure the component is properly unmounting

### Multiple Layers Close at Once

This shouldn't happen with this system. If it does:

1. Check if multiple layers share the same ID
2. Verify you're using the `useLayer` hook correctly
3. Check for global ESC key handlers that might interfere

## Future Enhancements

Potential improvements for the future:

- **Focus trap** - Keep focus within the topmost layer
- **Scroll lock** - Prevent background scrolling when layer is open
- **Animation support** - Built-in animations for open/close
- **A11y improvements** - ARIA attributes, screen reader support
- **History integration** - Browser back button closes layers
- **Persist to URL** - Store layer state in URL query parameters

## Related Files

- `src/stores/useLayerStore.ts` - Zustand store for layer state
- `src/hooks/useLayer.ts` - React hook for layer registration
- `src/routes/LayerManagementDemo.tsx` - Comprehensive demo component
- `LAYER_MANAGEMENT.md` - This documentation file
