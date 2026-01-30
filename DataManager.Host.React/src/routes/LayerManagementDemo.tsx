import { useState } from 'react';
import { useLayer } from '../hooks/useLayer';
import { useLayerStore } from '../stores/useLayerStore';

/**
 * Demo component showcasing the layer management system.
 * Demonstrates multiple overlays (drawers, modals, dialogs) with proper ESC key handling.
 */
export function LayerManagementDemo() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [nestedDrawerOpen, setNestedDrawerOpen] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  
  const layers = useLayerStore((state) => state.layers);
  
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
        <h2 className="text-2xl font-semibold mb-4">Features</h2>
        <ul className="list-disc list-inside space-y-2">
          <li><strong>Stack-based layer management</strong> - Tracks all open overlays in order</li>
          <li><strong>ESC key handling</strong> - Pressing ESC closes only the topmost layer</li>
          <li><strong>Reusable hooks</strong> - Easy integration with <code className="px-1 py-0.5 bg-gray-200 dark:bg-gray-700 rounded">useLayer</code> hook</li>
          <li><strong>Zustand state management</strong> - Lightweight and performant</li>
          <li><strong>Automatic cleanup</strong> - Layers are removed when components unmount</li>
        </ul>
      </div>
      
      <div className="p-6 bg-green-50 dark:bg-green-950 rounded-lg border border-green-200 dark:border-green-800">
        <h2 className="text-2xl font-semibold mb-4">Try It Out!</h2>
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
      
      <div className="p-6 bg-purple-50 dark:bg-purple-950 rounded-lg border border-purple-200 dark:border-purple-800">
        <h2 className="text-2xl font-semibold mb-4">Active Layers</h2>
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
                  <span className="font-mono text-sm">{layer.id}</span>
                  {index === layers.length - 1 && (
                    <span className="ml-2 text-xs">(Press ESC to close this one)</span>
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
