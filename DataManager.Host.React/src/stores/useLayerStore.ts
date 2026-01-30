import { create } from 'zustand';

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
}

interface LayerState {
  /** Stack of currently open layers (newest at the end) */
  layers: Layer[];
  /** Add a new layer to the stack */
  pushLayer: (layer: Layer) => void;
  /** Remove a layer from the stack by ID */
  removeLayer: (id: string) => void;
  /** Close the topmost layer (used for ESC key handling) */
  closeTopLayer: () => void;
  /** Get the topmost layer */
  getTopLayer: () => Layer | undefined;
  /** Check if a layer exists in the stack */
  hasLayer: (id: string) => boolean;
  /** Clear all layers */
  clearLayers: () => void;
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
      return { layers: [...state.layers, layer] };
    });
  },
  
  removeLayer: (id) => {
    set((state) => ({
      layers: state.layers.filter((layer) => layer.id !== id),
    }));
  },
  
  closeTopLayer: () => {
    const topLayer = get().getTopLayer();
    if (topLayer) {
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
}));
