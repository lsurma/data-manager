window.keyboardShortcuts = {
    listeners: new Map(),

    registerSaveShortcut: function (dotNetHelper, componentId) {
        if (!dotNetHelper || !componentId) {
            console.error('dotNetHelper and componentId are required');
            return;
        }

        const handler = (event) => {
            // Check for Ctrl+S (Windows/Linux) or Cmd+S (Mac)
            if ((event.ctrlKey || event.metaKey) && event.key === 's') {
                event.preventDefault();
                event.stopPropagation();

                // Invoke the .NET method
                dotNetHelper.invokeMethodAsync('OnSaveShortcut')
                    .catch(err => console.error('Error invoking save shortcut:', err));
            }
        };

        // Store the handler so we can remove it later
        this.listeners.set(componentId, handler);

        // Add the event listener
        document.addEventListener('keydown', handler);

        console.log(`Save shortcut registered for component: ${componentId}`);
    },

    unregisterSaveShortcut: function (componentId) {
        if (!componentId) {
            console.error('componentId is required');
            return;
        }

        const handler = this.listeners.get(componentId);
        if (handler) {
            document.removeEventListener('keydown', handler);
            this.listeners.delete(componentId);
            console.log(`Save shortcut unregistered for component: ${componentId}`);
        }
    }
};

export default {};
