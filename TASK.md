Dodaj obsługę do exportu plików:
1: W TranslationsPage mamy button "Export all to Excel (.xlsx)" dodaj do niego obsługę
2: Po kliknięciu w button, reobimy request "ExportTranslationsQuery" używając "IRequestSender"
3: W QueryController dodaj obslugę zwrotu dla plików podobnie tak jak mamy w "ExportController" ale ExportController nie będzie używany