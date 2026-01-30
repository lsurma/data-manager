import { useState } from 'react';
import { useLayer } from '../hooks/useLayer';
import { useLayerStore } from '../stores/useLayerStore';
import { BaseDrawer } from '../components/BaseDrawer';

/**
 * Demo component showcasing the layer management system.
 * Demonstrates multiple overlays (drawers, modals, dialogs) with proper ESC key handling.
 */
export function LayerManagementDemo() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [nestedDrawerOpen, setNestedDrawerOpen] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [nonDismissibleModalOpen, setNonDismissibleModalOpen] = useState(false);
  const [focusTrapModalOpen, setFocusTrapModalOpen] = useState(false);
  const [formDrawerOpen, setFormDrawerOpen] = useState(false);
  const [editDrawerOpen, setEditDrawerOpen] = useState(false);
  const [loadingDrawerOpen, setLoadingDrawerOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  
  const layers = useLayerStore((state) => state.layers);
  const layerCount = useLayerStore((state) => state.getLayerCount());
  
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-4xl font-bold mb-4">Layer Management System Demo</h1>
        <p className="text-lg mb-4 text-gray-600 dark:text-gray-400">
          This demo showcases a reusable layer management system using Zustand.
          Try opening multiple overlays and pressing ESC to close them one by one.
        </p>
      </div>
      
      <div className="p-6 bg-blue-50 dark:bg-blue-950 rounded-lg border border-blue-200 dark:border-blue-800">
        <h2 className="text-2xl font-semibold mb-4">Enhanced Features</h2>
        <ul className="list-disc list-inside space-y-2">
          <li><strong>Stack-based layer management</strong> - Tracks all open overlays in order</li>
          <li><strong>ESC key handling</strong> - Pressing ESC closes only the topmost layer</li>
          <li><strong>Reusable hooks</strong> - Easy integration with <code className="px-1 py-0.5 bg-gray-200 dark:bg-gray-700 rounded">useLayer</code> hook</li>
          <li><strong>Zustand state management</strong> - Lightweight and performant</li>
          <li><strong>Automatic cleanup</strong> - Layers are removed when components unmount</li>
          <li><strong>Scroll blocking</strong> - Body scroll locked when layers are open</li>
          <li><strong>Focus management</strong> - Restores focus to previous element on close</li>
          <li><strong>Non-dismissible layers</strong> - Critical dialogs that require explicit action</li>
          <li><strong>Event callbacks</strong> - onOpen, onBeforeClose, onAfterClose hooks</li>
          <li><strong>Focus trapping</strong> - Keeps keyboard navigation within the layer</li>
        </ul>
      </div>
      
      <div className="p-6 bg-green-50 dark:bg-green-950 rounded-lg border border-green-200 dark:border-green-800">
        <h2 className="text-2xl font-semibold mb-4">Basic Examples</h2>
        <div className="space-y-3">
          <button
            onClick={() => setDrawerOpen(true)}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md font-medium"
          >
            Open Drawer
          </button>
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Click to open a drawer. From there you can open a nested drawer, modal, or confirmation dialog.
          </p>
        </div>
      </div>
      
      <div className="p-6 bg-orange-50 dark:bg-orange-950 rounded-lg border border-orange-200 dark:border-orange-800">
        <h2 className="text-2xl font-semibold mb-4">Advanced Features Demo</h2>
        <div className="space-y-3">
          <div>
            <button
              onClick={() => setNonDismissibleModalOpen(true)}
              className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md font-medium"
            >
              Open Non-Dismissible Modal
            </button>
            <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
              This modal cannot be closed with ESC or backdrop click - you must click a button.
            </p>
          </div>
          
          <div>
            <button
              onClick={() => setFocusTrapModalOpen(true)}
              className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-md font-medium"
            >
              Open Focus Trap Demo
            </button>
            <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
              Try using Tab to navigate - focus stays trapped within the modal.
            </p>
          </div>
        </div>
      </div>
      
      <div className="p-6 bg-teal-50 dark:bg-teal-950 rounded-lg border border-teal-200 dark:border-teal-800">
        <h2 className="text-2xl font-semibold mb-4">🎯 Reusable Drawer Components</h2>
        <p className="mb-4 text-gray-600 dark:text-gray-400">
          Use the <code className="px-1 py-0.5 bg-gray-200 dark:bg-gray-700 rounded">BaseDrawer</code> component 
          to quickly create drawers with forms or custom content. No need to recreate the structure every time!
        </p>
        <div className="space-y-3">
          <div>
            <button
              onClick={() => setFormDrawerOpen(true)}
              className="px-4 py-2 bg-teal-600 hover:bg-teal-700 text-white rounded-md font-medium"
            >
              Open Simple Form Drawer
            </button>
            <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
              Example with form inputs and action buttons (Cancel/Save).
            </p>
          </div>
          
          <div>
            <button
              onClick={() => setEditDrawerOpen(true)}
              className="px-4 py-2 bg-teal-600 hover:bg-teal-700 text-white rounded-md font-medium"
            >
              Open Large Edit Drawer
            </button>
            <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
              Larger drawer with more form fields and validation example.
            </p>
          </div>
          
          <div>
            <button
              onClick={() => setLoadingDrawerOpen(true)}
              className="px-4 py-2 bg-teal-600 hover:bg-teal-700 text-white rounded-md font-medium"
            >
              Open Drawer with Loading State
            </button>
            <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
              Shows how to handle loading states in action buttons.
            </p>
          </div>
        </div>
      </div>
      
      <div className="p-6 bg-purple-50 dark:bg-purple-950 rounded-lg border border-purple-200 dark:border-purple-800">
        <h2 className="text-2xl font-semibold mb-4">Active Layers ({layerCount})</h2>
        {layers.length === 0 ? (
          <p className="text-gray-600 dark:text-gray-400">No layers currently open</p>
        ) : (
          <div className="space-y-2">
            <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
              Stack order (newest at bottom):
            </p>
            <ul className="space-y-1">
              {layers.map((layer, index) => (
                <li 
                  key={layer.id}
                  className={`p-2 rounded ${
                    index === layers.length - 1 
                      ? 'bg-purple-200 dark:bg-purple-800 font-semibold' 
                      : 'bg-purple-100 dark:bg-purple-900'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <span className="font-mono text-sm">{layer.id}</span>
                    <div className="flex gap-2 text-xs">
                      {layer.dismissible === false && (
                        <span className="px-2 py-1 bg-red-200 dark:bg-red-900 rounded">Non-dismissible</span>
                      )}
                      {layer.blockScroll && (
                        <span className="px-2 py-1 bg-blue-200 dark:bg-blue-900 rounded">Scroll blocked</span>
                      )}
                      {layer.trapFocus && (
                        <span className="px-2 py-1 bg-green-200 dark:bg-green-900 rounded">Focus trapped</span>
                      )}
                    </div>
                  </div>
                  {index === layers.length - 1 && (
                    <span className="ml-2 text-xs">
                      {layer.dismissible === false 
                        ? '(Cannot dismiss with ESC)' 
                        : '(Press ESC to close this one)'}
                    </span>
                  )}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
      
      {/* Drawer Component */}
      <Drawer 
        isOpen={drawerOpen} 
        onClose={() => setDrawerOpen(false)}
        onOpenNested={() => setNestedDrawerOpen(true)}
        onOpenModal={() => setModalOpen(true)}
      />
      
      {/* Nested Drawer Component */}
      <Drawer 
        isOpen={nestedDrawerOpen} 
        onClose={() => setNestedDrawerOpen(false)}
        title="Nested Drawer"
        onOpenModal={() => setModalOpen(true)}
        onOpenDialog={() => setConfirmDialogOpen(true)}
      />
      
      {/* Modal Component */}
      <Modal 
        isOpen={modalOpen} 
        onClose={() => setModalOpen(false)}
        onOpenDialog={() => setConfirmDialogOpen(true)}
      />
      
      {/* Confirmation Dialog Component */}
      <ConfirmDialog 
        isOpen={confirmDialogOpen} 
        onClose={() => setConfirmDialogOpen(false)}
      />
      
      {/* Non-Dismissible Modal Component */}
      <NonDismissibleModal 
        isOpen={nonDismissibleModalOpen} 
        onClose={() => setNonDismissibleModalOpen(false)}
      />
      
      {/* Focus Trap Modal Component */}
      <FocusTrapModal 
        isOpen={focusTrapModalOpen} 
        onClose={() => setFocusTrapModalOpen(false)}
      />
      
      {/* Reusable Drawer Examples */}
      <SimpleFormDrawer 
        isOpen={formDrawerOpen} 
        onClose={() => setFormDrawerOpen(false)}
      />
      
      <EditDrawer 
        isOpen={editDrawerOpen} 
        onClose={() => setEditDrawerOpen(false)}
      />
      
      <LoadingDrawer 
        isOpen={loadingDrawerOpen} 
        onClose={() => setLoadingDrawerOpen(false)}
        isLoading={isLoading}
        setIsLoading={setIsLoading}
      />
    </div>
  );
}

// Drawer component with side panel
interface DrawerProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  onOpenNested?: () => void;
  onOpenModal?: () => void;
  onOpenDialog?: () => void;
}

function Drawer({ isOpen, onClose, title = 'Drawer', onOpenNested, onOpenModal, onOpenDialog }: DrawerProps) {
  useLayer({
    isOpen,
    onClose,
    id: `drawer-${title.toLowerCase().replace(/\s+/g, '-')}`,
  });
  
  if (!isOpen) return null;
  
  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black/50 z-40"
        onClick={onClose}
      />
      
      {/* Drawer */}
      <div className="fixed inset-y-0 right-0 w-96 bg-white dark:bg-gray-900 shadow-xl z-50 overflow-y-auto">
        <div className="p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold">{title}</h2>
            <button
              onClick={onClose}
              className="p-2 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-full"
              aria-label="Close"
            >
              ✕
            </button>
          </div>
          
          <div className="space-y-4">
            <div className="p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
              <p className="text-sm text-gray-600 dark:text-gray-400 mb-3">
                This is a drawer panel. Try pressing ESC to close it.
              </p>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                You can also open more layers from here to test the stacking behavior.
              </p>
            </div>
            
            <div className="space-y-2">
              {onOpenNested && (
                <button
                  onClick={onOpenNested}
                  className="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md font-medium"
                >
                  Open Nested Drawer
                </button>
              )}
              
              {onOpenModal && (
                <button
                  onClick={onOpenModal}
                  className="w-full px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-md font-medium"
                >
                  Open Modal
                </button>
              )}
              
              {onOpenDialog && (
                <button
                  onClick={onOpenDialog}
                  className="w-full px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-md font-medium"
                >
                  Open Confirmation Dialog
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

// Modal component (centered overlay)
interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  onOpenDialog?: () => void;
}

function Modal({ isOpen, onClose, onOpenDialog }: ModalProps) {
  useLayer({
    isOpen,
    onClose,
    id: 'modal-centered',
  });
  
  if (!isOpen) return null;
  
  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black/60 z-50 flex items-center justify-center"
        onClick={onClose}
      >
        {/* Modal */}
        <div 
          className="bg-white dark:bg-gray-900 rounded-lg shadow-2xl p-6 max-w-md w-full mx-4"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-bold">Modal Dialog</h2>
            <button
              onClick={onClose}
              className="p-2 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-full"
              aria-label="Close"
            >
              ✕
            </button>
          </div>
          
          <div className="space-y-4">
            <p className="text-gray-600 dark:text-gray-400">
              This is a centered modal dialog. Press ESC to close the topmost layer.
            </p>
            
            {onOpenDialog && (
              <button
                onClick={onOpenDialog}
                className="w-full px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-md font-medium"
              >
                Open Confirmation Dialog
              </button>
            )}
          </div>
        </div>
      </div>
    </>
  );
}

// Confirmation Dialog component
interface ConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
}

function ConfirmDialog({ isOpen, onClose }: ConfirmDialogProps) {
  useLayer({
    isOpen,
    onClose,
    id: 'confirm-dialog',
  });
  
  if (!isOpen) return null;
  
  return (
    <>
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black/70 z-60 flex items-center justify-center"
        onClick={onClose}
      >
        {/* Dialog */}
        <div 
          className="bg-white dark:bg-gray-900 rounded-lg shadow-2xl p-6 max-w-sm w-full mx-4"
          onClick={(e) => e.stopPropagation()}
        >
          <h2 className="text-xl font-bold mb-4">Confirm Action</h2>
          
          <p className="text-gray-600 dark:text-gray-400 mb-6">
            This is a confirmation dialog. It's the topmost layer, so pressing ESC will close this first.
          </p>
          
          <div className="flex gap-3">
            <button
              onClick={onClose}
              className="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 rounded-md font-medium"
            >
              Cancel
            </button>
            <button
              onClick={onClose}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-md font-medium"
            >
              Confirm
            </button>
          </div>
        </div>
      </div>
    </>
  );
}

// Non-Dismissible Modal component
interface NonDismissibleModalProps {
  isOpen: boolean;
  onClose: () => void;
}

function NonDismissibleModal({ isOpen, onClose }: NonDismissibleModalProps) {
  const [showWarning, setShowWarning] = useState(false);
  
  useLayer({
    isOpen,
    onClose,
    id: 'non-dismissible-modal',
    dismissible: false,
    onOpen: () => console.log('Non-dismissible modal opened'),
    onBeforeClose: () => {
      console.log('Attempting to close non-dismissible modal');
      return true;
    },
    onAfterClose: () => console.log('Non-dismissible modal closed'),
  });
  
  if (!isOpen) return null;
  
  return (
    <>
      {/* Backdrop - click does nothing */}
      <div 
        className="fixed inset-0 bg-black/70 z-50 flex items-center justify-center"
        onClick={() => setShowWarning(true)}
      >
        {/* Modal */}
        <div 
          className="bg-white dark:bg-gray-900 rounded-lg shadow-2xl p-6 max-w-md w-full mx-4 border-4 border-red-500"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex items-center gap-3 mb-4">
            <span className="text-3xl">⚠️</span>
            <h2 className="text-2xl font-bold text-red-600 dark:text-red-400">Critical Action Required</h2>
          </div>
          
          <div className="space-y-4">
            <p className="text-gray-600 dark:text-gray-400">
              This is a non-dismissible modal. You <strong>cannot</strong> close it with ESC or by clicking outside.
              You must explicitly click one of the buttons below.
            </p>
            
            {showWarning && (
              <div className="p-3 bg-yellow-100 dark:bg-yellow-900 border border-yellow-400 rounded-md">
                <p className="text-sm text-yellow-800 dark:text-yellow-200">
                  ⚡ This modal requires explicit action! Please click a button.
                </p>
              </div>
            )}
            
            <div className="flex gap-3">
              <button
                onClick={() => {
                  setShowWarning(false);
                  onClose();
                }}
                className="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 rounded-md font-medium"
              >
                Cancel
              </button>
              <button
                onClick={() => {
                  alert('Action confirmed!');
                  setShowWarning(false);
                  onClose();
                }}
                className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md font-medium"
              >
                Proceed
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

// Focus Trap Modal component
import { useFocusTrap } from '../hooks/useFocusTrap';
import { useClickOutside } from '../hooks/useClickOutside';

interface FocusTrapModalProps {
  isOpen: boolean;
  onClose: () => void;
}

function FocusTrapModal({ isOpen, onClose }: FocusTrapModalProps) {
  const focusTrapRef = useFocusTrap(isOpen);
  const clickOutsideRef = useClickOutside<HTMLDivElement>(() => {
    if (isOpen) onClose();
  }, isOpen);
  
  useLayer({
    isOpen,
    onClose,
    id: 'focus-trap-modal',
    trapFocus: true,
  });
  
  if (!isOpen) return null;
  
  return (
    <>
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black/70 z-50 flex items-center justify-center">
        {/* Modal */}
        <div 
          ref={(node) => {
            // Merge refs
            if (node) {
              (focusTrapRef as React.MutableRefObject<HTMLElement | null>).current = node;
              (clickOutsideRef as React.MutableRefObject<HTMLDivElement | null>).current = node;
            }
          }}
          className="bg-white dark:bg-gray-900 rounded-lg shadow-2xl p-6 max-w-md w-full mx-4 border-2 border-purple-500"
        >
          <div className="flex items-center gap-3 mb-4">
            <span className="text-3xl">⌨️</span>
            <h2 className="text-2xl font-bold text-purple-600 dark:text-purple-400">Focus Trap Demo</h2>
          </div>
          
          <div className="space-y-4">
            <p className="text-gray-600 dark:text-gray-400">
              Try pressing <kbd className="px-2 py-1 bg-gray-200 dark:bg-gray-700 rounded">Tab</kbd> to navigate.
              Focus will cycle through the elements below and stay within this modal.
            </p>
            
            <div className="space-y-2">
              <label className="block">
                <span className="text-sm font-medium">First Name:</span>
                <input 
                  type="text" 
                  placeholder="John"
                  className="w-full mt-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-purple-500 dark:bg-gray-800"
                />
              </label>
              
              <label className="block">
                <span className="text-sm font-medium">Last Name:</span>
                <input 
                  type="text" 
                  placeholder="Doe"
                  className="w-full mt-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-purple-500 dark:bg-gray-800"
                />
              </label>
              
              <label className="block">
                <span className="text-sm font-medium">Email:</span>
                <input 
                  type="email" 
                  placeholder="john@example.com"
                  className="w-full mt-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-purple-500 dark:bg-gray-800"
                />
              </label>
            </div>
            
            <div className="flex gap-3 pt-2">
              <button
                onClick={onClose}
                className="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 rounded-md font-medium"
              >
                Cancel
              </button>
              <button
                onClick={() => {
                  alert('Form submitted!');
                  onClose();
                }}
                className="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-md font-medium"
              >
                Submit
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

// Simple Form Drawer using BaseDrawer
function SimpleFormDrawer({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) {
  const [formData, setFormData] = useState({ name: '', email: '' });
  
  const handleSave = () => {
    console.log('Saving:', formData);
    alert(`Saved: ${formData.name} - ${formData.email}`);
    onClose();
  };
  
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Create New User"
      size="md"
      actions={[
        { label: 'Cancel', onClick: onClose, variant: 'secondary' },
        { label: 'Save', onClick: handleSave, variant: 'primary' },
      ]}
    >
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">Name</label>
          <input
            type="text"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
            placeholder="Enter name"
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-1">Email</label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
            placeholder="Enter email"
          />
        </div>
        
        <div className="p-4 bg-teal-50 dark:bg-teal-900/30 rounded-lg">
          <p className="text-sm text-teal-800 dark:text-teal-200">
            💡 This drawer was created with just a few lines of code using <code>BaseDrawer</code>!
          </p>
        </div>
      </div>
    </BaseDrawer>
  );
}

// Large Edit Drawer with more fields
function EditDrawer({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    role: '',
  });
  
  const handleSave = () => {
    console.log('Saving:', formData);
    alert('User updated successfully!');
    onClose();
  };
  
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Edit User Profile"
      size="lg"
      actions={[
        { label: 'Cancel', onClick: onClose, variant: 'secondary' },
        { label: 'Delete', onClick: () => alert('Delete user'), variant: 'danger' },
        { label: 'Update', onClick: handleSave, variant: 'primary' },
      ]}
    >
      <div className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">First Name</label>
            <input
              type="text"
              value={formData.firstName}
              onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
              placeholder="John"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium mb-1">Last Name</label>
            <input
              type="text"
              value={formData.lastName}
              onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
              placeholder="Doe"
            />
          </div>
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-1">Email</label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
            placeholder="john.doe@example.com"
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-1">Phone</label>
          <input
            type="tel"
            value={formData.phone}
            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
            placeholder="+1 (555) 123-4567"
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-1">Role</label>
          <select
            value={formData.role}
            onChange={(e) => setFormData({ ...formData, role: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
          >
            <option value="">Select role...</option>
            <option value="admin">Administrator</option>
            <option value="editor">Editor</option>
            <option value="viewer">Viewer</option>
          </select>
        </div>
        
        <div className="p-4 bg-blue-50 dark:bg-blue-900/30 rounded-lg">
          <p className="text-sm text-blue-800 dark:text-blue-200">
            ℹ️ This larger drawer has multiple action buttons and a 'lg' size configuration.
          </p>
        </div>
      </div>
    </BaseDrawer>
  );
}

// Loading State Drawer
function LoadingDrawer({ isOpen, onClose, isLoading, setIsLoading }: { 
  isOpen: boolean; 
  onClose: () => void;
  isLoading: boolean;
  setIsLoading: (loading: boolean) => void;
}) {
  const [data, setData] = useState('');
  
  const handleSubmit = () => {
    setIsLoading(true);
    // Simulate API call
    setTimeout(() => {
      setIsLoading(false);
      alert('Data submitted successfully!');
      onClose();
    }, 2000);
  };
  
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Submit Data"
      size="md"
      dismissible={!isLoading}
      actions={[
        { label: 'Cancel', onClick: onClose, variant: 'secondary', disabled: isLoading },
        { label: 'Submit', onClick: handleSubmit, variant: 'primary', loading: isLoading },
      ]}
    >
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">Data</label>
          <textarea
            value={data}
            onChange={(e) => setData(e.target.value)}
            rows={5}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-2 focus:ring-teal-500 dark:bg-gray-800"
            placeholder="Enter some data..."
            disabled={isLoading}
          />
        </div>
        
        <div className="p-4 bg-yellow-50 dark:bg-yellow-900/30 rounded-lg">
          <p className="text-sm text-yellow-800 dark:text-yellow-200">
            🔄 Click Submit to see the loading state in action. The drawer becomes non-dismissible while loading.
          </p>
        </div>
      </div>
    </BaseDrawer>
  );
}
