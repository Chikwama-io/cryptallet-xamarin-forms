﻿using System;
using Wallet.Core;
using Wallet.Services;
namespace Wallet.ViewModels
{
    public class PasscodeViewModel : ViewModelBase
    {
        readonly NewWalletController controller;

        public PasscodeViewModel(NewWalletController controller)
        {
            this.controller = controller;
        }
    }
}
