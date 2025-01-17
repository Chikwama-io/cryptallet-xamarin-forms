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
using System;
using System.Windows.Input;
using Wallet.Core;
using Wallet.Services;
using Xamarin.Forms;
using Plugin.Share.Abstractions;
using Prism.Navigation;
using Acr.UserDialogs;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Wallet.ViewModels
{
    public partial class WalletViewModel : ViewModelBase
    {
        decimal _Balance;
        public decimal Balance
        {
            get => _Balance;
            set => SetProperty(ref _Balance, value);
        }

        decimal _BalanceInETH;
        public decimal BalanceInETH
        {
            get => _BalanceInETH;
            set => SetProperty(ref _BalanceInETH, value);
        }

        bool _Sendable;
        public bool Sendable
        {
            get => _Sendable;
            set => SetProperty(ref _Sendable, value);
        }

       

        public string DefaultAccountAddress => accountsManager.DefaultAccountAddress;

        readonly IAccountsManager accountsManager;
        readonly INavigationService navigationService;
        readonly IShare share;
        readonly IUserDialogs userDialogs;

        public WalletViewModel(
            IAccountsManager accountsManager,
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

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey("qr_code"))
            {
                ExtractAccountAddress(parameters.GetValue<string>("qr_code"));
            }

            if (Balance == 0)
            {
                UpdateBalance();
            }
        }

        async void UpdateBalance()
        {
            await Task.Delay(100);
            userDialogs.ShowLoading("Refreshing balance");
            await Task.WhenAll(Task.Run(async delegate
            {
                Balance = await accountsManager.GetTokensAsync(accountsManager.DefaultAccountAddress);
            }), Task.Run(async delegate
            {
                BalanceInETH = await accountsManager.GetBalanceInETHAsync(accountsManager.DefaultAccountAddress);
            }));

            Sendable = (Balance >= 1);

            userDialogs.HideLoading();
        }

        ICommand _ShareCommand;
        public ICommand ShareCommand
        {
            get { return (_ShareCommand = _ShareCommand ?? new Command<object>(ExecuteShareCommand, CanExecuteShareCommand)); }
        }
        bool CanExecuteShareCommand(object obj) => true;
        void ExecuteShareCommand(object obj)
        {
            share.Share(new ShareMessage
            {
                Title = "My Ethereum Address",
                Text = $"ethereum:{DefaultAccountAddress}"
            });
        }

        ICommand _RefreshBalanceCommand;
        public ICommand RefreshBalanceCommand
        {
            get { return (_RefreshBalanceCommand = _RefreshBalanceCommand ?? new Command<object>(ExecuteRefreshBalanceCommand, CanExecuteRefreshBalanceCommand)); }
        }
        bool CanExecuteRefreshBalanceCommand(object obj) => true;
        void ExecuteRefreshBalanceCommand(object obj)
        {
            UpdateBalance();
        }
    }

    partial class WalletViewModel
    {
        string _RecipientAddress;
        public string RecipientAddress
        {
            get => _RecipientAddress;
            set => SetProperty(ref _RecipientAddress, value);
        }

        double _SendingAmount;
        public double SendingAmount
        {
            get { return _SendingAmount; }
            set { _SendingAmount = value; Fee = _SendingAmount * 0.02; OnPropertyChanged(); }
        }

        double _Fee;
        public double Fee
        {
            get { return _Fee; }
            set { _Fee = value; OnPropertyChanged(); }
        }

        void ExtractAccountAddress(string qr)
        {
            if (string.IsNullOrWhiteSpace(qr)) return;

            var fragments = qr.Split(new char[] { ':', '?' }, StringSplitOptions.RemoveEmptyEntries);

            RecipientAddress = fragments.Length == 1
                                        ? fragments[0]
                                        : fragments[1];
        }

        ICommand _ScanQRCommand;
        public ICommand ScanQRCommand
        {
            get { return (_ScanQRCommand = _ScanQRCommand ?? new Command<object>(ExecuteScanQRCommand, CanExecuteScanQRCommand)); }
        }
        bool CanExecuteScanQRCommand(object obj) => true;
        async void ExecuteScanQRCommand(object obj)
        {
            await navigationService.NavigateAsync(NavigationKeys.ScanQRCode);
        }

        ICommand _SendCommand;
        public ICommand SendCommand
        {
            get { return (_SendCommand = _SendCommand ?? new Command<object>(ExecuteSendCommand, CanExecuteSendCommand)); }
        }
        bool CanExecuteSendCommand(object obj) => true;
        async void ExecuteSendCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(RecipientAddress) || SendingAmount <= 0) return;
            var toAddress = RecipientAddress.Trim();

            if (toAddress.Length != DefaultAccountAddress.Length) return;

            userDialogs.ShowLoading("Sending");
            var result = await accountsManager.TransferAsync(DefaultAccountAddress, toAddress, SendingAmount);
            userDialogs.Toast($"tx:{result}");
            userDialogs.HideLoading();
            UpdateBalance();
        }

    }

    partial class WalletViewModel
    {
        private Uri _Url;

        public Uri Url
        {
            get { return _Url; }
            set { _Url = value; }
        }

        ICommand _ViewHistoryCommand;
        public ICommand ViewHistoryCommand
        {
            get { return (_ViewHistoryCommand = _ViewHistoryCommand ?? new Command<object>(ExecuteViewHistoryCommand, CanExecuteViewHistoryCommand)); }
        }
        bool CanExecuteViewHistoryCommand(object obj) => true;
        async void ExecuteViewHistoryCommand(object obj)
        {
            //await navigationService.NavigateAsync(NavigationKeys.WalletViewHistory);
            Url = new Uri("https://explorer.testnet.rsk.co/address/" + DefaultAccountAddress);
            await Browser.OpenAsync(Url, BrowserLaunchMode.SystemPreferred);
        
        }
    }
}
