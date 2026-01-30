# Rozszerzone Funkcje Zarządzania Warstwami

Ten dokument opisuje dodatkowe funkcje dodane do systemu zarządzania warstwami wykraczające poza podstawową funkcjonalność.

## Przegląd Nowych Funkcji

Rozszerzony system zarządzania warstwami zawiera teraz:

1. **Warstwy nie-zamykalne** - Krytyczne dialogi wymagające wyraźnej akcji użytkownika
2. **Pułapka fokusa** - Utrzymuje nawigację klawiaturą w obrębie aktywnej warstwy
3. **Wykrywanie kliknięcia poza** - Hook użytkowy do obsługi kliknięć w tło
4. **Blokowanie przewijania** - Zapobiega przewijaniu body z inteligentną kompensacją scrollbara
5. **Przywracanie fokusa** - Automatycznie przywraca fokus do poprzedniego elementu
6. **Callbacki cyklu życia** - Hooki dla zdarzeń otwarcia, przed zamknięciem i po zamknięciu
7. **Śledzenie właściwości warstw** - Dokładna kontrola nad zachowaniem warstw
8. **Rozszerzone metody store** - Dodatkowe metody do manipulacji warstwami

## Rozszerzony Interfejs Warstwy

```typescript
interface Layer {
  id: string;
  onClose?: () => void;
  metadata?: Record<string, unknown>;
  
  // NOWE WŁAŚCIWOŚCI
  dismissible?: boolean;                    // Czy można zamknąć przez ESC (domyślnie: true)
  blockScroll?: boolean;                    // Blokuj przewijanie body (domyślnie: true)
  trapFocus?: boolean;                      // Pułapka fokusa w warstwie (domyślnie: true)
  animationState?: LayerAnimationState;     // Śledzenie stanu animacji
  previousActiveElement?: Element | null;   // Do przywracania fokusa
  onOpen?: () => void;                      // Wywoływane przy otwarciu warstwy
  onBeforeClose?: () => boolean | Promise<boolean>; // Może zapobiec zamknięciu
  onAfterClose?: () => void;                // Wywoływane po zamknięciu warstwy
}
```

## 1. Warstwy Nie-Zamykalne (Non-Dismissible)

Warstwy nie-zamykalne nie mogą być zamknięte przez naciśnięcie ESC lub kliknięcie poza nimi. Jest to przydatne dla krytycznych akcji wymagających wyraźnego potwierdzenia użytkownika.

### Użycie

```typescript
useLayer({
  isOpen,
  onClose,
  id: 'critical-action',
  dismissible: false,  // Zapobiega zamknięciu ESC i kliknięciem poza
});
```

### Przykład: Dialog Potwierdzenia

```tsx
function DeleteConfirmation({ isOpen, onClose, onConfirm }) {
  useLayer({
    isOpen,
    onClose,
    id: 'delete-confirmation',
    dismissible: false,  // Musi kliknąć przycisk
    onBeforeClose: async () => {
      // Walidacja przed zamknięciem
      return await confirmAction();
    },
  });
  
  if (!isOpen) return null;
  
  return (
    <div className="modal">
      <h2>⚠️ Usunąć Element?</h2>
      <p>Ta akcja jest nieodwracalna.</p>
      <button onClick={onClose}>Anuluj</button>
      <button onClick={() => { onConfirm(); onClose(); }}>Usuń</button>
    </div>
  );
}
```

## 2. Pułapka Fokusa (Focus Trap)

Funkcja pułapki fokusa utrzymuje nawigację klawiaturą (klawisz Tab) w obrębie aktywnej warstwy, poprawiając dostępność i UX.

### Hook useFocusTrap

```typescript
import { useFocusTrap } from '@/hooks/useFocusTrap';

function MyModal({ isOpen, onClose }) {
  const containerRef = useFocusTrap(isOpen);
  
  return (
    <div ref={containerRef}>
      <input type="text" />
      <button>Akcja</button>
      <button onClick={onClose}>Zamknij</button>
    </div>
  );
}
```

### Funkcje

- Automatycznie fokusuje pierwszy element możliwy do sfokusowania
- Tab przełącza między elementami w kontenerze
- Shift+Tab nawiguje wstecz
- Obsługuje dynamicznie dodawane/usuwane elementy

## 3. Wykrywanie Kliknięcia Poza (Click Outside)

Hook `useClickOutside` wykrywa, gdy użytkownik kliknie poza wskazanym elementem.

### Użycie

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
        <li>Opcja 1</li>
        <li>Opcja 2</li>
      </ul>
    </div>
  );
}
```

## 4. Blokowanie Przewijania (Scroll Blocking)

Automatycznie blokuje przewijanie body gdy warstwy są otwarte, z inteligentną kompensacją szerokości scrollbara aby zapobiec przeskokowi layoutu.

### Automatyczne Zachowanie

```typescript
useLayer({
  isOpen,
  onClose,
  id: 'my-modal',
  blockScroll: true,  // Domyślnie: true
});
```

### Jak To Działa

1. Mierzy szerokość scrollbara przed zablokowaniem
2. Ustawia `overflow: hidden` na body
3. Dodaje padding-right aby skompensować szerokość scrollbara
4. Przywraca oryginalne style gdy warstwa się zamyka

## 5. Przywracanie Fokusa (Focus Restoration)

System automatycznie zapamiętuje który element miał fokus przed otwarciem warstwy i przywraca fokus do tego elementu gdy warstwa się zamyka.

### Automatyczne Zachowanie

```typescript
useLayer({
  isOpen,
  onClose,
  id: 'my-modal',
});

// Gdy modal się otwiera, aktualny aktywny element jest zapisywany
// Gdy modal się zamyka, fokus jest przywracany do tego elementu
```

### Przypadek Użycia

Jest to szczególnie przydatne dla:
- Użytkowników nawigacji klawiaturą
- Użytkowników czytników ekranu
- Utrzymania kontekstu po zamknięciu dialogów

## 6. Callbacki Cyklu Życia (Event Lifecycle)

Trzy hooki callbacków zapewniają dokładną kontrolę nad cyklem życia warstwy:

### onOpen

Wywoływany natychmiast gdy warstwa jest dodawana do stosu.

```typescript
useLayer({
  isOpen,
  onClose,
  onOpen: () => {
    console.log('Warstwa otwarta');
    trackAnalyticsEvent('modal_opened');
  },
});
```

### onBeforeClose

Wywoływany przed zamknięciem. Zwróć `false` aby zapobiec zamknięciu.

```typescript
useLayer({
  isOpen,
  onClose,
  onBeforeClose: () => {
    if (hasUnsavedChanges) {
      return confirm('Masz niezapisane zmiany. Zamknąć mimo to?');
    }
    return true;
  },
});
```

Może być również async:

```typescript
onBeforeClose: async () => {
  const result = await showConfirmDialog();
  return result.confirmed;
}
```

### onAfterClose

Wywoływany po usunięciu warstwy ze stosu.

```typescript
useLayer({
  isOpen,
  onClose,
  onAfterClose: () => {
    console.log('Warstwa zamknięta i wyczyszczona');
    resetFormData();
  },
});
```

### Kolejność Zdarzeń

1. Użytkownik inicjuje zamknięcie (klawisz ESC, kliknięcie przycisku, itp.)
2. `onBeforeClose()` - Może zapobiec zamknięciu
3. `onClose()` - Główny callback aktualizujący stan komponentu
4. Warstwa jest usuwana ze stosu
5. Fokus jest przywracany (jeśli dotyczy)
6. `onAfterClose()` - Czyszczenie i efekty uboczne

## 7. Śledzenie Właściwości Warstw

Każda warstwa śledzi teraz dodatkowe właściwości wpływające na jej zachowanie:

```typescript
interface Layer {
  dismissible?: boolean;     // Czy można zamknąć ESC
  blockScroll?: boolean;     // Blokuje przewijanie body
  trapFocus?: boolean;       // Pułapka fokusa klawiatury
  animationState?: 'entering' | 'entered' | 'exiting' | 'exited';
}
```

### Wizualny Feedback

Komponent demo pokazuje te właściwości jako znaczki (badges):

- **Non-dismissible** - Czerwony znaczek
- **Scroll blocked** - Niebieski znaczek
- **Focus trapped** - Zielony znaczek

## 8. Rozszerzone Metody Store

Nowe metody dodane do `useLayerStore`:

### getLayer(id)

Pobierz konkretną warstwę po ID.

```typescript
const layer = useLayerStore.getState().getLayer('my-modal');
console.log(layer?.metadata);
```

### updateLayer(id, updates)

Aktualizuj właściwości warstwy dynamicznie.

```typescript
const { updateLayer } = useLayerStore();
updateLayer('my-modal', {
  dismissible: false,
  metadata: { step: 2 },
});
```

### getLayerCount()

Pobierz całkowitą liczbę otwartych warstw.

```typescript
const layerCount = useLayerStore((state) => state.getLayerCount());
console.log(`${layerCount} warstw otwartych`);
```

### closeAllDismissible()

Zamknij wszystkie warstwy które są zamykalne.

```typescript
const { closeAllDismissible } = useLayerStore();
closeAllDismissible(); // Zamyka wszystkie oprócz nie-zamykalnych
```

## Kompletny Przykład: Modal Formularza ze Wszystkimi Funkcjami

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
      console.log('Formularz otwarty');
    },
    onBeforeClose: () => {
      if (hasChanges) {
        return confirm('Odrzucić niezapisane zmiany?');
      }
      return true;
    },
    onAfterClose: () => {
      setHasChanges(false);
      console.log('Formularz zamknięty i zresetowany');
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
        <h2>Formularz Użytkownika</h2>
        <input
          type="text"
          onChange={() => setHasChanges(true)}
          placeholder="Imię"
        />
        <div className="flex gap-2">
          <button onClick={onClose}>Anuluj</button>
          <button onClick={() => { onSave(); onClose(); }}>
            Zapisz
          </button>
        </div>
      </div>
    </div>
  );
}
```

## Najlepsze Praktyki

### 1. Warstwy Nie-Zamykalne

Używaj oszczędnie, tylko dla krytycznych akcji:
- Usuwanie ważnych danych
- Potwierdzanie destrukcyjnych akcji
- Wieloetapowe procesy które nie powinny być przerywane

### 2. Pułapka Fokusa

Zawsze używaj dla dostępności:
- Modale i dialogi
- Każda nakładka która przejmuje fokus

### 3. Blokowanie Przewijania

Domyślnie włączone, ale można wyłączyć dla:
- Dropdownów i tooltipów
- Małych nakładek które nie potrzebują blokowania przewijania

### 4. Callbacki Zdarzeń

Używaj dla:
- Śledzenia analityki (`onOpen`)
- Walidacji (`onBeforeClose`)
- Czyszczenia (`onAfterClose`)
- Efektów ubocznych

## Rozwiązywanie Problemów

### Pułapka Fokusa Nie Działa

- Upewnij się że kontener ma elementy możliwe do sfokusowania
- Sprawdź czy elementy nie są wyłączone (disabled)
- Zweryfikuj że prop `isActive` jest true

### Problemy z Blokowaniem Przewijania

- Sprawdź czy jest wiele warstw z blokowaniem przewijania
- Zweryfikuj że element body nie jest nadpisany przez inne style
- Przetestuj kalkulację szerokości scrollbara na różnych przeglądarkach

### Callbacki Zdarzeń Nie Działają

- Upewnij się że callbacki są stabilne (użyj `useCallback`)
- Sprawdź czy warstwa jest poprawnie zarejestrowana
- Zweryfikuj że stan `isOpen` zmienia się poprawnie

## Powiązana Dokumentacja

- [Główna Dokumentacja](./LAYER_MANAGEMENT.md) - Podstawowe zarządzanie warstwami (po angielsku)
- [Dokumentacja Polski Podstawowa](./IMPLEMENTACJA_WARSTW.md) - Wersja polska podstawowa
- Demo: Odwiedź route `/layer-demo` dla interaktywnych przykładów
