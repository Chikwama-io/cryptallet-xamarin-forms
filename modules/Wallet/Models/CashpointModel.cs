using System;
using System.Numerics;

namespace Wallet.Models
{
    public class CashpointModel
    {
            public string AccountName { get; set; }
            //public string DefaultAddress { get; set; }
            public bool isCashPoint { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int PhoneNumber { get; set; }
            public decimal Rate { get; set; } //local currency to usd rate
            public string EndTime { get; set; } //when time as cashpoint will expire


    }
}


