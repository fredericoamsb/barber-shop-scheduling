﻿using Prism.Commands;
using Prism.Navigation;
using Plugin.Connectivity;
using Prism.Services;
using System.Threading.Tasks;
using PrismBarbearia.Services;
using PrismBarbearia.Helpers;
using System.Diagnostics;
using System;
using System.Linq;

namespace PrismBarbearia.ViewModels
{
    public class MenuPageViewModel : BaseViewModel, INavigatedAware
    {
        private string privilegio;
        public string Privilegio
        {
            get { return privilegio; }
            set { SetProperty(ref privilegio, value); }
        }

        private bool isVisibleLogInButton;
        public bool IsVisibleLogInButton
        {
            get { return isVisibleLogInButton; }
            set { SetProperty(ref isVisibleLogInButton, value); }
        }

        private bool isVisibleLogOutButton;
        public bool IsVisibleLogOutButton
        {
            get { return isVisibleLogOutButton; }
            set { SetProperty(ref isVisibleLogOutButton, value); }
        }

        private bool isVisibleAdminButtons;
        public bool IsVisibleAdminButtons
        {
            get { return isVisibleAdminButtons; }
            set { SetProperty(ref isVisibleAdminButtons, value); }
        }

        private bool isVisibleMainPageButton;
        public bool IsVisibleMainPageButton
        {
            get { return isVisibleMainPageButton; }
            set { SetProperty(ref isVisibleMainPageButton, value); }
        }

        protected AzureService azureService;
        private AzureService loginService;

        public DelegateCommand LoginFacebookCommand { get; set; }
        public DelegateCommand LogOutFacebookCommand { get; set; }
        public DelegateCommand SchedulesWeekPageCommand { get; set; }
        public DelegateCommand MainPageCommand { get; set; }
        public DelegateCommand EditServicesCommand { get; set; }

        //--------------------------------------------------CONSTRUTOR-------------------------------------------------//
        public MenuPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : base(navigationService, pageDialogService)
        {
            loginService = new AzureService();
            azureService = Xamarin.Forms.DependencyService.Get<AzureService>();

            LoginFacebookCommand = new DelegateCommand(async () => await ExecuteLoginFacebookCommand());
            LogOutFacebookCommand = new DelegateCommand(async () => await ExecuteLogOutFacebookCommandAsync());
            SchedulesWeekPageCommand = new DelegateCommand(async () => await ExecuteSchedulesWeekPageCommand());
            MainPageCommand = new DelegateCommand(async () => await ExecuteMainPageCommand());
            EditServicesCommand = new DelegateCommand(async () => await ExecuteEditServicesCommand());


            if (Settings.IsLoggedIn)
            {
                GetNameAsync();
            }
            else
                Privilegio = "Faça o login";

            if (!CrossConnectivity.Current.IsConnected)
            {
                Settings.AuthToken = string.Empty;
                Settings.UserId = string.Empty;
            }

            IsVisibleAdminButtons = Settings.IsAdmin;
            IsVisibleLogInButton = !Settings.IsLoggedIn;
            IsVisibleLogOutButton = Settings.IsLoggedIn;
            IsVisibleMainPageButton = false;
        }

        async void GetNameAsync()
        {
            await GetFacebookInfo();
        }

        async Task GetFacebookInfo()
        {
            var identity = await loginService.GetIdentityAsync();
            Privilegio = identity.UserClaims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")).Value;
        }

        private async Task ExecuteEditServicesCommand()
        {
            IsVisibleAdminButtons = Settings.IsAdmin;
            IsVisibleMainPageButton = true;
            await _navigationService.NavigateAsync("MyNavigationPage/EditServicesPage", useModalNavigation: false);
        }

        private async Task ExecuteLoginFacebookCommand()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    if (IsBusy || !(await LoginAsync()))
                        return;

                    IsBusy = false;
                    IsVisibleLogInButton = false;
                    IsVisibleLogOutButton = true;
                    IsVisibleAdminButtons = Settings.IsAdmin;
                    GetNameAsync();
                }
                catch { IsBusy = false; }
            }
            else //Se desconectado
            {
                await _pageDialogService.DisplayAlertAsync("Sem rede", "Não é possível fazer login sem conexão com a internet", "OK");
            }
        }

        private Task<bool> LoginAsync()
        {
            IsBusy = true;

            if (Settings.IsLoggedIn)
                return Task.FromResult(true);

            return azureService.LoginAsync();
        }

        private async Task ExecuteLogOutFacebookCommandAsync()
        {
            if (Settings.IsLoggedIn)
            {
                Settings.AuthToken = string.Empty;
                Settings.UserId = string.Empty;
                IsVisibleLogInButton = true;
                IsVisibleLogOutButton = false;
                IsVisibleAdminButtons = false;
                Privilegio = "Faça o login";
                IsVisibleMainPageButton = false;
                await _navigationService.NavigateAsync("MyNavigationPage/MainPage", useModalNavigation: false);
            }
        }

        private async Task ExecuteSchedulesWeekPageCommand()
        {
            await _navigationService.NavigateAsync("MyNavigationPage/SchedulesWeekPage", useModalNavigation: false);
            IsVisibleMainPageButton = true;
        }

        private async Task ExecuteMainPageCommand()
        {
            await _navigationService.NavigateAsync("MyNavigationPage/MainPage", useModalNavigation: false);
            IsVisibleMainPageButton = false;
        }

    }
}