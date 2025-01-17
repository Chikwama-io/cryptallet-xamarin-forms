﻿/*
 * Copyright 2018 NAXAM CO.,LTD.
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */ 
using Wallet.Services;
using Acr.UserDialogs;
namespace Wallet.ViewModels
{
    using System.Windows.Input;
    using Prism.Navigation;
    using Wallet.Core;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    public partial class UnlockViewModel : ViewModelBase
    {
        string _Passcode;
        public string Passcode
        {
            get => _Passcode;
            set => SetProperty(ref _Passcode, value);
        }

        readonly IWalletManager walletManager;
        readonly INavigationService navigator;
        readonly IUserDialogs userDialogs;

        public UnlockViewModel(
            INavigationService navigator,
            IWalletManager walletManager,
            IUserDialogs userDialogs
        )
        {
            this.userDialogs = userDialogs;
            this.walletManager = walletManager;
            this.navigator = navigator;

            ShouldShowNavigationBar = false;
        }

        ICommand _UnlockCommand;
        public ICommand UnlockCommand
        {
            get { return (_UnlockCommand = _UnlockCommand ?? new Command<object>(ExecuteUnlockCommand, CanExecuteUnlockCommand)); }
        }
        bool CanExecuteUnlockCommand(object obj) => true;
        async void ExecuteUnlockCommand(object obj)
        {
            var unlocked = await walletManager.UnlockWalletAsync(Passcode);
            var current = Connectivity.NetworkAccess;

            if (unlocked == false)
            {
                userDialogs.Toast("Invalid PIN.");

                return;
            }

            if (current == NetworkAccess.Internet)
            {
                await navigator.NavigateAsync(NavigationKeys.UnlockWallet);
            }
            else
            {
                userDialogs.Alert("No internet connection");
            }
        }
    }

    partial class UnlockViewModel
    {
        ICommand _CreateCommand;
        public ICommand CreateCommand
        {
            get { return (_CreateCommand = _CreateCommand ?? new Command<object>(ExecuteCreateCommand, CanExecuteCreateCommand)); }
        }
        bool CanExecuteCreateCommand(object obj) => true;
        async void ExecuteCreateCommand(object obj)
        {
            var result = await navigator.NavigateAsync(NavigationKeys.CreateWallet);
        }

        ICommand _RecoverCommand;
        public ICommand RecoverCommand
        {
            get { return (_RecoverCommand = _RecoverCommand ?? new Command<object>(ExecuteRecoverCommand, CanExecuteRecoverCommand)); }
        }
        bool CanExecuteRecoverCommand(object obj) => true;
        async void ExecuteRecoverCommand(object obj)
        {
            await navigator.NavigateAsync(NavigationKeys.RecoverWallet);
        }
    }
}

