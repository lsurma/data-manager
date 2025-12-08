# RemoveDuplicateTranslationsCommand

## Overview

The `RemoveDuplicateTranslationsCommand` is a command that removes duplicate translations from a specific dataset when they also exist in a base dataset. This is useful for cleaning up datasets that contain translations inherited from or duplicated from a parent/base dataset.

## Purpose

When a specific dataset contains translations that are identical (same TranslationKey, CultureName, and Content) to translations in a base dataset, those duplicates can be safely removed from the specific dataset. This helps:

- Reduce data redundancy
- Simplify dataset management
- Ensure translations are maintained in a single canonical location (the base dataset)
- Keep datasets lean by only storing unique or overridden translations

## How It Works

1. **Authorization Check**: Verifies that the user has access to both the specific and base datasets
2. **Batch Processing**: Processes translations from the specific dataset in batches of 250 for memory efficiency
3. **Duplicate Detection**: For each batch:
   - Fetches translations from the specific dataset
   - Queries the base dataset for translations with matching TranslationKey, CultureName, and Content
   - Identifies exact duplicates
4. **Deletion**: Removes identified duplicates from the specific dataset
5. **Result Tracking**: Returns statistics including processed count, removed count, and any errors

## API Usage

### Endpoint

```
POST /api/command/RemoveDuplicateTranslationsCommand
```

### Request Body

```json
{
  "SpecificDataSetId": "00000000-0000-0000-0000-000000000000",
  "BaseDataSetId": "00000000-0000-0000-0000-000000000000"
}
```

### Parameters

- **SpecificDataSetId** (required): The GUID of the dataset from which duplicates will be removed
- **BaseDataSetId** (required): The GUID of the base dataset containing the canonical translations

### Response

```json
{
  "removedCount": 42,
  "processedCount": 250,
  "errors": []
}
```

- **removedCount**: Number of duplicate translations that were deleted
- **processedCount**: Total number of translations processed from the specific dataset
- **errors**: Array of error messages (empty if successful)

## Example Scenario

You have two datasets:

1. **common-translations** (Base Dataset): Contains standard translations used across multiple applications
2. **app-specific-translations** (Specific Dataset): Contains translations for a specific application

Initially, app-specific-translations contains:
- Some unique translations specific to the app
- Some translations copied from common-translations

After running RemoveDuplicateTranslationsCommand:
- Unique translations remain in app-specific-translations
- Duplicate translations (identical to those in common-translations) are removed
- The app still works correctly because the base dataset provides the removed translations

## Technical Details

### Batch Processing

The command processes translations in batches of 250 to balance:
- Memory efficiency (avoids loading all translations at once)
- Database query performance (reasonable batch size for IN clauses)

### Duplicate Detection Criteria

Two translations are considered duplicates if ALL of the following match:
- **TranslationKey**: Built from ResourceName and TranslationName
- **CultureName**: The culture/language code (e.g., "en-US", "pl-PL")
- **Content**: The actual translation text

### Authorization

The command enforces authorization at two levels:
1. Dataset access verification using `IAuthorizationService.CanAccessDataSetAsync`
2. Query-level filtering using `TranslationsQueryService.PrepareQueryAsync`

Users can only remove duplicates from datasets they have access to.

### Performance Considerations

- Uses EF Core's `RemoveRange` for efficient batch deletions
- Commits changes after each batch with `SaveChangesAsync(acceptAllChangesOnSuccess: true)`
- When duplicates are deleted, the next batch starts at the same position (items shift)
- When no duplicates are found, skip is incremented to process the next batch

### Logging

The handler logs:
- Information about duplicates removed in each batch
- Total statistics upon completion
- Errors encountered during processing

## Error Handling

The command handles errors gracefully:
- Dataset not found or not accessible
- Authorization failures
- Database errors during processing
- Unexpected exceptions

All errors are captured and returned in the `errors` array of the result.

## Implementation Files

- **Command**: `DataManager.Application.Contracts/Modules/Translations/RemoveDuplicateTranslationsCommand.cs`
- **Result DTO**: `DataManager.Application.Contracts/Modules/Translations/RemoveDuplicateTranslationsResult.cs`
- **Handler**: `DataManager.Application.Core/Modules/Translations/Handlers/RemoveDuplicateTranslationsCommandHandler.cs`

## See Also

- Dataset hierarchy and includes documentation
- Translation materialization features
- Authorization system documentation
