using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace InstanceManager.Host.WA.Components;

public partial class DataGridSettingsPanel : IDialogContentComponent<DataGridSettingsPanelParameters>
{
    [Parameter]
    public DataGridSettingsPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    private bool IsSaving { get; set; }

    private async Task HandleColumnOrderUpdate(FluentSortableListEventArgs args)
    {
        var movedItem = Content.Columns[args.OldIndex];
        Content.Columns.RemoveAt(args.OldIndex);
        Content.Columns.Insert(args.NewIndex, movedItem);

        for (int i = 0; i < Content.Columns.Count; i++)
        {
            Content.Columns[i].Order = i;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleSaveAsync()
    {
        try
        {
            IsSaving = true;

            if (Content.OnSettingsChanged != null)
            {
                await Content.OnSettingsChanged.Invoke();
            }

            await Dialog!.CloseAsync(DialogResult.Ok(Content.Columns));
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task HandleCancelAsync()
    {
        await Dialog!.CancelAsync();
    }
}

public class DataGridSettingsPanelParameters
{
    public List<ColumnState> Columns { get; set; } = new();
    public Func<Task>? OnSettingsChanged { get; set; }
}
