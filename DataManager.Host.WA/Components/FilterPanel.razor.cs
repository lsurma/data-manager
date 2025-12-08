using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Components;

public partial class FilterPanel : IDialogContentComponent<FilterPanelParameters>
{
    [Parameter]
    public FilterPanelParameters Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    private bool IsSaving { get; set; }

    private async Task HandleFilterOrderUpdate(FluentSortableListEventArgs args)
    {
        var movedItem = Content.Filters[args.OldIndex];
        Content.Filters.RemoveAt(args.OldIndex);
        Content.Filters.Insert(args.NewIndex, movedItem);

        for (int i = 0; i < Content.Filters.Count; i++)
        {
            Content.Filters[i].OrderIndex = i;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleSaveAsync()
    {
        try
        {
            IsSaving = true;

            await Dialog!.CloseAsync(DialogResult.Ok(Content.Filters));
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

public class FilterPanelParameters
{
    public List<FilterSettings> Filters { get; set; } = new();
}
