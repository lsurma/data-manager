import { ReactNode } from 'react';
import { useLayer } from '../hooks/useLayer';
import { useFocusTrap } from '../hooks/useFocusTrap';

export interface DrawerAction {
  /** Label text for the button */
  label: string;
  /** Click handler for the button */
  onClick: () => void;
  /** Button variant/style */
  variant?: 'primary' | 'secondary' | 'danger';
  /** Whether the button is disabled */
  disabled?: boolean;
  /** Whether the button is loading */
  loading?: boolean;
}

export interface BaseDrawerProps {
  /** Whether the drawer is open */
  isOpen: boolean;
  /** Callback when drawer should close */
  onClose: () => void;
  /** Drawer title */
  title: string;
  /** Drawer content */
  children: ReactNode;
  /** Optional footer actions (buttons) */
  actions?: DrawerAction[];
  /** Optional custom footer content (overrides actions) */
  footer?: ReactNode;
  /** Drawer width (default: 'md') */
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full';
  /** Unique ID for the drawer (auto-generated if not provided) */
  id?: string;
  /** Whether to show the close button in header (default: true) */
  showCloseButton?: boolean;
  /** Whether the drawer can be dismissed (default: true) */
  dismissible?: boolean;
  /** Whether to trap focus (default: true) */
  trapFocus?: boolean;
  /** Whether to block scroll (default: true) */
  blockScroll?: boolean;
}

const sizeClasses = {
  sm: 'w-80',
  md: 'w-96',
  lg: 'w-[32rem]',
  xl: 'w-[48rem]',
  full: 'w-full max-w-7xl',
};

const variantClasses = {
  primary: 'bg-blue-600 hover:bg-blue-700 text-white disabled:bg-blue-300',
  secondary: 'bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 disabled:bg-gray-100 dark:disabled:bg-gray-800',
  danger: 'bg-red-600 hover:bg-red-700 text-white disabled:bg-red-300',
};

/**
 * Reusable drawer component with common structure.
 * 
 * Features:
 * - Header with title and close button
 * - Content area for custom content (forms, etc.)
 * - Footer with configurable action buttons
 * - Integration with layer management system
 * - Focus trap and scroll blocking
 * 
 * @example
 * ```tsx
 * <BaseDrawer
 *   isOpen={isOpen}
 *   onClose={() => setIsOpen(false)}
 *   title="Edit User"
 *   actions={[
 *     { label: 'Cancel', onClick: () => setIsOpen(false), variant: 'secondary' },
 *     { label: 'Save', onClick: handleSave, variant: 'primary' },
 *   ]}
 * >
 *   <form>
 *     <input type="text" placeholder="Name" />
 *   </form>
 * </BaseDrawer>
 * ```
 */
export function BaseDrawer({
  isOpen,
  onClose,
  title,
  children,
  actions,
  footer,
  size = 'md',
  id,
  showCloseButton = true,
  dismissible = true,
  trapFocus = true,
  blockScroll = true,
}: BaseDrawerProps) {
  const focusTrapRef = useFocusTrap(isOpen && trapFocus) as React.RefObject<HTMLDivElement>;
  
  useLayer({
    isOpen,
    onClose,
    id: id || `drawer-${title.toLowerCase().replace(/\s+/g, '-')}`,
    dismissible,
    trapFocus,
    blockScroll,
  });
  
  if (!isOpen) return null;
  
  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black/50 z-40"
        onClick={dismissible ? onClose : undefined}
      />
      
      {/* Drawer */}
      <div 
        ref={focusTrapRef}
        className={`fixed inset-y-0 right-0 ${sizeClasses[size]} bg-white dark:bg-gray-900 shadow-xl z-50 flex flex-col`}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700 flex-shrink-0">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100">
            {title}
          </h2>
          {showCloseButton && (
            <button
              onClick={onClose}
              className="p-2 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-full transition-colors"
              aria-label="Close drawer"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          )}
        </div>
        
        {/* Content */}
        <div className="flex-1 overflow-y-auto px-6 py-4">
          {children}
        </div>
        
        {/* Footer */}
        {(footer || (actions && actions.length > 0)) && (
          <div className="px-6 py-4 border-t border-gray-200 dark:border-gray-700 flex-shrink-0">
            {footer || (
              <div className="flex gap-3 justify-end">
                {actions?.map((action, index) => (
                  <button
                    key={index}
                    onClick={action.onClick}
                    disabled={action.disabled || action.loading}
                    className={`px-4 py-2 rounded-md font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${
                      variantClasses[action.variant || 'secondary']
                    }`}
                  >
                    {action.loading ? (
                      <span className="flex items-center gap-2">
                        <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                        </svg>
                        {action.label}
                      </span>
                    ) : (
                      action.label
                    )}
                  </button>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </>
  );
}
