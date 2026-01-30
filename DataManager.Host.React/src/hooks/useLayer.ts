import { useEffect, useState, useRef } from 'react';
import { useLayerStore } from '../stores/useLayerStore';

// Helper function to generate unique IDs
let idCounter = 0;
function generateUniqueId(): string {
  return `layer-${Date.now()}-${++idCounter}`;
}

export interface UseLayerOptions {
  /** 
   * Whether the layer is currently open/visible.
   * When this changes from true to false, the layer is removed from the stack.
   */
  isOpen: boolean;
  /** 
   * Callback to execute when the layer should be closed (e.g., via ESC key).
   * This function should update the state that controls isOpen.
   */
  onClose: () => void;
  /** 
   * Optional unique identifier for the layer.
   * If not provided, a unique ID will be generated.
   */
  id?: string;
  /**
   * Optional metadata for the layer
   */
  metadata?: Record<string, unknown>;
  /**
   * Whether ESC key handling is enabled for this layer.
   * Defaults to true.
   */
  enableEscapeKey?: boolean;
}

/**
 * React hook for registering a layer (modal, drawer, dialog) with the layer management system.
 * 
 * Features:
 * - Automatic registration/unregistration based on `isOpen` state
 * - ESC key handling for closing the topmost layer
 * - Unique ID generation if not provided
 * - Cleanup on unmount
 * 
 * @example
 * ```tsx
 * function MyModal() {
 *   const [isOpen, setIsOpen] = useState(false);
 *   
 *   useLayer({
 *     isOpen,
 *     onClose: () => setIsOpen(false),
 *     id: 'my-modal',
 *   });
 *   
 *   if (!isOpen) return null;
 *   
 *   return <div>Modal content...</div>;
 * }
 * ```
 */
export function useLayer({
  isOpen,
  onClose,
  id,
  metadata,
  enableEscapeKey = true,
}: UseLayerOptions) {
  const pushLayer = useLayerStore((state) => state.pushLayer);
  const removeLayer = useLayerStore((state) => state.removeLayer);
  const getTopLayer = useLayerStore((state) => state.getTopLayer);
  
  // Generate a unique ID if not provided, using useState with lazy initializer
  const [layerId] = useState(() => id || generateUniqueId());
  
  // Store onClose in a ref to avoid re-running effects when it changes
  const onCloseRef = useRef(onClose);
  useEffect(() => {
    onCloseRef.current = onClose;
  }, [onClose]);
  
  // Register/unregister layer based on isOpen state
  useEffect(() => {
    if (isOpen) {
      pushLayer({
        id: layerId,
        onClose: () => onCloseRef.current(),
        metadata,
      });
    } else {
      removeLayer(layerId);
    }
    
    // Cleanup on unmount
    return () => {
      removeLayer(layerId);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen, layerId, pushLayer, removeLayer]);
  
  // Handle ESC key press
  useEffect(() => {
    if (!enableEscapeKey || !isOpen) return;
    
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        const topLayer = getTopLayer();
        // Only close if this layer is the topmost one
        if (topLayer && topLayer.id === layerId) {
          event.preventDefault();
          event.stopPropagation();
          onCloseRef.current();
        }
      }
    };
    
    // Add listener with capture to ensure it runs before other handlers
    window.addEventListener('keydown', handleKeyDown, true);
    
    return () => {
      window.removeEventListener('keydown', handleKeyDown, true);
    };
  }, [enableEscapeKey, isOpen, layerId, getTopLayer]);
  
  return {
    layerId,
  };
}
