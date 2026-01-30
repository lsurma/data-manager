# Komponent Reużywalnego Drawera (BaseDrawer)

## Przegląd

Komponent `BaseDrawer` to elastyczny, reużywalny komponent drawer (panel boczny), który eliminuje potrzebę tworzenia tej samej struktury za każdym razem gdy potrzebujesz drawer z formularzem lub własną zawartością.

## Rozwiązany Problem

Wcześniej, przy tworzeniu drawerów z formularzami, programiści musieli:
- Tworzyć całą strukturę drawer od nowa (backdrop, nagłówek, zawartość, stopka)
- Ręcznie implementować style przycisków akcji
- Za każdym razem obsługiwać integrację z systemem warstw
- Duplikować kod dla wspólnych wzorców

Komponent `BaseDrawer` rozwiązuje to przez dostarczenie gotowego do użycia drawer z:
- Pre-zbudowanym nagłówkiem z tytułem i przyciskiem zamknięcia
- Przewijalnym obszarem zawartości dla własnej treści
- Opcjonalną stopką z konfigurowalnymi przyciskami akcji
- Pełną integracją z systemem zarządzania warstwami
- Wsparciem dla stanów ładowania i wielu wariantów przycisków

## Podstawowe Użycie

```tsx
import { useState } from 'react';
import { BaseDrawer } from '@/components/BaseDrawer';

function MyComponent() {
  const [isOpen, setIsOpen] = useState(false);
  
  return (
    <>
      <button onClick={() => setIsOpen(true)}>Otwórz Drawer</button>
      
      <BaseDrawer
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        title="Utwórz Nowego Użytkownika"
        actions={[
          { label: 'Anuluj', onClick: () => setIsOpen(false), variant: 'secondary' },
          { label: 'Zapisz', onClick: handleSave, variant: 'primary' },
        ]}
      >
        <form>
          <input type="text" placeholder="Imię" />
          <input type="email" placeholder="Email" />
        </form>
      </BaseDrawer>
    </>
  );
}
```

## Właściwości (Props)

### BaseDrawerProps

| Właściwość | Typ | Domyślna | Opis |
|------------|-----|----------|------|
| `isOpen` | `boolean` | - | Czy drawer jest otwarty (wymagane) |
| `onClose` | `() => void` | - | Callback gdy drawer powinien się zamknąć (wymagane) |
| `title` | `string` | - | Tytuł drawer wyświetlany w nagłówku (wymagane) |
| `children` | `ReactNode` | - | Zawartość drawer (wymagane) |
| `actions` | `DrawerAction[]` | `undefined` | Przyciski akcji w stopce |
| `footer` | `ReactNode` | `undefined` | Własna zawartość stopki (nadpisuje actions) |
| `size` | `'sm' \| 'md' \| 'lg' \| 'xl' \| 'full'` | `'md'` | Szerokość drawer |
| `id` | `string` | auto-generowane | Unikalne ID dla zarządzania warstwami |
| `showCloseButton` | `boolean` | `true` | Pokaż przycisk zamknięcia w nagłówku |
| `dismissible` | `boolean` | `true` | Czy można zamknąć ESC lub kliknięciem tła |
| `trapFocus` | `boolean` | `true` | Pułapka fokusa klawiatury w drawer |
| `blockScroll` | `boolean` | `true` | Blokuj przewijanie body gdy otwarty |

### DrawerAction

| Właściwość | Typ | Domyślna | Opis |
|------------|-----|----------|------|
| `label` | `string` | - | Tekst przycisku (wymagane) |
| `onClick` | `() => void` | - | Handler kliknięcia (wymagane) |
| `variant` | `'primary' \| 'secondary' \| 'danger'` | `'secondary'` | Styl przycisku |
| `disabled` | `boolean` | `false` | Czy przycisk jest wyłączony |
| `loading` | `boolean` | `false` | Pokaż spinner ładowania |

## Opcje Rozmiaru

Drawer wspiera 5 predefiniowanych rozmiarów:

- `sm` - 320px (20rem)
- `md` - 384px (24rem) - Domyślny
- `lg` - 512px (32rem)
- `xl` - 768px (48rem)
- `full` - Pełna szerokość z ograniczeniem max-width

## Przykłady

### Prosty Drawer z Formularzem

```tsx
function CreateUserDrawer({ isOpen, onClose }) {
  const [formData, setFormData] = useState({ name: '', email: '' });
  
  const handleSave = () => {
    console.log('Zapisywanie:', formData);
    onClose();
  };
  
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Utwórz Nowego Użytkownika"
      actions={[
        { label: 'Anuluj', onClick: onClose, variant: 'secondary' },
        { label: 'Zapisz', onClick: handleSave, variant: 'primary' },
      ]}
    >
      <div className="space-y-4">
        <input
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="Imię"
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

### Duży Drawer z Wieloma Akcjami

```tsx
function EditUserDrawer({ isOpen, onClose }) {
  return (
    <BaseDrawer
      isOpen={isOpen}
      onClose={onClose}
      title="Edytuj Profil Użytkownika"
      size="lg"
      actions={[
        { label: 'Anuluj', onClick: onClose, variant: 'secondary' },
        { label: 'Usuń', onClick: handleDelete, variant: 'danger' },
        { label: 'Aktualizuj', onClick: handleUpdate, variant: 'primary' },
      ]}
    >
      {/* Zawartość formularza */}
    </BaseDrawer>
  );
}
```

### Drawer ze Stanem Ładowania

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
      title="Wyślij Dane"
      dismissible={!isLoading}  // Zapobiegnij zamknięciu podczas ładowania
      actions={[
        { 
          label: 'Anuluj', 
          onClick: onClose, 
          variant: 'secondary',
          disabled: isLoading 
        },
        { 
          label: 'Wyślij', 
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

## Funkcje

### Automatyczna Integracja z Zarządzaniem Warstwami

Drawer automatycznie integruje się z systemem zarządzania warstwami:
- Śledzi pozycję w stosie warstw
- Tylko najwyższy drawer reaguje na klawisz ESC
- Właściwe zarządzanie fokusem i przywracanie
- Blokowanie przewijania z kompensacją scrollbara

### Pułapka Fokusa

Gdy `trapFocus={true}` (domyślnie), nawigacja klawiaturą pozostaje w drawer:
- Tab przełącza między elementami możliwymi do sfokusowania
- Pierwszy element otrzymuje fokus przy otwarciu
- Fokus wraca do elementu wyzwalającego przy zamknięciu

### Stan Ładowania

Przyciski akcji wspierają stan ładowania:
- Pokazuje ikonę spinnera
- Wyłącza przycisk
- Opcjonalnie czyni drawer nie-zamykalnym podczas ładowania

### Warianty Przycisków

Trzy wbudowane style przycisków:
- `primary` - Niebieskie tło (główna akcja)
- `secondary` - Szare tło (anuluj/alternatywa)
- `danger` - Czerwone tło (akcje destrukcyjne)

## Najlepsze Praktyki

### 1. Używaj Znaczących ID

```tsx
<BaseDrawer
  id="edit-user-123"  // Przydatne do debugowania
  // ...
/>
```

### 2. Właściwie Zarządzaj Stanem

```tsx
// ✅ Dobrze - Wyczyść stan przy zamknięciu
const handleClose = () => {
  setFormData(initialState);
  setIsOpen(false);
};

// ❌ Źle - Stan pozostaje między otwarciami
const handleClose = () => setIsOpen(false);
```

### 3. Obsługuj Stany Ładowania

```tsx
// ✅ Dobrze - Wyłącz interakcje podczas ładowania
<BaseDrawer
  dismissible={!isLoading}
  actions={[
    { label: 'Anuluj', onClick: onClose, disabled: isLoading },
    { label: 'Zapisz', onClick: handleSave, loading: isLoading },
  ]}
>
  <form disabled={isLoading}>...</form>
</BaseDrawer>
```

## Demo

Odwiedź `/layer-demo` aby zobaczyć interaktywne przykłady wszystkich funkcji BaseDrawer!

## Powiązana Dokumentacja

- [System Zarządzania Warstwami](./LAYER_MANAGEMENT.md) (EN)
- [Rozszerzone Funkcje Warstw](./ROZSZERZONE_FUNKCJE_WARSTW.md) (PL)
- [Dokumentacja Użycia BaseDrawer](./BASEDRAWER_USAGE.md) (EN - szczegółowa)
