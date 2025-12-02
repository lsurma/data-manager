using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using Microsoft.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Emails
{
    public partial class EmailsPage : ComponentBase
    {
        private List<IQueryFilter> _filters = new();

        protected override void OnInitialized()
        {
            _filters.Add(new InternalGroupName1Filter { Value = "Email" });
        }
    }
}
