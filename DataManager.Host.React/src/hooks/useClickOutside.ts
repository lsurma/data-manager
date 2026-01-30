import { useEffect, useRef } from 'react';

/**
 * Hook to detect clicks outside a referenced element.
 * Useful for closing dropdowns, modals, or drawers when clicking outside.
 * 
 * @param callback - Function to call when a click outside is detected
 * @param isActive - Whether the click detection is active
 * @returns Ref to attach to the element
 * 
 * @example
 * ```tsx
 * function MyDropdown({ isOpen, onClose }) {
 *   const ref = useClickOutside(() => onClose(), isOpen);
 *   
 *   if (!isOpen) return null;
 *   
 *   return (
 *     <div ref={ref}>
 *       <p>Dropdown content</p>
 *     </div>
 *   );
 * }
 * ```
 */
export function useClickOutside<T extends HTMLElement = HTMLElement>(
  callback: () => void,
  isActive: boolean = true
) {
  const ref = useRef<T>(null);
  const callbackRef = useRef(callback);
  
  // Keep callback up to date
  useEffect(() => {
    callbackRef.current = callback;
  }, [callback]);
  
  useEffect(() => {
    if (!isActive) return;
    
    const handleClick = (event: MouseEvent) => {
      // Check if click is outside the referenced element
      if (ref.current && !ref.current.contains(event.target as Node)) {
        callbackRef.current();
      }
    };
    
    // Add a small delay to avoid triggering on the same click that opened the element
    const timeoutId = setTimeout(() => {
      document.addEventListener('mousedown', handleClick);
    }, 0);
    
    return () => {
      clearTimeout(timeoutId);
      document.removeEventListener('mousedown', handleClick);
    };
  }, [isActive]);
  
  return ref;
}
