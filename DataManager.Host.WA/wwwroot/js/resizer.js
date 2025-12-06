import interactjs from 'https://cdn.jsdelivr.net/npm/interactjs@1.10.27/+esm';

window.interactJs = {
    makeResizablePanel: function (dotNetHelper, panelElement) {
        if(!panelElement) {
            console.error('Panel element not specified');
            return;
        }
        
        if(typeof panelElement === 'string') {
            panelElement = document.querySelector(panelElement);
        }
        
        if(!panelElement) {
            console.error(`Element ${panelElement} not found`);
            return;
        }
        
        const handle = panelElement?.shadowRoot?.querySelector(".control");
     
        if(!handle) {
            console.error(`Element ${panelElement} does not contain a handle`);
            return;
        }
          
        this.makeResizable(dotNetHelper, handle, panelElement, '--dialog-width');
    },
    
    makeResizable: function (dotNetHelper, targetElement, propertyElement, cssVarNameToSet) {
        const computedStyle = getComputedStyle(targetElement);
        const minWidthVar = computedStyle.getPropertyValue('--min-width');
        const minWidth = parseInt(minWidthVar, 10) || 300;
        let debounceTimeout;

        interactjs(targetElement)
            .resizable({
                edges: { left: true, right: false, bottom: false, top: false },

                listeners: {
                    move: function (event) {
                        const width = event.rect.width;

                        if (propertyElement) {
                            propertyElement.style.setProperty(cssVarNameToSet, `${width}px`);
                        } else {
                            targetElement.style.width = `${width}px`;
                        }

                        clearTimeout(debounceTimeout);
                        debounceTimeout = setTimeout(() => {
                            if(dotNetHelper) {
                                dotNetHelper.invokeMethodAsync('SetWidth', width);
                            }
                        }, 250);
                    }
                },
                modifiers: [
                    interactjs.modifiers.restrictSize({
                        min: { width: minWidth }
                    })
                ],
                inertia: true
            });
    }
};

export default {};