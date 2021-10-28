using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Plugin.Share.Abstractions;
using Prism.Navigation;
using Wallet.Core;
using Wallet.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Wallet.ViewModels
{
    public class CreateCashpointViewModel : ViewModelBase
    {
            readonly IAccountsManager accountsManager;
            readonly INavigationService navigationService;
            readonly IShare share;
            readonly IUserDialogs userDialogs;


            string _CashpointName;
            public string CashpointName
            {
                get => _CashpointName;
                set => SetProperty(ref _CashpointName, value);
            }

            decimal _Rate;
            public decimal Rate
            {
                get => _Rate;
                set => SetProperty(ref _Rate, value);
            }

            uint _Phone;
            public uint Phone
            {
                get => _Phone;
                set => SetProperty(ref _Phone, value);
            }

            uint _Duration;
            public uint Duration
            {
                get => _Duration;
                set { _Duration = value; Cost = _Duration * 0.5; OnPropertyChanged(); }
            }



            double _Cost;
            public double Cost
            {
                get => _Cost;
                set => SetProperty(ref _Cost, value);
             }



    double MyLat;
            double MyLong;
            //CancellationTokenSource cts;
        //public double Cost => Duration * 1;
        public string DefaultAccountAddress => accountsManager.DefaultAccountAddress;

            public ICommand CreateCashpointCommand { get; set; }

            public CreateCashpointViewModel(IAccountsManager accountsManager,
                INavigationService navigationService,
                IShare share,
                IUserDialogs userDialogs)
            {
                this.accountsManager = accountsManager;
                this.navigationService = navigationService;
                this.share = share;
                this.userDialogs = userDialogs;
            }


        ICommand _AddCashPointCommand;
        public ICommand AddCashPointCommand
        {
            get { return (_AddCashPointCommand = _AddCashPointCommand ?? new Command<object>(ExecuteAddCashPointCommand, CanExecuteAddCashPointCommand)); }
        }
        bool CanExecuteAddCashPointCommand(object obj) => true;
        async void ExecuteAddCashPointCommand(object obj)
        {
            userDialogs.ShowLoading("Adding cash point");
            var location = await Geolocation.GetLocationAsync();
            MyLat = location.Latitude; 
            MyLong = location.Longitude;
            var result = await accountsManager.AddCashPointAsync(CashpointName, Nethereum.Util.UnitConversion.Convert.ToWei(MyLat), Nethereum.Util.UnitConversion.Convert.ToWei(MyLong),Phone,Rate,Duration);
            userDialogs.Alert($"Cash point added, Transaction ID:{result}");
            userDialogs.HideLoading();
        }



    }

}
