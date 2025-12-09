// EditorJS wrapper
const editorJSInstances = {};

// Helper function to wait for EditorJS to be available
async function waitForEditorJS(maxRetries = 10, delayMs = 100) {
    for (let i = 0; i < maxRetries; i++) {
        if (typeof EditorJS !== 'undefined') {
            return true;
        }
        await new Promise(resolve => setTimeout(resolve, delayMs));
    }
    return false;
}

export async function initializeEditorJS(editorId, dotNetRef, placeholder, initialValue) {
    const container = document.getElementById(editorId);
    if (!container) {
        console.error(`EditorJS container ${editorId} not found`);
        return;
    }

    // Wait for EditorJS to be available
    const editorJSAvailable = await waitForEditorJS();
    if (!editorJSAvailable) {
        console.error('EditorJS library is not available after retries');
        return;
    }

    // Parse initial value if it's JSON, otherwise treat as plain text
    let initialData;
    if (initialValue && initialValue.trim() !== '') {
        try {
            initialData = JSON.parse(initialValue);
        } catch (e) {
            // If not valid JSON, create a simple paragraph block
            initialData = {
                blocks: [
                    {
                        type: "paragraph",
                        data: {
                            text: initialValue
                        }
                    }
                ]
            };
        }
    }

    // Initialize EditorJS
    const tools = {};
    
    // Add tools only if they are available
    if (typeof Header !== 'undefined') {
        tools.header = {
            class: Header,
            inlineToolbar: true
        };
    }
    if (typeof List !== 'undefined') {
        tools.list = {
            class: List,
            inlineToolbar: true
        };
    }
    if (typeof Quote !== 'undefined') {
        tools.quote = {
            class: Quote,
            inlineToolbar: true
        };
    }
    if (typeof Delimiter !== 'undefined') {
        tools.delimiter = Delimiter;
    }
    if (typeof Table !== 'undefined') {
        tools.table = {
            class: Table,
            inlineToolbar: true
        };
    }
    if (typeof CodeTool !== 'undefined') {
        tools.code = {
            class: CodeTool
        };
    }
    if (typeof Warning !== 'undefined') {
        tools.warning = {
            class: Warning,
            inlineToolbar: true
        };
    }
    if (typeof Marker !== 'undefined') {
        tools.marker = {
            class: Marker,
            shortcut: 'CMD+SHIFT+M'
        };
    }
    if (typeof InlineCode !== 'undefined') {
        tools.inlineCode = {
            class: InlineCode,
            shortcut: 'CMD+SHIFT+C'
        };
    }

    const editor = new EditorJS({
        holder: editorId,
        placeholder: placeholder || 'Start writing...',
        data: initialData,
        onChange: async (api, event) => {
            try {
                const outputData = await editor.save();
                const jsonString = JSON.stringify(outputData);
                dotNetRef.invokeMethodAsync('OnContentChanged', jsonString);
            } catch (error) {
                console.error('EditorJS save error:', error);
            }
        },
        tools: tools
    });

    // Store instance
    editorJSInstances[editorId] = {
        editor: editor,
        dotNetRef: dotNetRef
    };

    // Wait for editor to be ready
    await editor.isReady;
}

export async function getEditorJSContent(editorId) {
    const instance = editorJSInstances[editorId];
    if (instance && instance.editor) {
        try {
            const outputData = await instance.editor.save();
            return JSON.stringify(outputData);
        } catch (error) {
            console.error('EditorJS get content error:', error);
            return '';
        }
    }
    return '';
}

export async function setEditorJSContent(editorId, jsonString) {
    const instance = editorJSInstances[editorId];
    if (instance && instance.editor) {
        try {
            const data = JSON.parse(jsonString);
            await instance.editor.render(data);
        } catch (error) {
            console.error('EditorJS set content error:', error);
        }
    }
}

export async function destroyEditorJS(editorId) {
    const instance = editorJSInstances[editorId];
    if (instance) {
        try {
            if (instance.editor && typeof instance.editor.destroy === 'function') {
                await instance.editor.destroy();
            }
        } catch (error) {
            console.error('EditorJS destroy error:', error);
        }
        delete editorJSInstances[editorId];
    }
}
