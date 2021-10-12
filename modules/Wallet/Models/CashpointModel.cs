using System;
namespace Wallet.Models
{
    public class CashpointModel
    {
            public string AccountName { get; set; }
            public string DefaultAddress { get; set; }
            public bool isCashPoint { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public uint PhoneNumber { get; set; }
            public decimal Rate { get; set; } //local currency to usd rate
            public DateTime EndTime { get; set; } //when time as cashpoint will expire


    }
}
