# Translation Filter Improvements - Testing Guide

## Overview
This guide describes how to test the new translation filter improvements implemented in this PR.

## Features to Test

### 1. Culture Dropdown Filter

#### Test Case 1.1: Filter Visibility
**Steps:**
1. Navigate to the Translations page
2. Select a TranslationsSet from the top buttons
3. Observe the filter area above the data grid

**Expected Result:**
- A dropdown labeled with cultures should be visible (if filter is enabled in settings)
- The dropdown should show "All Cultures" as the first option
- The dropdown should list all available cultures from the backend

#### Test Case 1.2: Filter Functionality
**Steps:**
1. Select a specific culture from the dropdown (e.g., "en-US")
2. Observe the translations grid

**Expected Result:**
- Grid should refresh showing only translations for the selected culture
- Page should reset to first page
- Selection should persist during the session

#### Test Case 1.3: Clear Filter
**Steps:**
1. Select a culture filter
2. Select "All Cultures" from the dropdown

**Expected Result:**
- Grid should refresh showing all translations
- Filter should be cleared

### 2. Filter Settings Panel

#### Test Case 2.1: Open Filter Panel
**Steps:**
1. Navigate to the Translations page
2. Click the filter icon button in the toolbar (next to the search box)

**Expected Result:**
- A right-side panel should slide in
- Panel title should be "Filter Settings"
- Panel should show a list of available filters (currently just "Culture")
- Each filter should have a checkbox and label

#### Test Case 2.2: Show/Hide Filters
**Steps:**
1. Open the Filter Settings panel
2. Uncheck the "Culture" checkbox
3. Click "Save"
4. Observe the filters area

**Expected Result:**
- Panel should close
- Culture dropdown should no longer be visible in the filters area

**Steps to Restore:**
1. Open the Filter Settings panel again
2. Check the "Culture" checkbox
3. Click "Save"

**Expected Result:**
- Culture dropdown should be visible again

#### Test Case 2.3: Reorder Filters (Future Enhancement)
**Steps:**
1. Open the Filter Settings panel
2. Drag and drop a filter to a different position
3. Click "Save"

**Expected Result:**
- Panel should close
- Filters should appear in the new order

**Note:** Currently only one filter exists, so this test is for future reference.

#### Test Case 2.4: Cancel Changes
**Steps:**
1. Open the Filter Settings panel
2. Uncheck a filter
3. Click "Close" (not "Save")
4. Reopen the Filter Settings panel

**Expected Result:**
- Changes should not be saved
- Filter should still be checked

#### Test Case 2.5: Settings Persistence
**Steps:**
1. Open the Filter Settings panel
2. Uncheck the "Culture" filter
3. Click "Save"
4. Refresh the page (F5)
5. Navigate back to the Translations page

**Expected Result:**
- Culture filter should remain hidden
- Settings should persist across page refreshes

### 3. Integration Tests

#### Test Case 3.1: Culture Loading
**Steps:**
1. Open browser developer console (F12)
2. Navigate to the Translations page
3. Check the Network tab for API calls

**Expected Result:**
- Should see a call to `GetAvailableCulturesQuery`
- Should receive a list of cultures
- No errors in console

#### Test Case 3.2: Error Handling
**Steps:**
1. Open browser developer console (F12)
2. Navigate to the Translations page
3. Observe console logs

**Expected Result:**
- If there are errors loading cultures or settings, they should be logged properly
- Application should not crash
- Default settings should be used if loading fails

#### Test Case 3.3: Multiple TranslationsSets
**Steps:**
1. Navigate to the Translations page
2. Select different TranslationsSets using the top buttons
3. Observe culture filter behavior

**Expected Result:**
- Culture filter should work for all TranslationsSets
- Filter settings should persist across TranslationsSet changes

## Manual Testing Checklist

- [ ] Culture dropdown displays correctly
- [ ] Culture dropdown loads available cultures from backend
- [ ] Selecting a culture filters the translations
- [ ] "All Cultures" option clears the filter
- [ ] Filter settings button is visible in toolbar
- [ ] Filter panel opens and closes correctly
- [ ] Show/hide filter checkbox works
- [ ] Save button persists changes
- [ ] Close button cancels changes
- [ ] Settings persist across page refreshes
- [ ] Settings persist across browser sessions
- [ ] No console errors
- [ ] No visual regressions in the UI
- [ ] Responsive design works on different screen sizes
- [ ] Filter works with all TranslationsSets

## Browser Compatibility

Test in the following browsers:
- [ ] Chrome/Edge (Chromium)
- [ ] Firefox
- [ ] Safari (if available)

## Performance Testing

- [ ] Filter dropdown responds quickly
- [ ] Filter panel opens/closes smoothly
- [ ] Grid refresh happens promptly after filter selection
- [ ] No noticeable lag when toggling filter visibility

## Local Storage Inspection

To inspect the saved settings:
1. Open browser developer console (F12)
2. Go to Application tab (Chrome) or Storage tab (Firefox)
3. Expand Local Storage
4. Look for key: `translations-filter-settings`

**Expected Value:**
```json
{
  "Filters": [
    {
      "Id": "culture",
      "Label": "Culture",
      "Visible": true,
      "OrderIndex": 0
    }
  ]
}
```

## Known Limitations

1. Azure Functions tooling not available in CI environment - API testing requires local setup
2. Only one filter (Culture) currently implemented - more filters can be added following the same pattern
3. Culture dropdown styling may vary slightly between browsers due to FluentUI implementation

## Troubleshooting

### Issue: Culture dropdown is empty
**Solution:**
- Check that `GetAvailableCulturesQuery` handler is working correctly
- Verify backend API is running
- Check browser console for errors

### Issue: Filter settings not persisting
**Solution:**
- Check that local storage is enabled in the browser
- Clear browser cache and try again
- Check browser console for localStorage errors

### Issue: Filter panel not opening
**Solution:**
- Check browser console for JavaScript errors
- Verify FluentUI dialog service is properly configured
- Check that DialogProvider is in the layout

## Next Steps

After manual testing is complete:
1. Deploy to test environment
2. Test with real data
3. Gather user feedback
4. Add additional filters as needed (resource name, translation name, etc.)
