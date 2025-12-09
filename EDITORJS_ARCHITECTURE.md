# EditorJS Component Architecture

## Component Hierarchy

```
TranslationPanel.razor
    └── ContentEditor.razor
            ├── Button Group (Editor Type Selection)
            │   ├── Text Input
            │   ├── Text Area
            │   ├── Code Editor (Monaco)
            │   ├── Rich Text (Radzen)
            │   ├── Quill Editor
            │   └── EditorJS ← NEW
            └── ContentEditorRow.razor (for each item)
                    └── [Switch on EditorType]
                        ├── FluentTextField
                        ├── FluentTextArea
                        ├── StandaloneCodeEditor
                        ├── RadzenHtmlEditor
                        ├── QuillEditor
                        └── EditorJSEditor ← NEW
```

## Data Flow

```
User Edits Content
    ↓
EditorJS JavaScript Library
    ↓
editorjs-wrapper.js
    ↓ [JS Interop]
EditorJSEditor.razor
    ↓ [OnContentChanged callback]
ContentEditorRow.razor
    ↓ [HandleEditorJSChanged]
ContentEditorItem.Content (JSON string)
    ↓ [OnContentChanged event]
Parent Component (TranslationPanel)
```

## File Structure

```
DataManager.Host.WA/
├── Components/
│   ├── EditorJSEditor.razor ← NEW Blazor component
│   └── ContentEditor/
│       ├── ContentEditor.razor (Modified: added label)
│       ├── ContentEditor.razor.cs
│       ├── ContentEditorRow.razor (Modified: added EditorJS case)
│       ├── ContentEditorType.cs (Modified: added EditorJS enum)
│       └── ContentEditorItem.cs
└── wwwroot/
    ├── index.html (Modified: added CDN references)
    └── js/
        └── editorjs-wrapper.js ← NEW JavaScript interop
```

## JavaScript Interop Pattern

```javascript
// Blazor → JavaScript
await module.InvokeVoidAsync("initializeEditorJS", ...)
await module.InvokeAsync<string>("getEditorJSContent", ...)
await module.InvokeVoidAsync("setEditorJSContent", ...)
await module.InvokeVoidAsync("destroyEditorJS", ...)

// JavaScript → Blazor
dotNetRef.invokeMethodAsync('OnContentChanged', jsonString)
```

## EditorJS Tools Configuration

```javascript
tools: {
    header: Header,      // H1, H2, H3 headings
    list: List,          // Ordered/unordered lists
    quote: Quote,        // Block quotes
    delimiter: Delimiter, // Section separators
    table: Table,        // Data tables
    code: CodeTool,      // Code blocks
    warning: Warning,    // Warning blocks
    marker: Marker,      // Text highlighting
    inlineCode: InlineCode, // Inline code
    image: SimpleImage   // Simple image blocks
}
```

## CDN Dependencies

```html
<!-- Core -->
@editorjs/editorjs@2.30.7

<!-- Tools (10 plugins) -->
@editorjs/header@2.8.8
@editorjs/list@1.10.0
@editorjs/quote@2.7.4
@editorjs/delimiter@1.4.2
@editorjs/table@2.4.1
@editorjs/code@2.9.3
@editorjs/warning@1.4.0
@editorjs/marker@1.4.0
@editorjs/inline-code@1.5.1
@editorjs/simple-image@1.6.0
```

## Example EditorJS JSON Output

```json
{
  "time": 1702136073827,
  "blocks": [
    {
      "id": "abc123",
      "type": "header",
      "data": {
        "text": "Welcome to EditorJS",
        "level": 2
      }
    },
    {
      "id": "def456",
      "type": "paragraph",
      "data": {
        "text": "This is a paragraph with <mark>highlighted</mark> text."
      }
    },
    {
      "id": "ghi789",
      "type": "list",
      "data": {
        "style": "unordered",
        "items": [
          "First item",
          "Second item",
          "Third item"
        ]
      }
    }
  ],
  "version": "2.30.7"
}
```

## Comparison with Other Editors

| Feature | EditorJS | QuillEditor | Monaco | Radzen |
|---------|----------|-------------|--------|--------|
| Output Format | JSON | HTML | Text | HTML |
| Structure | Block-based | WYSIWYG | Plain text | WYSIWYG |
| Extensibility | High | Medium | Low | Low |
| Data Parsing | Easy | Medium | N/A | Medium |
| Use Case | Structured content | Rich text | Code | Rich text |

## Styling Classes

```css
.editorjs-wrapper { /* Wrapper container */ }
.codex-editor { /* Editor main container */ }
.ce-block__content { /* Block content */ }
.ce-toolbar__content { /* Toolbar content */ }
.codex-editor__redactor { /* Editor area */ }
```
