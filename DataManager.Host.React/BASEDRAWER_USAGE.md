# Reusable Drawer Component (BaseDrawer)

## Overview

The `BaseDrawer` component is a flexible, reusable drawer (side panel) component that eliminates the need to recreate the same structure every time you need a drawer with a form or custom content.

## Problem Solved

Previously, when creating drawers with forms, developers had to:
- Recreate the entire drawer structure (backdrop, header, content, footer)
- Manually implement action buttons styling
- Handle layer management integration each time
- Duplicate code for common patterns

The `BaseDrawer` component solves this by providing a ready-to-use drawer with:
- Pre-built header with title and close button
- Scrollable content area for custom content
- Optional footer with configurable action buttons
- Full integration with layer management system
- Support for loading states and multiple button variants

## Basic Usage

```tsx
import { useState } from 'react';
import { BaseDrawer } from '@/components/BaseDrawer';

function MyComponent() {
  const [isOpen, setIsOpen] = useState(false);
  
  return (
    <>
      <button onClick={() => setIsOpen(true)}>Open Drawer</button>
      
      <BaseDrawer
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        title="Create New User"
        actions={[
          { label: 'Cancel', onClick: () => setIsOpen(false), variant: 'secondary' },
          { label: 'Save', onClick: handleSave, variant: 'primary' },
        ]}
      >
        <form>
          <input type="text" placeholder="Name" />
          <input type="email" placeholder="Email" />
        </form>
      </BaseDrawer>
    </>
  );
}
```

## Props

### BaseDrawerProps

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `isOpen` | `boolean` | - | Whether the drawer is open (required) |
| `onClose` | `() => void` | - | Callback when drawer should close (required) |
| `title` | `string` | - | Drawer title displayed in header (required) |
| `children` | `ReactNode` | - | Drawer content (required) |
| `actions` | `DrawerAction[]` | `undefined` | Footer action buttons |
| `footer` | `ReactNode` | `undefined` | Custom footer content (overrides actions) |
| `size` | `'sm' \| 'md' \| 'lg' \| 'xl' \| 'full'` | `'md'` | Drawer width |
| `id` | `string` | auto-generated | Unique ID for layer management |
| `showCloseButton` | `boolean` | `true` | Show close button in header |
| `dismissible` | `boolean` | `true` | Can be dismissed with ESC or backdrop click |
| `trapFocus` | `boolean` | `true` | Trap keyboard focus within drawer |
| `blockScroll` | `boolean` | `true` | Block body scroll when open |

### DrawerAction

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `label` | `string` | - | Button text (required) |
| `onClick` | `() => void` | - | Click handler (required) |
| `variant` | `'primary' \| 'secondary' \| 'danger'` | `'secondary'` | Button style |
| `disabled` | `boolean` | `false` | Whether button is disabled |
| `loading` | `boolean` | `false` | Show loading spinner |

## Size Options

The drawer supports 5 size presets:

- `sm` - 320px (20rem)
- `md` - 384px (24rem) - Default
- `lg` - 512px (32rem)
- `xl` - 768px (48rem)
- `full` - Full width with max-width constraint

## Examples

### Simple Form Drawer

```tsx
function CreateUserDrawer({ isOpen, onClose }) {
  const [formData, setFormData] = useState({ name: '', email: '' });
  
  const handleSave = () => {
    // Save logic
    console.log('Saving:', formData);
    onClose();
  };
  
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Create New User"
      actions={[
        { label: 'Cancel', onClick: onClose, variant: 'secondary' },
        { label: 'Save', onClick: handleSave, variant: 'primary' },
      ]}
    >
      <div className="space-y-4">
        <input
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="Name"
        />
        <input
          type="email"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          placeholder="Email"
        />
      </div>
    </BaseDrawer>
  );
}
```

### Large Drawer with Multiple Actions

```tsx
function EditUserDrawer({ isOpen, onClose }) {
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Edit User Profile"
      size="lg"
      actions={[
        { label: 'Cancel', onClick: onClose, variant: 'secondary' },
        { label: 'Delete', onClick: handleDelete, variant: 'danger' },
        { label: 'Update', onClick: handleUpdate, variant: 'primary' },
      ]}
    >
      {/* Form content */}
    </BaseDrawer>
  );
}
```

### Drawer with Loading State

```tsx
function SubmitDataDrawer({ isOpen, onClose }) {
  const [isLoading, setIsLoading] = useState(false);
  
  const handleSubmit = async () => {
    setIsLoading(true);
    try {
      await submitData();
      onClose();
    } finally {
      setIsLoading(false);
    }
  };
  
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Submit Data"
      dismissible={!isLoading}  // Prevent closing during load
      actions={[
        { 
          label: 'Cancel', 
          onClick: onClose, 
          variant: 'secondary',
          disabled: isLoading 
        },
        { 
          label: 'Submit', 
          onClick: handleSubmit, 
          variant: 'primary',
          loading: isLoading 
        },
      ]}
    >
      <textarea disabled={isLoading} />
    </BaseDrawer>
  );
}
```

### Drawer with Custom Footer

```tsx
<BaseDrawer
  isOpen={isOpen}
  onClose={onClose}
  title="Custom Footer"
  footer={
    <div className="flex justify-between">
      <button onClick={handleSecondaryAction}>Secondary</button>
      <div className="flex gap-2">
        <button onClick={onClose}>Cancel</button>
        <button onClick={handleSave}>Save</button>
      </div>
    </div>
  }
>
  {/* Content */}
</BaseDrawer>
```

### Non-Dismissible Drawer

```tsx
<BaseDrawer
  isOpen={isOpen}
  onClose={onClose}
  title="Important Action"
  dismissible={false}  // Can't close with ESC or backdrop click
  showCloseButton={false}  // Hide X button
  actions={[
    { label: 'Proceed', onClick: handleProceed, variant: 'primary' },
  ]}
>
  <p>You must complete this action before continuing.</p>
</BaseDrawer>
```

## Features

### Automatic Layer Management Integration

The drawer automatically integrates with the layer management system:
- Tracks position in layer stack
- Only topmost drawer responds to ESC key
- Proper focus management and restoration
- Scroll blocking with scrollbar compensation

### Focus Trap

When `trapFocus={true}` (default), keyboard navigation stays within the drawer:
- Tab cycles through focusable elements
- First element receives focus on open
- Focus returns to trigger element on close

### Loading State

Action buttons support loading state:
- Shows spinner icon
- Disables the button
- Optionally make drawer non-dismissible during loading

### Button Variants

Three built-in button styles:
- `primary` - Blue background (main action)
- `secondary` - Gray background (cancel/alternative)
- `danger` - Red background (destructive actions)

## Best Practices

### 1. Use Meaningful IDs

```tsx
<BaseDrawer
  id="edit-user-123"  // Useful for debugging
  // ...
/>
```

### 2. Manage State Properly

```tsx
// ✅ Good - Clear state on close
const handleClose = () => {
  setFormData(initialState);
  setIsOpen(false);
};

// ❌ Bad - State persists between opens
const handleClose = () => setIsOpen(false);
```

### 3. Handle Loading States

```tsx
// ✅ Good - Disable interactions during load
<BaseDrawer
  dismissible={!isLoading}
  actions={[
    { label: 'Cancel', onClick: onClose, disabled: isLoading },
    { label: 'Save', onClick: handleSave, loading: isLoading },
  ]}
>
  <form disabled={isLoading}>...</form>
</BaseDrawer>
```

### 4. Provide Clear Actions

```tsx
// ✅ Good - Clear action labels
actions={[
  { label: 'Cancel', onClick: onClose, variant: 'secondary' },
  { label: 'Save Changes', onClick: handleSave, variant: 'primary' },
]}

// ❌ Bad - Ambiguous labels
actions={[
  { label: 'No', onClick: onClose },
  { label: 'Yes', onClick: handleSave },
]}
```

## Styling

The drawer uses Tailwind CSS classes and automatically adapts to dark mode. Content area is scrollable if it exceeds the viewport height.

### Size Classes

```typescript
const sizeClasses = {
  sm: 'w-80',      // 320px
  md: 'w-96',      // 384px
  lg: 'w-[32rem]', // 512px
  xl: 'w-[48rem]', // 768px
  full: 'w-full max-w-7xl',
};
```

### Button Variants

```typescript
const variantClasses = {
  primary: 'bg-blue-600 hover:bg-blue-700 text-white',
  secondary: 'bg-gray-200 hover:bg-gray-300 dark:bg-gray-700',
  danger: 'bg-red-600 hover:bg-red-700 text-white',
};
```

## Integration with Layer Management

The BaseDrawer automatically uses the layer management system:

```tsx
// This is handled internally:
useLayer({
  isOpen,
  onClose,
  id: id || `drawer-${title.toLowerCase().replace(/\s+/g, '-')}`,
  dismissible,
  trapFocus,
  blockScroll,
});
```

No manual layer registration needed!

## Troubleshooting

### Drawer doesn't close with ESC

- Check if `dismissible={true}` (default)
- Verify drawer is the topmost layer
- Ensure another layer isn't blocking ESC

### Focus not trapped

- Verify `trapFocus={true}` (default)
- Check if content has focusable elements
- Ensure elements aren't disabled

### Buttons not working

- Check `onClick` handlers are defined
- Verify `disabled` prop isn't set
- Check for JavaScript errors in console

## Related Documentation

- [Layer Management System](./LAYER_MANAGEMENT.md)
- [Enhanced Layer Features](./ENHANCED_LAYER_FEATURES.md)
- Demo: Visit `/layer-demo` to see interactive examples
