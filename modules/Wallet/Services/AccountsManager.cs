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
//using System;
//using System.Threading.Tasks;
//using Nethereum.JsonRpc.Client;
//using Nethereum.Web3;
//using Nethereum.Web3.Accounts;
//using Nethereum.StandardTokenEIP20.Events.DTO;
//using Wallet.Core;
//using Nethereum.RPC.Eth.DTOs;
//using Nethereum.Hex.HexConvertors.Extensions;
using Wallet.Models;
//using System.Linq;
//using Xamarin.Forms.Internals;
//using Nethereum.Contracts;
//using Nethereum.StandardTokenEIP20;
//using Nethereum.StandardTokenEIP20.ContractDefinition;

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

namespace Wallet.Services
{
    public interface IAccountsManager
    {
        string DefaultAccountAddress { get; }

        Task<string[]> GetAccountsAsync();

        Task<decimal> GetTokensAsync(string accountAddress);
        Task<decimal> GetBalanceInETHAsync(string accountAddress);

        Task<TransactionModel[]> GetTransactionsAsync(bool sent = false);

        Task<string> TransferAsync(string from, string to, double amount);

        Task<CashpointModel[]> GetCashPointsAsync();

        Task<string> AddCashPointAsync(string name, BigInteger latitude, BigInteger longitude, uint phone, uint rate, uint duration);
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

    [Function("Count", "uint256")]
    public class Count : FunctionMessage
    {

    }

    [Function("keys", "address")]
    public class Keys : FunctionMessage
    {
        [Parameter("uint256", "_index", 1)] public int Index { get; set; }
    }

    [Function("cashpoints", "CashPoint")]
    public class CashPoints : FunctionMessage
    {
        [Parameter("string", "_address", 1)] public string Address { get; set; }
    }

    [Function("addCashPoint")]
    public class AddCashPointFunction:FunctionMessage
    {
        [Parameter("tuple[]", "cashPoint", 1)]
        public CashpointModel CashPoint { get; set; }
    }

    [Function("UpdateEndTime")]
    public class UpdateEndTimeFunction : FunctionMessage
    {
        [Parameter("string", "endTime", 1)]
        public string EndTime { get; set; }

    [FunctionOutput]
    public class AddCashPointOutputDTO : IFunctionOutputDTO
    {
        [Parameter("bool","update", 1)]
        public bool Update { get; set; }

        [Parameter("string", "endTime", 2)]
        public string EndTime { get; set; }
    }

    [FunctionOutput]
    public class CashPointsOutputDTO : IFunctionOutputDTO
    {
            [Parameter("tuple", "cashPoint", 1)]
            public virtual CashpointModel CashPoint { get; set; }

    }

    [FunctionOutput]
    public class KeysOutputDTO : IFunctionOutputDTO
    {

        [Parameter("string", "address", 1)]
        public string Address { get; set; }
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
        const string CONTRACT_ADDRESS = "0x51D46014D44E20F4d3D38218948b194365AC0A70";
        const string CASHPOINT_CONTRACT_ADDRESS = "0x477667CDE1712Cbb698f414C464834303a3b2f0f";

        public string DefaultAccountAddress => DefaultAccount?.Address;

        Account DefaultAccount => walletManager.Wallet?.GetAccount(0);

        readonly IWalletManager walletManager;
        //StandardTokenService standardTokenService;
        Web3 web3;

        public AccountsManager(IWalletManager walletManager)
        {
            this.walletManager = walletManager;

            Initialize();
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
            var countQueryHandler = web3.Eth.GetContractQueryHandler<Count>();

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

            var changes = await transferEventHandler.GetAllChanges(filter);

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

        public async Task<string> AddCashPointAsync(string name, BigInteger latitude, BigInteger longitude, uint phone, uint rate, uint duration)
        {
            var contractHandler = web3.Eth.GetContractHandler(CASHPOINT_CONTRACT_ADDRESS);
            CashpointModel temp = new CashpointModel();

            temp.AccountName = name;
            temp.Latitude = latitude;
            temp.Longitude = longitude;
            temp.isCashPoint = true;
            temp.PhoneNumber = phone;
            temp.Rate = rate;
            

            //DateTime now = DateTime.Now;
            //var endtime = now.AddDays(duration).ToString("F");
            
            //var receiptSending = await contractHandler.SendRequestAndWaitForReceiptAsync(new AddCashPointFunction()
            //{CashPoint = temp});

            var query = await contractHandler
            .QueryDeserializingToObjectAsync<AddCashPointFunction, AddCashPointOutputDTO>(
                new AddCashPointFunction() { CashPoint = temp });

                if (query.Update == true)
                {
                    var oldEnd = DateTime.Parse(query.EndTime);
                    var newEnd = oldEnd.AddDays(duration).ToString("F");
                    var receiptSending = await contractHandler
                    .SendRequestAndWaitForReceiptAsync(new UpdateEndTimeFunction()
                    { EndTime = newEnd});
                    return newEnd;
                }
                else
                {
                    return query.EndTime;   
                }
            //return receiptSending.TransactionHash;
        }

        public string[] addresses;
        public CashpointModel[] cashPoints;

        public async Task<CashpointModel[]> GetCashPointsAsync()
        {
            var contractHandler = web3.Eth.GetContractHandler(CASHPOINT_CONTRACT_ADDRESS);
            var countQueryHandler = web3.Eth.GetContractQueryHandler<Count>();
            var count = await countQueryHandler.QueryAsync<int>(CASHPOINT_CONTRACT_ADDRESS).ConfigureAwait(false);

                //var keysQueryHandler = web3.Eth.GetContractQueryHandler<Keys>();
               
                for (int x = 0; x < count; x++)
                {

                    var query = await contractHandler.QueryDeserializingToObjectAsync<Keys, KeysOutputDTO>(
                    new Keys() { Index = x });

                    addresses[x] = query.Address;
                }


                for (int i = 0; i < addresses.Length; i++)
                {
                    var query = await contractHandler.QueryDeserializingToObjectAsync<CashPoints, CashPointsOutputDTO>(
                    new CashPoints() { Address = addresses[i] });

                    var thisCashPoint = query.CashPoint;

                    CashpointModel cashpoint = new CashpointModel 
                    {
                        AccountName = thisCashPoint.AccountName,
                        Latitude = thisCashPoint.Latitude,
                        Longitude = thisCashPoint.Longitude,
                        PhoneNumber = thisCashPoint.PhoneNumber,
                        EndTime = thisCashPoint.EndTime,
                        Rate = thisCashPoint.Rate,
                        isCashPoint = thisCashPoint.isCashPoint
                    };

                    cashPoints[i] = cashpoint;
                }

                return cashPoints;
            }

            void Initialize()
        {
            //var client = new RpcClient(new Uri("http://192.168.12.154:8545"));//iOS
            //var client = new RpcClient(new Uri("http://10.0.2.2:8545"));//ANDROID
            //var client = new RpcClient(new Uri("https://rinkeby.infura.io/O3CUsRfVYECJi12W8fk3"));

            //web3 = new Web3(DefaultAccount, client);
            //standardTokenService = new StandardTokenService(web3, CONTRACT_ADDRESS);
            web3 = new Web3(DefaultAccount,"http://127.0.0.1:4444/");
        }

        

    
    }

}
