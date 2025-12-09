# EditorJS Integration - Quick Start Guide

## What Was Added

EditorJS has been added as a **6th content editor option** in the ContentEditor component, alongside:
1. Text Input
2. Text Area  
3. Monaco Editor (Code Editor)
4. Radzen Rich Text Editor
5. Quill Editor
6. **EditorJS** ← NEW

## Where to Find It

The EditorJS editor is available in the **Translation Panel** when editing translations:
- Navigate to a translation
- Open the translation panel
- Look for the editor type buttons at the top
- Click **"EditorJS"** button to activate

## How It Works

### For Users:
1. Click the "EditorJS" button to switch to this editor
2. Click "+" or press Tab to add blocks
3. Choose block type: Header, Paragraph, List, Quote, etc.
4. Use inline formatting: Bold, Italic, Marker, Code
5. Drag blocks to reorder
6. Content saves automatically as JSON

### For Developers:
```csharp
// Data is stored as JSON string in ContentEditorItem.Content
var content = item.Content; // Returns JSON string

// Example JSON structure:
{
  "blocks": [
    {
      "type": "header",
      "data": { "text": "Title", "level": 2 }
    },
    {
      "type": "paragraph", 
      "data": { "text": "Content..." }
    }
  ]
}
```

## Available Tools

- 📝 **Header**: H1, H2, H3 headings
- ¶ **Paragraph**: Standard text blocks
- • **List**: Ordered/unordered lists  
- " **Quote**: Block quotations
- ━ **Delimiter**: Visual separators
- ═ **Table**: Data tables
- </> **Code**: Code blocks with syntax
- ⚠ **Warning**: Alert/warning blocks
- 🖍 **Marker**: Text highlighting
- `code` **Inline Code**: Inline code formatting

## Key Features

✅ **Clean Data**: Outputs structured JSON instead of HTML  
✅ **Block-Based**: Each content piece is a separate block  
✅ **Extensible**: Easy to add custom block types  
✅ **Modern UI**: Clean, minimalist interface  
✅ **Mobile-Friendly**: Touch-optimized controls  
✅ **Keyboard Shortcuts**: Efficient editing workflow

## Technical Details

### Dependencies
All dependencies are loaded from CDN (jsdelivr.net):
- EditorJS Core: v2.30.7
- 9 Tool Plugins: All pinned to specific versions

### Files Created
1. `DataManager.Host.WA/Components/EditorJSEditor.razor`
2. `DataManager.Host.WA/wwwroot/js/editorjs-wrapper.js`

### Files Modified
1. `ContentEditorType.cs` - Added enum value
2. `ContentEditor.razor` - Added label
3. `ContentEditorRow.razor` - Added rendering logic
4. `index.html` - Added CDN references

### JavaScript Interop
```javascript
// Module-based ES6 imports
import { initializeEditorJS, getEditorJSContent, 
         setEditorJSContent, destroyEditorJS } 
from './js/editorjs-wrapper.js';
```

## Advantages Over Other Editors

| Feature | EditorJS | Quill | Radzen | Monaco |
|---------|----------|-------|--------|--------|
| **Output** | Clean JSON | HTML | HTML | Plain Text |
| **Structure** | Block-based | WYSIWYG | WYSIWYG | Code |
| **Parsing** | ✅ Easy | Medium | Medium | N/A |
| **Extensible** | ✅ Yes | Some | Limited | No |
| **Data Quality** | ✅ High | Medium | Medium | High |
| **Best For** | Structured content | Rich text | Rich text | Code |

## Use Cases

**Perfect for:**
- Structured articles and documents
- Multi-language content with consistent formatting
- Content that needs to be transformed/parsed
- Modern, mobile-friendly editing experience

**Less ideal for:**
- Simple single-line inputs → Use Text Input
- Raw HTML editing → Use Monaco Editor
- Traditional WYSIWYG → Use Quill or Radzen

## Data Migration

### Converting from HTML to EditorJS:
```javascript
// Simple paragraph conversion
const htmlContent = "<p>Hello World</p>";
const editorjsData = {
  blocks: [
    {
      type: "paragraph",
      data: { text: "Hello World" }
    }
  ]
};
```

### Converting from EditorJS to HTML:
Use EditorJS Parser library or custom renderer.

## Troubleshooting

### Issue: Editor doesn't load
- Check browser console for CDN errors
- Verify internet connection (CDN access required)
- Check if EditorJS scripts loaded in index.html

### Issue: Tools missing
- Some tools may fail to load from CDN
- Editor gracefully degrades - shows available tools only
- Check network tab in DevTools

### Issue: Content not saving
- Verify OnContentChanged callback is wired
- Check JSON format is valid
- Look for JavaScript errors in console

## Performance Considerations

- **Initial Load**: ~200KB for all tools (gzipped)
- **Runtime**: Minimal overhead, event-driven
- **Memory**: One instance per editor field
- **CDN**: Fast loading from jsdelivr.net CDN

## Browser Support

EditorJS supports all modern browsers:
- ✅ Chrome/Edge (Chromium)
- ✅ Firefox
- ✅ Safari
- ✅ Mobile browsers (iOS/Android)

## Future Enhancements

Potential additions:
1. Custom block types for translation-specific needs
2. Image upload support
3. Embed blocks (video, audio)
4. Collaboration features
5. Custom theme matching app design
6. Export to PDF/Word
7. Content templates

## Documentation

Comprehensive documentation available:
1. `EDITORJS_INTEGRATION.md` - Implementation details
2. `EDITORJS_ARCHITECTURE.md` - Architecture diagrams
3. `EDITORJS_UI_MOCKUP.md` - UI mockups and examples
4. This file - Quick start guide

## Official Resources

- EditorJS Website: https://editorjs.io/
- Getting Started: https://editorjs.io/getting-started/
- GitHub: https://github.com/codex-team/editor.js
- Documentation: https://editorjs.io/docs/

## Support

For issues or questions:
1. Check browser console for errors
2. Review documentation files
3. Check EditorJS official docs
4. Verify CDN accessibility

## Changelog

### v1.0.0 (December 2024)
- ✅ Initial implementation
- ✅ 9 EditorJS tools included
- ✅ Full Blazor/JS interop
- ✅ Version pinning for stability
- ✅ Graceful degradation
- ✅ Comprehensive documentation
