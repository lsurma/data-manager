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
  /**
   * Whether this layer can be dismissed via ESC key or click outside.
   * Defaults to true.
   */
  dismissible?: boolean;
  /**
   * Whether to block scrolling on the body when this layer is open.
   * Defaults to true for better UX.
   */
  blockScroll?: boolean;
  /**
   * Whether to trap focus within this layer.
   * Defaults to true for accessibility.
   */
  trapFocus?: boolean;
  /**
   * Callback executed when layer starts opening.
   */
  onOpen?: () => void;
  /**
   * Callback executed before layer closes.
   * Return false to prevent closing.
   */
  onBeforeClose?: () => boolean | Promise<boolean>;
  /**
   * Callback executed after layer has closed.
   */
  onAfterClose?: () => void;
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
  dismissible = true,
  blockScroll = true,
  trapFocus = true,
  onOpen,
  onBeforeClose,
  onAfterClose,
}: UseLayerOptions) {
  const pushLayer = useLayerStore((state) => state.pushLayer);
  const removeLayer = useLayerStore((state) => state.removeLayer);
  const getTopLayer = useLayerStore((state) => state.getTopLayer);
  
  // Generate a unique ID if not provided, using useState with lazy initializer
  const [layerId] = useState(() => id || generateUniqueId());
  
  // Store callbacks in refs to avoid re-running effects when they change
  const onCloseRef = useRef(onClose);
  const onOpenRef = useRef(onOpen);
  const onBeforeCloseRef = useRef(onBeforeClose);
  const onAfterCloseRef = useRef(onAfterClose);
  
  useEffect(() => {
    onCloseRef.current = onClose;
  }, [onClose]);
  
  useEffect(() => {
    onOpenRef.current = onOpen;
  }, [onOpen]);
  
  useEffect(() => {
    onBeforeCloseRef.current = onBeforeClose;
  }, [onBeforeClose]);
  
  useEffect(() => {
    onAfterCloseRef.current = onAfterClose;
  }, [onAfterClose]);
  
  // Register/unregister layer based on isOpen state
  useEffect(() => {
    if (isOpen) {
      pushLayer({
        id: layerId,
        onClose: () => onCloseRef.current(),
        onOpen: () => onOpenRef.current?.(),
        onBeforeClose: () => onBeforeCloseRef.current?.() ?? true,
        onAfterClose: () => onAfterCloseRef.current?.(),
        metadata,
        dismissible,
        blockScroll,
        trapFocus,
      });
    } else {
      removeLayer(layerId);
    }
    
    // Cleanup on unmount
    return () => {
      removeLayer(layerId);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen, layerId, pushLayer, removeLayer, dismissible, blockScroll, trapFocus]);
  
  // Handle ESC key press
  useEffect(() => {
    if (!enableEscapeKey || !isOpen || !dismissible) return;
    
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        const topLayer = getTopLayer();
        // Only close if this layer is the topmost one and is dismissible
        if (topLayer && topLayer.id === layerId && topLayer.dismissible !== false) {
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
  }, [enableEscapeKey, isOpen, layerId, getTopLayer, dismissible]);
  
  // Handle scroll blocking
  useEffect(() => {
    if (!isOpen || !blockScroll) return;
    
    const topLayer = getTopLayer();
    // Only block scroll if this is the topmost layer
    if (topLayer && topLayer.id === layerId) {
      const originalOverflow = document.body.style.overflow;
      const originalPaddingRight = document.body.style.paddingRight;
      
      // Calculate scrollbar width to prevent layout shift
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      
      document.body.style.overflow = 'hidden';
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      
      return () => {
        document.body.style.overflow = originalOverflow;
        document.body.style.paddingRight = originalPaddingRight;
      };
    }
  }, [isOpen, layerId, getTopLayer, blockScroll]);
  
  return {
    layerId,
  };
}
