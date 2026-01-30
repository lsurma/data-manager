# Enhanced Layer Management Features

This document describes the additional features added to the layer management system beyond the basic functionality.

## New Features Overview

The enhanced layer management system now includes:

1. **Non-dismissible layers** - Critical dialogs that require explicit user action
2. **Focus trap** - Keeps keyboard navigation within the active layer
3. **Click outside detection** - Utility hook for handling backdrop clicks
4. **Scroll blocking** - Prevents body scrolling with smart scrollbar compensation
5. **Focus restoration** - Automatically restores focus to the previous element
6. **Event lifecycle callbacks** - Hooks for open, before close, and after close events
7. **Layer property tracking** - Fine-grained control over layer behavior
8. **Enhanced store methods** - Additional methods for layer manipulation

## Enhanced Layer Interface

```typescript
interface Layer {
  id: string;
  onClose?: () => void;
  metadata?: Record<string, unknown>;
  
  // NEW PROPERTIES
  dismissible?: boolean;                    // Can be dismissed via ESC (default: true)
  blockScroll?: boolean;                    // Block body scroll (default: true)
  trapFocus?: boolean;                      // Trap focus within layer (default: true)
  animationState?: LayerAnimationState;     // Track animation state
  previousActiveElement?: Element | null;   // For focus restoration
  onOpen?: () => void;                      // Called when layer opens
  onBeforeClose?: () => boolean | Promise<boolean>; // Can prevent closing
  onAfterClose?: () => void;                // Called after layer closes
}
```

## 1. Non-Dismissible Layers

Non-dismissible layers cannot be closed by pressing ESC or clicking outside. This is useful for critical actions that require explicit user confirmation.

### Usage

```typescript
useLayer({
  isOpen,
  onClose,
  id: 'critical-action',
  dismissible: false,  // Prevents ESC and outside click dismissal
});
```

### Example: Confirmation Dialog

```tsx
function DeleteConfirmation({ isOpen, onClose, onConfirm }) {
  useLayer({
    isOpen,
    onClose,
    id: 'delete-confirmation',
    dismissible: false,  // Must click a button
    onBeforeClose: async () => {
      // Validate before closing
      return await confirmAction();
    },
  });
  
  if (!isOpen) return null;
  
  return (
    <div className="modal">
      <h2>⚠️ Delete Item?</h2>
      <p>This action cannot be undone.</p>
      <button onClick={onClose}>Cancel</button>
      <button onClick={() => { onConfirm(); onClose(); }}>Delete</button>
    </div>
  );
}
```

## 2. Focus Trap

The focus trap feature keeps keyboard navigation (Tab key) within the active layer, improving accessibility and UX.

### useFocusTrap Hook

```typescript
import { useFocusTrap } from '@/hooks/useFocusTrap';

function MyModal({ isOpen, onClose }) {
  const containerRef = useFocusTrap(isOpen);
  
  return (
    <div ref={containerRef}>
      <input type="text" />
      <button>Action</button>
      <button onClick={onClose}>Close</button>
    </div>
  );
}
```

### Features

- Automatically focuses the first focusable element
- Tab cycles through focusable elements within the container
- Shift+Tab navigates backward
- Handles dynamically added/removed elements

### Focusable Elements

The hook recognizes these elements:
- Links with href
- Buttons (not disabled)
- Inputs, textareas, selects (not disabled)
- Elements with tabindex (except -1)

## 3. Click Outside Detection

The `useClickOutside` hook detects when a user clicks outside a referenced element.

### Usage

```typescript
import { useClickOutside } from '@/hooks/useClickOutside';

function Dropdown({ isOpen, onClose }) {
  const ref = useClickOutside<HTMLDivElement>(() => {
    if (isOpen) onClose();
  }, isOpen);
  
  if (!isOpen) return null;
  
  return (
    <div ref={ref} className="dropdown">
      <ul>
        <li>Option 1</li>
        <li>Option 2</li>
      </ul>
    </div>
  );
}
```

### Features

- Type-safe with generics
- Includes small delay to prevent immediate closure
- Active state control to enable/disable detection

## 4. Scroll Blocking

Automatically blocks body scrolling when layers are open, with intelligent scrollbar width compensation to prevent layout shift.

### Automatic Behavior

```typescript
useLayer({
  isOpen,
  onClose,
  id: 'my-modal',
  blockScroll: true,  // Default: true
});
```

### How It Works

1. Measures scrollbar width before blocking
2. Sets `overflow: hidden` on body
3. Adds padding-right to compensate for scrollbar width
4. Restores original styles when layer closes

### Implementation Details

```typescript
const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
document.body.style.overflow = 'hidden';
if (scrollbarWidth > 0) {
  document.body.style.paddingRight = `${scrollbarWidth}px`;
}
```

## 5. Focus Restoration

The system automatically remembers which element had focus before a layer opened and restores focus to that element when the layer closes.

### Automatic Behavior

```typescript
useLayer({
  isOpen,
  onClose,
  id: 'my-modal',
});

// When modal opens, current active element is saved
// When modal closes, focus is restored to that element
```

### Use Case

This is especially useful for:
- Keyboard navigation users
- Screen reader users
- Maintaining context after closing dialogs

## 6. Event Lifecycle Callbacks

Three callback hooks provide fine-grained control over layer lifecycle:

### onOpen

Called immediately when the layer is added to the stack.

```typescript
useLayer({
  isOpen,
  onClose,
  onOpen: () => {
    console.log('Layer opened');
    trackAnalyticsEvent('modal_opened');
  },
});
```

### onBeforeClose

Called before closing. Return `false` to prevent closing.

```typescript
useLayer({
  isOpen,
  onClose,
  onBeforeClose: () => {
    if (hasUnsavedChanges) {
      return confirm('You have unsaved changes. Close anyway?');
    }
    return true;
  },
});
```

Can also be async:

```typescript
onBeforeClose: async () => {
  const result = await showConfirmDialog();
  return result.confirmed;
}
```

### onAfterClose

Called after the layer is removed from the stack.

```typescript
useLayer({
  isOpen,
  onClose,
  onAfterClose: () => {
    console.log('Layer closed and cleaned up');
    resetFormData();
  },
});
```

### Event Order

1. User triggers close (ESC key, button click, etc.)
2. `onBeforeClose()` - Can prevent closing
3. `onClose()` - Main callback that updates component state
4. Layer is removed from stack
5. Focus is restored (if applicable)
6. `onAfterClose()` - Cleanup and side effects

## 7. Layer Property Tracking

Each layer now tracks additional properties that affect its behavior:

```typescript
interface Layer {
  dismissible?: boolean;     // Can be dismissed with ESC
  blockScroll?: boolean;     // Blocks body scroll
  trapFocus?: boolean;       // Traps keyboard focus
  animationState?: 'entering' | 'entered' | 'exiting' | 'exited';
}
```

### Visual Feedback

The demo component shows these properties as badges:

- **Non-dismissible** - Red badge
- **Scroll blocked** - Blue badge
- **Focus trapped** - Green badge

## 8. Enhanced Store Methods

New methods added to `useLayerStore`:

### getLayer(id)

Get a specific layer by ID.

```typescript
const layer = useLayerStore.getState().getLayer('my-modal');
console.log(layer?.metadata);
```

### updateLayer(id, updates)

Update layer properties dynamically.

```typescript
const { updateLayer } = useLayerStore();
updateLayer('my-modal', {
  dismissible: false,
  metadata: { step: 2 },
});
```

### getLayerCount()

Get the total number of open layers.

```typescript
const layerCount = useLayerStore((state) => state.getLayerCount());
console.log(`${layerCount} layers open`);
```

### closeAllDismissible()

Close all layers that are dismissible.

```typescript
const { closeAllDismissible } = useLayerStore();
closeAllDismissible(); // Closes all except non-dismissible layers
```

## Complete Example: Form Modal with All Features

```tsx
import { useState } from 'react';
import { useLayer } from '@/hooks/useLayer';
import { useFocusTrap } from '@/hooks/useFocusTrap';
import { useClickOutside } from '@/hooks/useClickOutside';

function UserFormModal({ isOpen, onClose, onSave }) {
  const [hasChanges, setHasChanges] = useState(false);
  const focusTrapRef = useFocusTrap(isOpen);
  const clickOutsideRef = useClickOutside(() => {
    if (isOpen && !hasChanges) onClose();
  }, isOpen);
  
  useLayer({
    isOpen,
    onClose,
    id: 'user-form',
    dismissible: true,
    blockScroll: true,
    trapFocus: true,
    onOpen: () => {
      console.log('Form opened');
    },
    onBeforeClose: () => {
      if (hasChanges) {
        return confirm('Discard unsaved changes?');
      }
      return true;
    },
    onAfterClose: () => {
      setHasChanges(false);
      console.log('Form closed and reset');
    },
  });
  
  if (!isOpen) return null;
  
  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center">
      <div 
        ref={(node) => {
          if (node) {
            focusTrapRef.current = node;
            clickOutsideRef.current = node;
          }
        }}
        className="bg-white rounded-lg p-6 max-w-md"
      >
        <h2>User Form</h2>
        <input
          type="text"
          onChange={() => setHasChanges(true)}
          placeholder="Name"
        />
        <div className="flex gap-2">
          <button onClick={onClose}>Cancel</button>
          <button onClick={() => { onSave(); onClose(); }}>
            Save
          </button>
        </div>
      </div>
    </div>
  );
}
```

## Best Practices

### 1. Non-Dismissible Layers

Use sparingly, only for critical actions:
- Deleting important data
- Confirming destructive actions
- Multi-step processes that shouldn't be interrupted

### 2. Focus Trap

Always use for accessibility:
- Modals and dialogs
- Any overlay that takes focus

### 3. Scroll Blocking

Enabled by default, but can disable for:
- Dropdowns and tooltips
- Small overlays that don't need scroll blocking

### 4. Event Callbacks

Use for:
- Analytics tracking (`onOpen`)
- Validation (`onBeforeClose`)
- Cleanup (`onAfterClose`)
- Side effects

### 5. Layer Priorities

If you need z-index management, consider:
```typescript
metadata: {
  priority: 'high',
  zIndex: 100,
}
```

## Troubleshooting

### Focus Trap Not Working

- Ensure container has focusable elements
- Check that elements aren't disabled
- Verify `isActive` prop is true

### Scroll Blocking Issues

- Check for multiple layers with scroll blocking
- Verify body element isn't overridden by other styles
- Test scrollbar width calculation on different browsers

### Event Callbacks Not Firing

- Ensure callbacks are stable (use `useCallback`)
- Check that layer is properly registered
- Verify `isOpen` state changes correctly

## Related Documentation

- [Main Documentation](./LAYER_MANAGEMENT.md) - Basic layer management
- [Polish Documentation](./IMPLEMENTACJA_WARSTW.md) - Polish version
- Demo: Visit `/layer-demo` route for interactive examples
