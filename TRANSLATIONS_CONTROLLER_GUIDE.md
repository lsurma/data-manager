# TranslationsController API Guide

This guide explains how to use the new TranslationsController endpoints for easier access to translation data.

## Overview

The TranslationsController provides simplified REST endpoints for working with translations, offering a more intuitive alternative to the generic query/command endpoints.

## Endpoints

### 1. GET Translations

**Endpoint:** `GET /api/translations/{dataSetNameOrId}`

Retrieves translations for a specific dataset with optional filtering, pagination, and sorting. Returns simplified translation data with only essential fields (id, translationName, cultureName, content).

#### Route Parameters

- `dataSetNameOrId` - The TranslationsSet identifier (can be either the TranslationsSet name or GUID)

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `orderBy` | string | - | Field name to sort by (e.g., "translationName", "cultureName") |
| `orderDirection` | string | "asc" | Sort direction: "asc" or "desc" |
| `limit` | integer | 20 | Number of items per page (page size) |
| `offset` | integer | 0 | Number of items to skip (for pagination) |

#### Examples

**Get first 20 translations from dataset by name:**
```
GET /api/translations/MyDataSet
```

**Get translations with custom limit:**
```
GET /api/translations/MyDataSet?limit=50
```

**Get translations with pagination (page 2, 10 items per page):**
```
GET /api/translations/MyDataSet?limit=10&offset=10
```

**Get translations sorted by translation name:**
```
GET /api/translations/MyDataSet?orderBy=translationName&orderDirection=asc
```

**Get translations by TranslationsSet ID:**
```
GET /api/translations/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Complex query with all parameters:**
```
GET /api/translations/MyDataSet?orderBy=translationName&orderDirection=asc&limit=25&offset=50
```

#### Response Format

This endpoint returns a **simplified projection** with only essential fields for better performance and reduced payload size.

**Response Schema:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "translationName": "MyTranslation",
      "cultureName": "en-US",
      "content": "Hello World"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Fields Included:**
- `id` (Guid) - Unique identifier
- `translationName` (string) - Translation name
- `cultureName` (string, nullable) - Culture/language code (e.g., "en-US", "fr-FR")
- `content` (string) - The translated content

> **Note:** For full translation details including metadata, resource names, and relationships, use the generic `/api/query/GetTranslationsQuery` endpoint.
```

#### Status Codes

- `200 OK` - Success, returns paginated list of translations
- `404 Not Found` - TranslationsSet with the specified name or ID not found
- `401 Unauthorized` - Authentication required (if authentication is enabled)
- `500 Internal Server Error` - Server error occurred

---

### 2. POST Import Translations

**Endpoint:** `POST /api/translations/{dataSetNameOrId}`

Imports translations from a remote source by accepting a JSON array of translation objects.

#### Route Parameters

- `dataSetNameOrId` - The TranslationsSet identifier (can be either the TranslationsSet name or GUID)

#### Request Format

This endpoint expects a JSON array of translation objects in the request body.

**Request Body Schema:**
```json
[
  {
    "cultureName": "en-US",
    "resourceName": "MyResource",
    "translationName": "MyTranslation",
    "content": "Hello World",
    "internalGroupName1": "Group1",
    "internalGroupName2": "Group2",
    "contentTemplate": null
  }
]
```

**Required Fields:**
- `resourceName` (string) - The resource identifier
- `translationName` (string) - The translation identifier
- `content` (string) - The translated content

**Optional Fields:**
- `cultureName` (string) - The culture/language code (e.g., "en-US", "fr-FR")
- `internalGroupName1` (string) - First grouping level
- `internalGroupName2` (string) - Second grouping level
- `contentTemplate` (string) - Optional MJML template content before processing

#### Examples

**Using curl:**
```bash
curl -X POST http://localhost:7233/api/translations/MyDataSet \
  -H "Content-Type: application/json" \
  -d '[
    {
      "cultureName": "en-US",
      "resourceName": "Emails",
      "translationName": "WelcomeEmail",
      "content": "Welcome to our service!"
    },
    {
      "cultureName": "fr-FR",
      "resourceName": "Emails",
      "translationName": "WelcomeEmail",
      "content": "Bienvenue à notre service!"
    }
  ]'
```

**Using curl with TranslationsSet ID:**
```bash
curl -X POST http://localhost:7233/api/translations/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Content-Type: application/json" \
  -d '[{"cultureName":"en-US","resourceName":"Test","translationName":"Hello","content":"Hello World"}]'
```

**Using PowerShell:**
```powershell
$uri = "http://localhost:7233/api/translations/MyDataSet"
$body = @(
    @{
        cultureName = "en-US"
        resourceName = "Emails"
        translationName = "WelcomeEmail"
        content = "Welcome to our service!"
    },
    @{
        cultureName = "fr-FR"
        resourceName = "Emails"
        translationName = "WelcomeEmail"
        content = "Bienvenue à notre service!"
    }
) | ConvertTo-Json

Invoke-RestMethod -Uri $uri -Method Post -Body $body -ContentType "application/json"
```

**Using JavaScript/Fetch:**
```javascript
const response = await fetch('http://localhost:7233/api/translations/MyDataSet', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify([
    {
      cultureName: 'en-US',
      resourceName: 'Emails',
      translationName: 'WelcomeEmail',
      content: 'Welcome to our service!'
    }
  ])
});

const result = await response.json();
console.log(result);
```

**Using Postman:**
1. Set method to POST
2. Set URL to `/api/translations/MyDataSet`
3. Go to "Body" tab
4. Select "raw"
5. Choose "JSON" from the dropdown
6. Paste your JSON array
7. Click "Send"

#### Response Format

**Success:**
```json
{
  "message": "Translations import completed.",
  "dataSetId": "7b8e9f10-1234-5678-90ab-cdef12345678",
  "importedCount": 2,
  "failedCount": 0,
  "errors": []
}
```

**Partial Success (some failures):**
```json
{
  "message": "Translations import completed.",
  "dataSetId": "7b8e9f10-1234-5678-90ab-cdef12345678",
  "importedCount": 1,
  "failedCount": 1,
  "errors": [
    "Failed to import translation 'InvalidTranslation' (InvalidResource): Required field missing"
  ]
}
```

#### Status Codes

- `200 OK` - Import completed (check importedCount and failedCount in response)
- `400 Bad Request` - Invalid JSON format, empty request body, or no translations provided
- `404 Not Found` - TranslationsSet with the specified name or ID not found
- `401 Unauthorized` - Authentication required (if authentication is enabled)
- `500 Internal Server Error` - Server error occurred

---

## Authentication

Both endpoints require authentication if authentication is enabled in the API configuration. See [AUTHENTICATION.md](AUTHENTICATION.md) for details on:
- API Key authentication (X-API-Key header)
- JWT Bearer token authentication (Authorization header)

---

## Comparison with Generic Endpoints

### Traditional Query Endpoint
```
GET /api/query/GetTranslationsQuery?body={"pagination":{"skip":0,"pageSize":20},"ordering":{"orderBy":"resourceName"},"filtering":{"queryFilters":[{"filterType":"DataSetIdFilter","value":"7b8e9f10-1234-5678-90ab-cdef12345678"}]}}
```

### New TranslationsController Endpoint
```
GET /api/translations/MyDataSet?orderBy=resourceName&limit=20&offset=0
POST /api/translations/MyDataSet (with JSON array of translations)
```

The TranslationsController provides:
- ✅ Cleaner, more intuitive URLs
- ✅ Simple query parameters instead of JSON encoding
- ✅ Support for both TranslationsSet names and IDs
- ✅ RESTful design patterns
- ✅ Automatic TranslationsSet filtering
- ✅ Direct JSON import for remote data sources

---

## Tips and Best Practices

1. **Use TranslationsSet names for readability** - Names are easier to work with than GUIDs in URLs
2. **Pagination** - Use `limit` and `offset` for efficient pagination of large datasets
3. **Sorting** - Combine `orderBy` and `orderDirection` for custom sorting
4. **Offset calculation** - For page-based pagination: `offset = (pageNumber - 1) * limit`
5. **Batch imports** - Import multiple translations in a single request for efficiency
6. **Error handling** - Check `importedCount` and `failedCount` in the response; review `errors` array for details on failed imports
7. **Required fields** - Ensure each translation object has at minimum: `resourceName`, `translationName`, and `content`

---

## Error Handling

All endpoints return structured error responses:

```json
{
  "error": "TranslationsSet 'NonExistent' not found."
}
```

Common errors:
- TranslationsSet not found
- Invalid query parameters
- Invalid JSON format in import request
- Empty request body or no translations provided
- Required fields missing in translation objects
- Authentication failures

---

## See Also

- [POSTMAN_TESTING.md](POSTMAN_TESTING.md) - Testing with Postman
- [AUTHENTICATION.md](AUTHENTICATION.md) - Authentication setup
- [PAGINATION_QUICKSTART.md](PAGINATION_QUICKSTART.md) - Pagination details
