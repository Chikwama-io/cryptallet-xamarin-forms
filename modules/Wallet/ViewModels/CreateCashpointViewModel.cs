using System.Numerics;
using System.Windows.Input;
using Acr.UserDialogs;
using Plugin.Share.Abstractions;
using Prism.Navigation;
using Wallet.Core;
using Wallet.Services;
using Xamarin.Essentials;

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

            public uint _Duration;
            public uint Duration
            {
                get { return _Duration; }
                set { _Duration = value; Cost = Duration * 0.5; OnPropertyChanged(); }
            }


            double _Cost;
            public double Cost
            {
                get { return _Cost; }
                set { _Cost = value; OnPropertyChanged(); }
            }


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


        //async Task ExecuteBecomeCashPointCommand()
        //{
        //    var location = await Geolocation.GetLastKnownLocationAsync();
        //    var MyLat = location.Latitude;
        //    var MyLong = location.Longitude;

        //    BigInteger latitude = Nethereum.Util.UnitConversion.Convert.ToWei(MyLat);
        //    BigInteger longitude = Nethereum.Util.UnitConversion.Convert.ToWei(MyLong);



        //    //var result = await accountsManager.BecomeCashPointAsync(DefaultAccountAddress, CashPointName, latitude, longitude, Phone, Rate, Duration);
        //    //await navService.DisplayAlert("Send Result", $"tx:{result}", "ok", "cancel");

        //}

    }

}
