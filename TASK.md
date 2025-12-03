1: Potrzebujemy Query (i query handler) które na podstawie Id translacji zwróci name tą główną translacje oraz powiązane translacje, 
tj. na podstawie id sprawdzamy co to za translacja i na podstawie (resourceName, oraz translationName) szukamy translacji z kazdego dostępnego cultureName
2: Powinnismy miec serwis który zwraca dostępne cultureName (w core project) + query
3: W projekcie UI w TranslationsPanel - powinismy przyjmowac Id translacji (lub null gdy tworzymy) i pobierac dane tam