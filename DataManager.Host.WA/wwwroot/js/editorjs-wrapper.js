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
        tools: {
            header: {
                class: Header,
                inlineToolbar: true
            },
            list: {
                class: List,
                inlineToolbar: true
            },
            quote: {
                class: Quote,
                inlineToolbar: true
            },
            delimiter: Delimiter,
            table: {
                class: Table,
                inlineToolbar: true
            },
            code: {
                class: CodeTool
            },
            warning: {
                class: Warning,
                inlineToolbar: true
            },
            marker: {
                class: Marker,
                shortcut: 'CMD+SHIFT+M'
            },
            inlineCode: {
                class: InlineCode,
                shortcut: 'CMD+SHIFT+C'
            }
        }
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
