# TranslationsController API Guide

This guide explains how to use the new TranslationsController endpoints for easier access to translation data.

## Overview

The TranslationsController provides simplified REST endpoints for working with translations, offering a more intuitive alternative to the generic query/command endpoints.

## Endpoints

### 1. GET Translations

**Endpoint:** `GET /api/translations/{dataSetNameOrId}`

Retrieves translations for a specific dataset with optional filtering, pagination, and sorting.

#### Route Parameters

- `dataSetNameOrId` - The DataSet identifier (can be either the DataSet name or GUID)

#### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `orderBy` | string | - | Field name to sort by (e.g., "resourceName", "cultureName", "createdAt") |
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

**Get translations sorted by creation date (newest first):**
```
GET /api/translations/MyDataSet?orderBy=createdAt&orderDirection=desc
```

**Get translations by DataSet ID:**
```
GET /api/translations/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Complex query with all parameters:**
```
GET /api/translations/MyDataSet?orderBy=resourceName&orderDirection=asc&limit=25&offset=50
```

#### Response Format

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "internalGroupName1": "Group1",
      "internalGroupName2": "Group2",
      "resourceName": "MyResource",
      "translationName": "MyTranslation",
      "cultureName": "en-US",
      "content": "Hello World",
      "contentTemplate": null,
      "dataSetId": "7b8e9f10-1234-5678-90ab-cdef12345678",
      "layoutId": null,
      "sourceId": null,
      "createdAt": "2025-12-02T10:00:00Z",
      "updatedAt": "2025-12-02T11:00:00Z",
      "createdBy": "user@example.com"
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

#### Status Codes

- `200 OK` - Success, returns paginated list of translations
- `404 Not Found` - DataSet with the specified name or ID not found
- `401 Unauthorized` - Authentication required (if authentication is enabled)
- `500 Internal Server Error` - Server error occurred

---

### 2. POST Import Translations

**Endpoint:** `POST /api/translations/{dataSetNameOrId}`

Imports translations from a file (e.g., CSV, JSON, XLSX) for a specific dataset.

#### Route Parameters

- `dataSetNameOrId` - The DataSet identifier (can be either the DataSet name or GUID)

#### Request Format

This endpoint expects a multipart/form-data request with a file upload.

**Form Data Fields:**
- `file` (required) - The translation file to import

#### Examples

**Using curl:**
```bash
curl -X POST http://localhost:7233/api/translations/MyDataSet \
  -F "file=@translations.csv"
```

**Using curl with DataSet ID:**
```bash
curl -X POST http://localhost:7233/api/translations/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -F "file=@translations.xlsx"
```

**Using PowerShell:**
```powershell
$uri = "http://localhost:7233/api/translations/MyDataSet"
$filePath = "C:\path\to\translations.csv"

$form = @{
    file = Get-Item -Path $filePath
}

Invoke-RestMethod -Uri $uri -Method Post -Form $form
```

**Using Postman:**
1. Set method to POST
2. Set URL to `/api/translations/MyDataSet`
3. Go to "Body" tab
4. Select "form-data"
5. Add key "file" with type "File"
6. Choose your translation file
7. Click "Send"

#### Response Format

**Success:**
```json
{
  "message": "Translations imported successfully.",
  "dataSetId": "7b8e9f10-1234-5678-90ab-cdef12345678"
}
```

#### Status Codes

- `200 OK` - File uploaded and queued for processing
- `400 Bad Request` - No file provided or invalid request
- `404 Not Found` - DataSet with the specified name or ID not found
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
```

The TranslationsController provides:
- ✅ Cleaner, more intuitive URLs
- ✅ Simple query parameters instead of JSON encoding
- ✅ Support for both DataSet names and IDs
- ✅ RESTful design patterns
- ✅ Automatic DataSet filtering

---

## Tips and Best Practices

1. **Use DataSet names for readability** - Names are easier to work with than GUIDs in URLs
2. **Pagination** - Use `limit` and `offset` for efficient pagination of large datasets
3. **Sorting** - Combine `orderBy` and `orderDirection` for custom sorting
4. **Offset calculation** - For page-based pagination: `offset = (pageNumber - 1) * limit`
5. **Import processing** - The POST endpoint saves the file for asynchronous processing; check logs for processing status

---

## Error Handling

All endpoints return structured error responses:

```json
{
  "error": "DataSet 'NonExistent' not found."
}
```

Common errors:
- DataSet not found
- Invalid query parameters
- Missing file in import request
- Authentication failures

---

## See Also

- [POSTMAN_TESTING.md](POSTMAN_TESTING.md) - Testing with Postman
- [AUTHENTICATION.md](AUTHENTICATION.md) - Authentication setup
- [PAGINATION_QUICKSTART.md](PAGINATION_QUICKSTART.md) - Pagination details
