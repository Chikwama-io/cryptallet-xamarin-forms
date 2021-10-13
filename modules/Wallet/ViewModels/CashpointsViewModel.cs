using System;
using Acr.UserDialogs;
using Plugin.Share.Abstractions;
using Prism.Navigation;
using Wallet.Core;
using Wallet.Services;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Wallet.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Wallet.ViewModels
{
    public class CashpointsViewModel : ViewModelBase
    {
        public Xamarin.Forms.Maps.Map CashpointsMap { get; private set; }


        string _CashPointName;

        bool _IsFetching;
        public bool IsFetching
        {
            get { return _IsFetching; }
            set { _IsFetching = value; OnPropertyChanged(); }
        }
        public string CashPointName
        {
            get { return _CashPointName; }
            set { _CashPointName = value; OnPropertyChanged(); }
        }

        uint _Rate;
        public uint Rate
        {
            get { return _Rate; }
            set { _Rate = value; OnPropertyChanged(); }
        }

        uint _Phone;
        public uint Phone
        {
            get { return _Phone; }
            set { _Phone = value; OnPropertyChanged(); }
        }

        decimal _Cost;
        public decimal Cost
        {
            get { return _Cost; }
            set { _Cost = value; OnPropertyChanged(); }
        }

        double MyLat;
        double MyLong;

        readonly IAccountsManager accountsManager;
        readonly INavigationService navigationService;
        readonly IShare share;
        readonly IUserDialogs userDialogs;

        public Position Position { get; set; }


        public string DefaultAccountAddress => accountsManager.DefaultAccountAddress;

        CashpointModel[] _Cashpoint;


        async void GetCashPoints()
        {
            userDialogs.ShowLoading("Finding cash points");
            //var location = await Geolocation.GetLastKnownLocationAsync();
            //MyLat = location.Latitude;
            //MyLong = location.Longitude;


            _Cashpoint = await accountsManager.GetCashPointsAsync();
            if (_Cashpoint.Length != 0)
            {
                foreach (var cashpoint in _Cashpoint)
                {
                    // Place a pin on the map for each cash point
                    var endTime = DateTime.Parse(cashpoint.EndTime);
                    var now = DateTime.Now;
                    if (endTime < now)
                    {
                        CashpointsMap.Pins.Add(new Pin
                        {
                            Type = PinType.SearchResult,
                            Label = cashpoint.AccountName + ": 0" + cashpoint.PhoneNumber + ", Local Currency to USD Rate:" + cashpoint.Rate + ", Until: "+ endTime.ToShortDateString(),
                            Position = new Position((double)cashpoint.Latitude, (double)cashpoint.Longitude)
                        });
                    }
                }
            }


            userDialogs.HideLoading();

        }

        async void ShowMap()
        {
            CashpointsMap = new Xamarin.Forms.Maps.Map();
            await GetCurrentLocation();
            

            //var MyLat = location.Latitude;
            //var MyLong = location.Longitude;

            // Center the map around users current location
            CashpointsMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(MyLat, MyLong), Distance.FromKilometers(10.0)));
            GetCashPoints();
        }

        CancellationTokenSource cts;

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

        //protected override void OnDisappearing()
        //{
        //    if (cts != null && !cts.IsCancellationRequested)
        //        cts.Cancel();
        //    base.OnDisappearing();
        //}


        public CashpointsViewModel(IAccountsManager accountsManager,
                INavigationService navigationService,
                IShare share,
                IUserDialogs userDialogs)
        {
            this.accountsManager = accountsManager;
            this.navigationService = navigationService;
            this.share = share;
            this.userDialogs = userDialogs;
            ShowMap();
        }
    }
}

