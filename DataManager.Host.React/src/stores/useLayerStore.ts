import { create } from 'zustand';

/**
 * Animation state for a layer
 */
export type LayerAnimationState = 'entering' | 'entered' | 'exiting' | 'exited';

/**
 * Represents a single layer (modal, drawer, dialog, etc.)
 */
export interface Layer {
  /** Unique identifier for the layer */
  id: string;
  /** Optional callback to execute when the layer is closed (e.g., via ESC key) */
  onClose?: () => void;
  /** Optional metadata for the layer */
  metadata?: Record<string, unknown>;
  /** Whether this layer can be dismissed via ESC key or click outside */
  dismissible?: boolean;
  /** Whether to block scroll on the body when this layer is open */
  blockScroll?: boolean;
  /** Whether to trap focus within this layer */
  trapFocus?: boolean;
  /** Animation state of the layer */
  animationState?: LayerAnimationState;
  /** Element that had focus before the layer opened (for restoration) */
  previousActiveElement?: Element | null;
  /** Callback executed when layer starts opening */
  onOpen?: () => void;
  /** Callback executed before layer closes (can prevent closing by returning false) */
  onBeforeClose?: () => boolean | Promise<boolean>;
  /** Callback executed after layer has closed */
  onAfterClose?: () => void;
}

interface LayerState {
  /** Stack of currently open layers (newest at the end) */
  layers: Layer[];
  /** Add a new layer to the stack */
  pushLayer: (layer: Layer) => void;
  /** Remove a layer from the stack by ID */
  removeLayer: (id: string) => void;
  /** Close the topmost layer (used for ESC key handling) */
  closeTopLayer: () => Promise<void>;
  /** Get the topmost layer */
  getTopLayer: () => Layer | undefined;
  /** Check if a layer exists in the stack */
  hasLayer: (id: string) => boolean;
  /** Clear all layers */
  clearLayers: () => void;
  /** Get layer by ID */
  getLayer: (id: string) => Layer | undefined;
  /** Update a layer's properties */
  updateLayer: (id: string, updates: Partial<Layer>) => void;
  /** Get the number of open layers */
  getLayerCount: () => number;
  /** Close all dismissible layers */
  closeAllDismissible: () => void;
}

/**
 * Zustand store for managing overlay layers (modals, drawers, dialogs)
 * 
 * Features:
 * - Stack-based layer management (LIFO)
 * - ESC key support for closing the topmost layer
 * - Callbacks for custom close behavior
 * 
 * @example
 * ```ts
 * const { pushLayer, removeLayer } = useLayerStore();
 * 
 * // Register a layer
 * pushLayer({ id: 'my-modal', onClose: () => setIsOpen(false) });
 * 
 * // Remove a layer
 * removeLayer('my-modal');
 * ```
 */
export const useLayerStore = create<LayerState>((set, get) => ({
  layers: [],
  
  pushLayer: (layer) => {
    set((state) => {
      // Prevent duplicate IDs
      if (state.layers.some((l) => l.id === layer.id)) {
        console.warn(`Layer with id "${layer.id}" already exists`);
        return state;
      }
      
      // Store the currently focused element for restoration
      const previousActiveElement = document.activeElement;
      
      // Execute onOpen callback
      layer.onOpen?.();
      
      return { 
        layers: [...state.layers, { 
          ...layer, 
          previousActiveElement,
          animationState: 'entering' as LayerAnimationState,
        }] 
      };
    });
  },
  
  removeLayer: (id) => {
    const layer = get().getLayer(id);
    if (layer) {
      // Restore focus to the previous element if specified
      if (layer.previousActiveElement && layer.previousActiveElement instanceof HTMLElement) {
        layer.previousActiveElement.focus();
      }
      
      // Execute onAfterClose callback
      layer.onAfterClose?.();
    }
    
    set((state) => ({
      layers: state.layers.filter((layer) => layer.id !== id),
    }));
  },
  
  closeTopLayer: async () => {
    const topLayer = get().getTopLayer();
    if (topLayer) {
      // Check if the layer can be closed
      if (topLayer.onBeforeClose) {
        const canClose = await topLayer.onBeforeClose();
        if (!canClose) {
          return; // Prevent closing
        }
      }
      
      // Execute the onClose callback if provided
      topLayer.onClose?.();
      // Remove the layer from the stack
      get().removeLayer(topLayer.id);
    }
  },
  
  getTopLayer: () => {
    const layers = get().layers;
    return layers[layers.length - 1];
  },
  
  hasLayer: (id) => {
    return get().layers.some((layer) => layer.id === id);
  },
  
  clearLayers: () => {
    set({ layers: [] });
  },
  
  getLayer: (id) => {
    return get().layers.find((layer) => layer.id === id);
  },
  
  updateLayer: (id, updates) => {
    set((state) => ({
      layers: state.layers.map((layer) =>
        layer.id === id ? { ...layer, ...updates } : layer
      ),
    }));
  },
  
  getLayerCount: () => {
    return get().layers.length;
  },
  
  closeAllDismissible: () => {
    const dismissibleLayers = get().layers.filter((layer) => layer.dismissible !== false);
    dismissibleLayers.forEach((layer) => {
      layer.onClose?.();
      get().removeLayer(layer.id);
    });
  },
}));
