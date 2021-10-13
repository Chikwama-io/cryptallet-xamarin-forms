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

            double MyLat;
            double MyLong;
            CancellationTokenSource cts;
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
            await GetCurrentLocation();
            var result = await accountsManager.AddCashPointAsync(CashpointName, Nethereum.Util.UnitConversion.Convert.ToWei(MyLat), Nethereum.Util.UnitConversion.Convert.ToWei(MyLong),Phone,Rate,Duration);
            userDialogs.Toast($"Cash point added, active until :{result}");
            userDialogs.HideLoading();
        }


        async Task GetCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);


                if (location != null)
                {
                    MyLat = location.Latitude;
                    MyLong = location.Longitude;
                    //Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }

                //return null;
            }
            catch (FeatureNotSupportedException fnsEx)
            {

                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {

                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {

                // Handle permission exception
            }
            catch (Exception ex)
            {

                // Unable to get location
            }
        }

    }

}
