using Microsoft.JSInterop;

namespace DataManager.Host.WA.Services;

public class KeyboardShortcutsService : IKeyboardShortcutsService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _componentId;
    private DotNetObjectReference<KeyboardShortcutsCallbackHandler>? _dotNetHelper;
    private KeyboardShortcutsCallbackHandler? _callbackHandler;

    public KeyboardShortcutsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _componentId = Guid.NewGuid().ToString();
    }

    public async Task RegisterSaveShortcutAsync(Func<Task> onSave)
    {
        // Create callback handler
        _callbackHandler = new KeyboardShortcutsCallbackHandler(onSave);
        _dotNetHelper = DotNetObjectReference.Create(_callbackHandler);

        // Register with JavaScript
        await _jsRuntime.InvokeVoidAsync("keyboardShortcuts.registerSaveShortcut", _dotNetHelper, _componentId);
    }

    public async Task UnregisterAsync()
    {
        if (_dotNetHelper != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("keyboardShortcuts.unregisterSaveShortcut", _componentId);
            }
            catch
            {
                // Ignore errors during unregistration
            }

            _dotNetHelper.Dispose();
            _dotNetHelper = null;
            _callbackHandler = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await UnregisterAsync();
    }

    private class KeyboardShortcutsCallbackHandler
    {
        private readonly Func<Task> _onSave;
        private bool _isSaving;

        public KeyboardShortcutsCallbackHandler(Func<Task> onSave)
        {
            _onSave = onSave;
        }

        [JSInvokable]
        public async Task OnSaveShortcut()
        {
            if (!_isSaving)
            {
                try
                {
                    _isSaving = true;
                    await _onSave();
                }
                finally
                {
                    _isSaving = false;
                }
            }
        }
    }
}
