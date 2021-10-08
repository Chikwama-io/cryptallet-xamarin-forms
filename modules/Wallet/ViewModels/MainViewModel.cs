using System;
using System.Windows.Input;
using Acr.UserDialogs;
using Plugin.Share.Abstractions;
using Prism.Navigation;
using Wallet.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Wallet.ViewModels
{
    public class MainViewModel
    {
        public string DefaultAccountAddress => accountsManager.DefaultAccountAddress;

        readonly IAccountsManager accountsManager;
        readonly INavigationService navigationService;
        readonly IShare share;
        readonly IUserDialogs userDialogs;

        public MainViewModel(IAccountsManager accountsManager,
            INavigationService navigationService,
            IShare share,
            IUserDialogs userDialogs
        )
        {
            this.accountsManager = accountsManager;
            this.navigationService = navigationService;
            this.share = share;
            this.userDialogs = userDialogs;
        }

        ICommand _ShareCommand;
        public ICommand ShareCommand
        {
            get { return (_ShareCommand = _ShareCommand ?? new Command<object>(ExecuteShareCommand, CanExecuteShareCommand)); }
        }
        bool CanExecuteShareCommand(object obj) => true;
        async void ExecuteShareCommand(object obj)
        {
            await Clipboard.SetTextAsync($"Chikwama Address:{DefaultAccountAddress}");
            userDialogs.Toast("Address copied to clipboard");
            //share.Share(new ShareMessage
            //{
            //    Title = "My ChikwamaAddress",
            //    Text = $"Chikwama Address:{DefaultAccountAddress}"
            //});
        }

        ICommand _SendCommand;
        public ICommand SendCommand
        {
            get { return (_SendCommand = _SendCommand ?? new Command<object>(ExecuteSendCommand, CanExecuteSendCommand)); }
        }
        bool CanExecuteSendCommand(object obj) => true;
        async void ExecuteSendCommand(object obj)
        {
            await navigationService.NavigateAsync(NavigationKeys.SendView);
        }

        ICommand _SellCommand;
        public ICommand SellCommand
        {
            get { return (_SellCommand = _SellCommand ?? new Command<object>(ExecuteSellCommand, CanExecuteSellCommand)); }
        }
        bool CanExecuteSellCommand(object obj) => true;
        async void ExecuteSellCommand(object obj)
        {
            await navigationService.NavigateAsync(NavigationKeys.CreateCashpoint);
        }

        ICommand _BuyCommand;
        public ICommand BuyCommand
        {
            get { return (_BuyCommand = _BuyCommand ?? new Command<object>(ExecuteBuyCommand, CanExecuteBuyCommand)); }
        }
        bool CanExecuteBuyCommand(object obj) => true;
        async void ExecuteBuyCommand(object obj)
        {
            await navigationService.NavigateAsync(NavigationKeys.ViewCashpoints);
        }

        ICommand _LogOutCommand;
        public ICommand LogOutCommand
        {
            get { return (_LogOutCommand = _LogOutCommand ?? new Command<object>(ExecuteLogOutCommand, CanExecuteLogOutCommand)); }
        }
        bool CanExecuteLogOutCommand(object obj) => true;
        async void ExecuteLogOutCommand(object obj)
        {
            await navigationService.NavigateAsync(NavigationKeys.Logout);
        }
    }
}
