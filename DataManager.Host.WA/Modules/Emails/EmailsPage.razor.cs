using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DataManager.Application.Contracts;
using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Host.WA.Modules.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataManager.Host.WA.Modules.Emails
{
    public partial class EmailsPage : ComponentBase, IDisposable
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IRequestSender RequestSender { get; set; } = null!;

        private List<IQueryFilter> Filters { get; set; } = new();
        private TranslationsGrid? _translationsGrid;
        private IDialogReference? _currentDialog;
        private Guid? _selectedTranslationId;
        private string _refreshToken = Guid.NewGuid().ToString();

        protected override void OnInitialized()
        {
            Filters.Add(new InternalGroupName1Filter { Value = "Email" });
            NavigationManager.LocationChanged += OnLocationChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await ProcessUrlParametersAsync();
            }
        }

        private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            await ProcessUrlParametersAsync();
        }

        private async Task ProcessUrlParametersAsync()
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var action = query["action"];
            var idParam = query["id"];

            if (action == "create")
            {
                _selectedTranslationId = null;
                await OpenEmailEditorPanelAsync();
            }
            else if (!string.IsNullOrEmpty(idParam) && Guid.TryParse(idParam, out var translationId))
            {
                _selectedTranslationId = translationId;
                await OpenEmailEditorPanelAsync(translationId);
            }
            else
            {
                _selectedTranslationId = null;
                if (_currentDialog != null)
                {
                    await _currentDialog.CloseAsync();
                    _currentDialog = null;
                }
            }

            StateHasChanged();
        }

        private async Task OpenEmailEditorPanelAsync(Guid? translationId = null)
        {
            var parameters = new EmailEditorPanelParameters
            {
                TranslationId = translationId,
                OnDataChanged = () =>
                {
                    // Refresh the translations grid by changing the key
                    _refreshToken = Guid.NewGuid().ToString();
                    InvokeAsync(StateHasChanged);
                }
            };

            var newDialog = await DialogService.ShowPanelAsync<EmailEditorPanel>(parameters, new DialogParameters
            {
                Title = translationId.HasValue ? "Edit Email" : "Create New Email",
                Width = "100%",
                TrapFocus = false,
                Modal = false,
                Id = $"email-panel-{Guid.NewGuid()}"
            });

            if (_currentDialog != null)
            {
                await _currentDialog.CloseAsync();
            }

            _currentDialog = newDialog;

            var result = await _currentDialog.Result;
            _currentDialog = null;

            if (result.Cancelled)
            {
                NavigationManager.NavigateTo("/emails", false);
            }

            StateHasChanged();
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}
