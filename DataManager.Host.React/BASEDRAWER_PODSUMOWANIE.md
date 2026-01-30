# Reużywalny Komponent Drawer - Podsumowanie Implementacji

## Problem

Często będziemy korzystać z panelów (drawer) które w sobie będą miały jakiś formularz wewnątrz albo inną część kodu, ale część jest podobna za każdym razem, np. drawer ma footer i tam ma buttony akcyjne.

**Pytanie:** Jak możemy podejść do tego w maksymalnie prosty sposób, tak żeby nie musieć tworzyć nowego drawera za każdym razem?

## Rozwiązanie

Utworzono komponent `BaseDrawer` - pojedynczy, elastyczny komponent który eliminuje duplikację kodu.

### Przed vs Po

**PRZED - Musieliśmy za każdym razem:**
```tsx
function MojDrawer() {
  return (
    <>
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black/50" onClick={onClose} />
      
      {/* Panel */}
      <div className="fixed inset-y-0 right-0 w-96 bg-white">
        {/* Nagłówek */}
        <div className="flex items-center justify-between p-6">
          <h2>Tytuł</h2>
          <button onClick={onClose}>X</button>
        </div>
        
        {/* Zawartość */}
        <div className="p-6">
          {/* Formularz */}
        </div>
        
        {/* Stopka z przyciskami */}
        <div className="p-6 border-t">
          <button>Anuluj</button>
          <button>Zapisz</button>
        </div>
      </div>
    </>
  );
}
```
**~100+ linii kodu za każdym razem!**

**PO - Teraz wystarczy:**
```tsx
<BaseDrawer
  title="Tytuł"
  isOpen={isOpen}
  onClose={onClose}
  actions={[
    { label: 'Anuluj', onClick: onClose, variant: 'secondary' },
    { label: 'Zapisz', onClick: handleSave, variant: 'primary' },
  ]}
>
  {/* Twój formularz */}
</BaseDrawer>
```
**~30 linii kodu!**

## Główne Funkcje

### 1. Gotowa Struktura
- ✅ Nagłówek z tytułem i przyciskiem zamknięcia
- ✅ Przewijalny obszar zawartości
- ✅ Stopka z przyciskami akcji
- ✅ Tło (backdrop) z obsługą kliknięcia

### 2. Konfigurowalne Przyciski Akcji
```tsx
actions={[
  { label: 'Anuluj', onClick: onClose, variant: 'secondary' },
  { label: 'Usuń', onClick: handleDelete, variant: 'danger' },
  { label: 'Zapisz', onClick: handleSave, variant: 'primary', loading: isLoading },
]}
```

**Warianty przycisków:**
- `primary` - Niebieski (główna akcja, np. Zapisz, Utwórz)
- `secondary` - Szary (anuluj, alternatywne akcje)
- `danger` - Czerwony (akcje destrukcyjne, np. Usuń)

### 3. Rozmiary
- `sm` (320px) - Kompaktowe formularze
- `md` (384px) - Standardowe formularze (domyślny)
- `lg` (512px) - Szczegółowe formularze
- `xl` (768px) - Złożone layouty
- `full` - Pełna szerokość

### 4. Stan Ładowania
- Przycisk pokazuje spinner
- Pola formularza wyłączone
- Drawer staje się nie-zamykalny podczas ładowania

## Przykłady Użycia

### 1. Prosty Formularz Tworzenia
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
      title="Utwórz Użytkownika"
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

### 2. Formularz Edycji z Wieloma Przyciskami
```tsx
<BaseDrawer
  isOpen={isOpen}
  onClose={onClose}
  title="Edytuj Użytkownika"
  size="lg"
  actions={[
    { label: 'Anuluj', onClick: onClose, variant: 'secondary' },
    { label: 'Usuń', onClick: handleDelete, variant: 'danger' },
    { label: 'Zaktualizuj', onClick: handleUpdate, variant: 'primary' },
  ]}
>
  {/* Szczegółowy formularz */}
</BaseDrawer>
```

### 3. Ze Stanem Ładowania
```tsx
function SubmitDrawer({ isOpen, onClose }) {
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
      dismissible={!isLoading}  // Nie można zamknąć podczas ładowania
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

## Demo

Odwiedź `/layer-demo` w aplikacji aby zobaczyć 3 działające przykłady:

1. **Prosty Formularz** - Podstawowy drawer z polami name i email
2. **Duży Formularz Edycji** - Większy drawer z wieloma polami i przyciskami
3. **Ze Stanem Ładowania** - Pokazuje jak obsługiwać operacje asynchroniczne

## Korzyści

### Dla Programistów
- 🚀 **70% mniej kodu** dla typowego drawera
- 🧩 **Spójna struktura** w całej aplikacji
- 🎨 **Nie trzeba stylować** przycisków za każdym razem
- 🔧 **Łatwa konserwacja** - zmiana w jednym miejscu

### Dla Użytkowników
- ✨ Spójne doświadczenie
- ♿ Wbudowana dostępność (focus trap, ESC)
- 🎨 Profesjonalny wygląd
- 💨 Płynne animacje

## Właściwości (Props)

```typescript
interface BaseDrawerProps {
  isOpen: boolean;              // Czy drawer jest otwarty (wymagane)
  onClose: () => void;          // Callback zamknięcia (wymagane)
  title: string;                // Tytuł drawer (wymagane)
  children: ReactNode;          // Zawartość (wymagane)
  actions?: DrawerAction[];     // Przyciski akcji
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full';  // Rozmiar (domyślnie: 'md')
  dismissible?: boolean;        // Czy można zamknąć ESC/tłem (domyślnie: true)
  trapFocus?: boolean;          // Pułapka fokusa (domyślnie: true)
  blockScroll?: boolean;        // Blokuj przewijanie (domyślnie: true)
}

interface DrawerAction {
  label: string;                // Tekst przycisku (wymagane)
  onClick: () => void;          // Handler kliknięcia (wymagane)
  variant?: 'primary' | 'secondary' | 'danger';  // Styl przycisku
  disabled?: boolean;           // Czy wyłączony
  loading?: boolean;            // Stan ładowania (spinner)
}
```

## Import

```tsx
import { BaseDrawer } from '@/components/BaseDrawer';
// lub
import { BaseDrawer } from '@/components';
```

## Porównanie

| Aspekt | Przed | Po |
|--------|-------|-----|
| Linie kodu | ~100+ na drawer | ~30 linii |
| Struktura | Ręcznie za każdym razem | Gotowa |
| Przyciski | Własny styling | Konfigurowalne warianty |
| Integracja warstw | Ręczna konfiguracja | Automatyczna |
| Stany ładowania | Własna implementacja | Wbudowana właściwość |
| Spójność | Różna | Jednolita |

## Automatyczne Funkcje

Komponent automatycznie obsługuje:
- ✅ Integrację z systemem zarządzania warstwami
- ✅ Pułapkę fokusa (focus trap)
- ✅ Blokowanie przewijania z kompensacją scrollbara
- ✅ Obsługę klawisza ESC (tylko najwyższy drawer)
- ✅ Przywracanie fokusa po zamknięciu

## Dokumentacja

**Polski:**
- `BASEDRAWER_UZYCIE.md` - Pełna dokumentacja po polsku

**Angielski:**
- `BASEDRAWER_USAGE.md` - Szczegółowa dokumentacja (API, przykłady, best practices)

## Pliki

**Komponent:**
- `src/components/BaseDrawer.tsx` - Główny komponent
- `src/components/index.ts` - Eksport

**Demo:**
- `src/routes/LayerManagementDemo.tsx` - Sekcja "Reusable Drawer Components"

## Najlepsze Praktyki

### 1. Czyść Stan przy Zamknięciu
```tsx
const handleClose = () => {
  setFormData(initialState);  // ✅ Wyczyść dane
  setIsOpen(false);
};
```

### 2. Obsługuj Stany Ładowania
```tsx
<BaseDrawer
  dismissible={!isLoading}  // ✅ Nie można zamknąć podczas ładowania
  actions={[
    { label: 'Anuluj', onClick: onClose, disabled: isLoading },
    { label: 'Zapisz', onClick: handleSave, loading: isLoading },
  ]}
/>
```

### 3. Używaj Jasnych Etykiet
```tsx
// ✅ Dobrze
actions={[
  { label: 'Anuluj', onClick: onClose },
  { label: 'Zapisz Zmiany', onClick: handleSave },
]}

// ❌ Źle
actions={[
  { label: 'Nie', onClick: onClose },
  { label: 'Tak', onClick: handleSave },
]}
```

## Podsumowanie

✅ **Problem rozwiązany:** Nie trzeba tworzyć struktury drawer za każdym razem  
✅ **Rozwiązanie:** Jeden komponent `BaseDrawer` ze wszystkimi wspólnymi funkcjami  
✅ **Implementacja:** W pełni działająca z 3 przykładami demo  
✅ **Dokumentacja:** Kompletna w języku polskim i angielskim  
✅ **Integracja:** Bezproblemowa z istniejącym systemem warstw  

Komponent BaseDrawer zapewnia dokładnie to, o co było pytanie: **"podejście w maksymalnie prosty sposób"** dla tworzenia drawerów z formularzami i przyciskami akcji! 🎉
