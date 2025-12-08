namespace DataManager.Host.WA.Services;

public interface IKeyboardShortcutsService
{
    /// <summary>
    /// Registers a Ctrl+S (or Cmd+S on Mac) keyboard shortcut that calls the provided callback.
    /// </summary>
    /// <param name="onSave">The callback to invoke when the save shortcut is triggered.</param>
    /// <returns>A task that completes when the registration is complete.</returns>
    Task RegisterSaveShortcutAsync(Func<Task> onSave);

    /// <summary>
    /// Unregisters the keyboard shortcut handler for the current component.
    /// </summary>
    /// <returns>A task that completes when the unregistration is complete.</returns>
    Task UnregisterAsync();
}
