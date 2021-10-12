using System;
using System.Numerics;

namespace Wallet.Models
{
    public class CashpointModel
    {
            public string AccountName { get; set; }
            //public string DefaultAddress { get; set; }
            public bool isCashPoint { get; set; }
            public BigInteger Latitude { get; set; }
            public BigInteger Longitude { get; set; }
            public uint PhoneNumber { get; set; }
            public decimal Rate { get; set; } //local currency to usd rate
            public DateTime EndTime { get; set; } //when time as cashpoint will expire


    }
}
