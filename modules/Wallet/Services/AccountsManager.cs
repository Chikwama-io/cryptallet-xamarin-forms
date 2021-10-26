/*
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

using Wallet.Models;


using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using Nethereum.RPC.Eth.DTOs;
using System.Linq;
using Nethereum.Hex.HexTypes;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace Wallet.Services
{
    public interface IAccountsManager
    {
        string DefaultAccountAddress { get; }
        string ThisCity { get; }
        Task<string[]> GetAccountsAsync();

        Task<decimal> GetTokensAsync(string accountAddress);
        Task<decimal> GetBalanceInETHAsync(string accountAddress);

        Task<TransactionModel[]> GetTransactionsAsync(bool sent = false);

        Task<string> TransferAsync(string from, string to, double amount);

        Task<CashpointModel[]> GetCashPointsAsync();

        Task<string> AddCashPointAsync(string name, BigInteger latitude, BigInteger longitude, uint phone, decimal rate, uint duration);
    }

    public partial class CashPoint : CashPointBase { }


    public class CashPointBase
    {
        [Parameter("string", "_name", 1)]
        public virtual string Name { get; set; }

        [Parameter("int", "_latitude", 2)]
        public virtual BigInteger Latitude { get; set; }

        [Parameter("int", "_longitude", 3)]
        public virtual BigInteger Longitude { get; set; }

        [Parameter("uint", "_phoneNumber", 4)]
        public virtual int PhoneNumber { get; set; }

        [Parameter("uint", "rate", 5)]
        public virtual BigInteger Rate { get; set; }

        [Parameter("string", "endtime", 6)]
        public virtual string Endtime { get; set; }

        [Parameter("bool", "isCashPoint", 7)]
        public virtual bool IsCashPoint { get; set; }
    }

    // The balance function message definition    
    [Function("balanceOf", "uint256")]
    public class BalanceOfFunction : FunctionMessage
    {
        [Parameter("address", "_owner", 1)] public string Owner { get; set; }
    }

    [Function("transfer", "bool")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger TokenAmount { get; set; }
    }

    [Function("count", "uint256")]
    public class Count : FunctionMessage
    {

    }

    [Function("keys", "address")]
    public class Keys : FunctionMessage
    {
        [Parameter("uint256", "_index", 1)] public int Index { get; set; }
    }

    public partial class cashpointsFunction : cashpointsBase { }

    [Function("getCashPoint", typeof(CashPointsOutputDTO))]
    public class cashpointsBase : FunctionMessage
    {
        [Parameter("address", "_cashpoint", 1)] public virtual string Cashpoint { get; set; }
    }

    [Function("addCashPoint")]
    public class AddCashPointFunction : FunctionMessage
    {
        [Parameter("string", "_name", 1)]
        public string Name { get; set; }

        [Parameter("int", "_latitude", 2)]
        public BigInteger Latitude { get; set; }

        [Parameter("int", "_longitude", 3)]
        public BigInteger Longitude { get; set; }

        [Parameter("uint", "_phoneNumber", 4)]
        public BigInteger PhoneNumber { get; set; }

        [Parameter("uint", "rate", 5)]
        public BigInteger Rate { get; set; }

        [Parameter("string", "endtime", 6)]
        public string Endtime { get; set; }
    }

    [Function("updateCashPoint")]
    public class UpdateCashPointFunction : FunctionMessage
    {
        [Parameter("string", "_name", 1)]
        public string Name { get; set; }

        [Parameter("int", "_latitude", 2)]
        public BigInteger Latitude { get; set; }

        [Parameter("int", "_longitude", 3)]
        public BigInteger Longitude { get; set; }

        [Parameter("uint", "_phoneNumber", 4)]
        public BigInteger PhoneNumber { get; set; }

        [Parameter("uint", "rate", 5)]
        public BigInteger Rate { get; set; }

        [Parameter("string", "endtime", 6)]
        public string Endtime { get; set; }
    }

    [FunctionOutput]
    public class AddCashPointOutputDTO : IFunctionOutputDTO
    {

        [Parameter("string", "endTime", 1)]
        public string EndTime { get; set; }

        [Parameter("bool","update", 2)]
        public bool Update { get; set; }
        
    }

    public partial class CashPointsOutputDTO : CashPointsOutputDTOBase { }

    [FunctionOutput]
    public class CashPointsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("tuple", "_cashpoint", 1)]
        public virtual CashPoint CashPoint { get; set; }
    }

    [FunctionOutput]
    public class KeysOutputDTO : IFunctionOutputDTO
    {

        [Parameter("string", "address", 1)]
        public string Address { get; set; }
    }

    [FunctionOutput]
    public class CountOutputDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "count", 1)]
        public BigInteger Count { get; set; }
    }

    [Event("Transfer")]
    public class TransferEventDTO : IEventDTO
    {
        [Parameter("address", "_from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "_to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 3, false)]
        public BigInteger Value { get; set; }
    }

    public class AccountsManager : IAccountsManager
    {
        const string CONTRACT_ADDRESS = "0x46Cb88e688cBd98c18F700ccF6430f511FBcF9Cc";
        const string CASHPOINT_CONTRACT_ADDRESS = "0x310c1F3f17B3B8493116F71e81ccc25e67733181";
        const string url = "http://127.0.0.1:4444";
        public string DefaultAccountAddress => DefaultAccount?.Address;
        public string ThisCity=>City;

        string City;
        BigInteger InitialGasPrice = 65164000;
        BigInteger GasPrice;

        Account DefaultAccount => walletManager.Wallet?.GetAccount(0);

        string ABI = @"[{'anonymous':false,'inputs':[{'indexed':false,'internalType':'address','name':'cashpoint','type':'address'}],'name':'CreatedCashPoint','type':'event'},{'inputs':[{'internalType':'string','name':'name','type':'string'},{'internalType':'int256','name':'mylat','type':'int256'},{'internalType':'int256','name':'mylong','type':'int256'},{'internalType':'uint256','name':'phone','type':'uint256'},{'internalType':'uint256','name':'rate','type':'uint256'},{'internalType':'string','name':'endtime','type':'string'}],'name':'addCashPoint','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'','type':'address'}],'name':'cashpoints','outputs':[{'internalType':'string','name':'_name','type':'string'},{'internalType':'int256','name':'_latitude','type':'int256'},{'internalType':'int256','name':'_longitude','type':'int256'},{'internalType':'uint256','name':'_phoneNumber','type':'uint256'},{'internalType':'uint256','name':'rate','type':'uint256'},{'internalType':'string','name':'endtime','type':'string'},{'internalType':'bool','name':'isCashPoint','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'count','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'Add','type':'address'}],'name':'getCashPoint','outputs':[{'components':[{'internalType':'string','name':'_name','type':'string'},{'internalType':'int256','name':'_latitude','type':'int256'},{'internalType':'int256','name':'_longitude','type':'int256'},{'internalType':'uint256','name':'_phoneNumber','type':'uint256'},{'internalType':'uint256','name':'rate','type':'uint256'},{'internalType':'string','name':'endtime','type':'string'},{'internalType':'bool','name':'isCashPoint','type':'bool'}],'internalType':'struct CashPoints.CashPoint','name':'_cashpoint','type':'tuple'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'','type':'uint256'}],'name':'keys','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'string','name':'name','type':'string'},{'internalType':'int256','name':'mylat','type':'int256'},{'internalType':'int256','name':'mylong','type':'int256'},{'internalType':'uint256','name':'phone','type':'uint256'},{'internalType':'uint256','name':'rate','type':'uint256'},{'internalType':'string','name':'endtime','type':'string'}],'name':'updateCashPoint','outputs':[],'stateMutability':'nonpayable','type':'function'}]";


        readonly IWalletManager walletManager;
        //StandardTokenService standardTokenService;
        Web3 web3;

        public AccountsManager(IWalletManager walletManager)
        {
            this.walletManager = walletManager;
            GetGasPrice();
            Initialize();
        }

        public async Task GetGasPrice()
        {
            var account = new Account(DefaultAccount.PrivateKey, 33);
            web3 = new Web3(account, url);
            var gas = await web3.Eth.GasPrice.SendRequestAsync();
            GasPrice = gas.Value;
        }

            public async Task<string[]> GetAccountsAsync()
        {
            return new string[] { DefaultAccountAddress };
        }

        public async Task<decimal> GetTokensAsync(string accountAddress)
        {
            //var wei = await standardTokenService.BalanceOfQueryAsync(accountAddress);

            //return (decimal)wei;
            var balanceOfMessage = new BalanceOfFunction() { Owner = DefaultAccountAddress };
            var queryHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

     
            var tknbalance = await queryHandler
            .QueryAsync<BigInteger>(CONTRACT_ADDRESS, balanceOfMessage)
            .ConfigureAwait(false);
            return Web3.Convert.FromWei(tknbalance);

        }

        public async Task<decimal> GetBalanceInETHAsync(string accountAddress)
        {
            //var balanceInWei = await web3.Eth.GetBalance.SendRequestAsync(accountAddress);

            //return Web3.Convert.FromWei(balanceInWei);

            var balance = await web3.Eth.GetBalance.SendRequestAsync(DefaultAccountAddress);
           

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            return etherAmount;
        }

        public async Task<string> TransferAsync(string from, string to, double amount)
        {
            //var receipt = await standardTokenService.TransferFromRequestAndWaitForReceiptAsync(from, to, new System.Numerics.BigInteger((int)amount));
            var receiverAddress = to;
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            

                var transfer = new TransferFunction()
            {
                To = receiverAddress,
                TokenAmount = Web3.Convert.ToWei(amount)
            };

            var transactionTransferReceipt =
            await transferHandler.SendRequestAndWaitForReceiptAsync(CONTRACT_ADDRESS, transfer);

            return transactionTransferReceipt.TransactionHash;
            //return receipt.TransactionHash;
        }

        public Task<TransactionModel[]> GetTransactionsAsync(bool sent = false)
        {



            return Task.Run(async delegate
            {
                    //        var transferEvent = standardTokenService.GetTransferEvent();



                    var transferEventHandler = web3.Eth.GetEvent<TransferEventDTO>(CONTRACT_ADDRESS);

            //var paddedAccountAddress = DefaultAccountAddress.RemoveHexPrefix()
            //                                 .PadLeft(64, '0')
            //                                 .EnsureHexPrefix();

            var filter = transferEventHandler.CreateFilterInput(
                        new object[] { sent ? DefaultAccountAddress : null },
                        new object[] { sent ? null : DefaultAccountAddress },
                        BlockParameter.CreateEarliest(),
                        BlockParameter.CreateLatest());

            //        var filter = transferEvent.CreateFilterInput(
            //            new object[] { sent ? paddedAccountAddress : null },
            //            new object[] { sent ? null : paddedAccountAddress },
            //            BlockParameter.CreateEarliest(),
            //            BlockParameter.CreateLatest());

            var changes = await transferEventHandler.GetAllChangesAsync(filter);

                var timestampTasks = changes.Select(x => Task.Factory.StartNew(async (state) =>
                {
                    var log = (EventLog<TransferEventDTO>)state;

                    var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(log.Log.BlockNumber);

                    return new TransactionModel
                    {
                        Sender = log.Event.From,
                        Receiver = log.Event.To,
                        Amount = (decimal)log.Event.Value,
                        Inward = false == sent,
                        Timestamp = (long)block.Timestamp.Value
                    };
                }, x));

                return await Task.WhenAll(timestampTasks).ContinueWith(tt =>
                {
                    return tt.Result.Select(x => x.Result).ToArray();
                });
            });
            }

        public async Task<string> AddCashPointAsync(string name, BigInteger latitude, BigInteger longitude, uint phone, decimal rate, uint duration)
        {
            var account = new Account(DefaultAccount.PrivateKey, 33);
            web3 = new Web3(account, url);
            //web3.Eth.TransactionManager.UseLegacyAsDefault = true;
            var contractHandler = web3.Eth.GetContractHandler(CASHPOINT_CONTRACT_ADDRESS);
            var updateCashPointHandler = web3.Eth.GetContractTransactionHandler<UpdateCashPointFunction>();
            var addCashPointHandler = web3.Eth.GetContractTransactionHandler<AddCashPointFunction>();
            //Contract CashPointContract = web3.Eth.GetContract(ABI, CASHPOINT_CONTRACT_ADDRESS);
            DateTime now = DateTime.Now;
            var endtime = now.AddDays(duration).ToString("F");

            
            var thisCashPointsDetails = await contractHandler.QueryDeserializingToObjectAsync<cashpointsFunction, CashPointsOutputDTO>(new cashpointsFunction { Cashpoint = DefaultAccountAddress });

            if (thisCashPointsDetails.CashPoint.IsCashPoint)
            {
                var oldEnd = DateTime.Parse(thisCashPointsDetails.CashPoint.Endtime);
                endtime = oldEnd.AddDays(duration).ToString("F");

                //var receiptSending = await contractHandler.SendRequestAndWaitForReceiptAsync(new UpdateCashPointFunction()
                //{ Name = name, Latitude = latitude, Longitude = longitude, PhoneNumber = phone, Rate = Web3.Convert.ToWei(rate), Endtime = endtime });
                var update = new UpdateCashPointFunction()
                {
                    Name = name,
                    Latitude = latitude,
                    Longitude = longitude,
                    PhoneNumber = phone,
                    Rate = Web3.Convert.ToWei(rate),
                    Endtime = endtime

                };
                var transactionReceipt = await updateCashPointHandler.SendRequestAndWaitForReceiptAsync(CASHPOINT_CONTRACT_ADDRESS, update);


                return transactionReceipt.TransactionHash;
            }
            else
            {
                var add = new AddCashPointFunction()
                {
                    Name = name,
                    Latitude = latitude,
                    Longitude = longitude,
                    PhoneNumber = phone,
                    Rate = Web3.Convert.ToWei(rate),
                    Endtime = endtime

                };
                var transactionReceipt = await addCashPointHandler.SendRequestAndWaitForReceiptAsync(CASHPOINT_CONTRACT_ADDRESS, add);


                return transactionReceipt.TransactionHash;
            }
        }

        //public string[] addresses;
        

        public async Task<CashpointModel[]> GetCashPointsAsync()
        {
            List<CashpointModel> cashPoints = new List<CashpointModel>();
            var account = new Account(DefaultAccount.PrivateKey, 33);
            web3 = new Web3(account, url);
            var contractHandler = web3.Eth.GetContractHandler(CASHPOINT_CONTRACT_ADDRESS);
            var queryCount = await contractHandler.QueryDeserializingToObjectAsync<Count, CountOutputDTO>(new Count() { });
            var count = queryCount.Count;
            GetCity();
            //var keysQueryHandler = web3.Eth.GetContractQueryHandler<Keys>();
            if (count == 0)
            {
                return cashPoints.ToArray();
            }
            else
            {
                for (int x = 1; x <= count; x++)
                {

                    //int length = 0;
                    var query = await contractHandler.QueryAsync<Keys, string>(new Keys() { Index = x });

                    //addresses[length] = query;
                    

                    var thisCashPointsDetails = await contractHandler.QueryDeserializingToObjectAsync<cashpointsFunction, CashPointsOutputDTO>(new cashpointsFunction { Cashpoint = query });

                    var thisCashPoint = thisCashPointsDetails.CashPoint;
                    var lat = double.Parse(Web3.Convert.FromWei(thisCashPoint.Latitude).ToString());
                    var longi = double.Parse(Web3.Convert.FromWei(thisCashPoint.Longitude).ToString());
                    var location = new Location(lat, longi);
                    var placemarks = await Geocoding.GetPlacemarksAsync(location);
                    var placemark = placemarks?.FirstOrDefault();

                   
                        var endTime = DateTime.Parse(thisCashPoint.Endtime);
                    var now = DateTime.Now;
                    if (endTime > now && placemark.Locality == City)
                    {
                        CashpointModel thiscashpoint = new CashpointModel()
                        {
                            AccountName = thisCashPoint.Name,
                            Latitude = lat,
                            Longitude = longi,
                            PhoneNumber = thisCashPoint.PhoneNumber,
                            EndTime = thisCashPoint.Endtime,
                            Rate = Web3.Convert.FromWei(thisCashPoint.Rate),
                            isCashPoint = thisCashPoint.IsCashPoint
                        };

                        cashPoints.Add(thiscashpoint);
                    }
                    //length++;


                }
            }


               

                return cashPoints.ToArray();
            
        }

        async void GetCity()
        {
            var location = await Geolocation.GetLocationAsync();
            var placemarks = await Geocoding.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            City = placemark.Locality;
         
        }
        void Initialize()
        {
            //var client = new RpcClient(new Uri("http://192.168.12.154:8545"));//iOS
            //var client = new RpcClient(new Uri("http://10.0.2.2:8545"));//ANDROID
            //var client = new RpcClient(new Uri("https://rinkeby.infura.io/O3CUsRfVYECJi12W8fk3"));

            //web3 = new Web3(DefaultAccount, client);
            //standardTokenService = new StandardTokenService(web3, CONTRACT_ADDRESS);
            web3 = new Web3(DefaultAccount,url);
        }




    }

}
