// Quill editor wrapper
const quillInstances = {};

// Helper function to wait for Quill to be available
async function waitForQuill(maxRetries = 10, delayMs = 100) {
    for (let i = 0; i < maxRetries; i++) {
        if (typeof Quill !== 'undefined') {
            return true;
        }
        await new Promise(resolve => setTimeout(resolve, delayMs));
    }
    return false;
}

export async function initializeQuill(editorId, dotNetRef, placeholder, height, initialValue) {
    const container = document.getElementById(editorId);
    if (!container) {
        console.error(`Quill container ${editorId} not found`);
        return;
    }

    // Wait for Quill to be available
    const quillAvailable = await waitForQuill();
    if (!quillAvailable) {
        console.error('Quill library is not available after retries');
        return;
    }

    // Create editor div
    const editorDiv = document.createElement('div');
    editorDiv.style.height = height;
    container.appendChild(editorDiv);

    // Initialize Quill
    const quill = new Quill(editorDiv, {
        theme: 'snow',
        placeholder: placeholder,
        modules: {
            toolbar: [
                [{ 'header': [1, 2, 3, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                [{ 'color': [] }, { 'background': [] }],
                [{ 'align': [] }],
                ['link', 'image'],
                ['clean']
            ]
        }
    });

    // Store instance
    quillInstances[editorId] = {
        quill: quill,
        dotNetRef: dotNetRef
    };

    // Set initial value if provided
    if (initialValue && initialValue.trim() !== '') {
        console.log('Setting initial value for Quill editor', initialValue);
        quill.clipboard.dangerouslyPasteHTML(0, initialValue);
    }

    // Listen for changes
    quill.on('text-change', () => {
        const html = quill.root.innerHTML;
        dotNetRef.invokeMethodAsync('OnContentChanged', html);
    });
}

export function getQuillContent(editorId) {
    const instance = quillInstances[editorId];
    if (instance && instance.quill) {
        return instance.quill.root.innerHTML;
    }
    return '';
}

export function setQuillContent(editorId, html) {
    const instance = quillInstances[editorId];
    if (instance && instance.quill) {
        const delta = instance.quill.clipboard.convert(html);
        instance.quill.setContents(delta, 'silent');
    }
}

export function destroyQuill(editorId) {
    const instance = quillInstances[editorId];
    if (instance) {
        delete quillInstances[editorId];
    }
}
