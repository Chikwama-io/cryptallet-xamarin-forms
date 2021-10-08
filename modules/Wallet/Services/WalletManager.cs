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
namespace Wallet.Services
{
    using System.Threading.Tasks;
    using NBitcoin;
    using Nethereum.HdWallet;
    //using Plugin.SecureStorage.Abstractions;
    using Xamarin.Essentials;
    using Nethereum.Hex.HexConvertors.Extensions;
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.IO;

    public interface IWalletManager
    {
        Wallet Wallet { get; }

        Task CreateWalletAsync();

        Task SaveWalletAsync(string password);

        Task<bool> UnlockWalletAsync(string password, bool bypass = false);

        Task<bool> RestoreWallet(string seedWords, string password);
    }

    public class WalletManager : IWalletManager
    {
        const string PASSWORD_KEY = "__wallet__password__";
        const string SEED_KEY = "__wallet__seed__";
        const string SEED_PASSWORD = "GNzheuuNsOkkBHG3hFSpA37UAQ1TDWQH0ncZcR2+7r4=";

        Wallet wallet;
        public Wallet Wallet => wallet;
        IDictionary<string, string> Keystore;
        private string JSON;

        //readonly ISecureStorage _secureStorage;

        public WalletManager()
        {
            Keystore = new Dictionary<string, string>();

        }

        public Task CreateWalletAsync()
        {
            return Task.Run(delegate
            {
                wallet = new Wallet(Wordlist.English, WordCount.Twelve, SEED_PASSWORD);
            });
        }

        public Task SaveWalletAsync(string password)
        {
            return Task.Run(delegate
            {
                if (wallet == null) return;

                StoreCredentials(password);
            });
        }

        public Task<bool> UnlockWalletAsync(string password, bool bypass = false)
        {
            return Task.Run(delegate
            {
                
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string filePath = Path.Combine(path, "info.json");
                if (File.Exists(filePath))
                {
                    JSON = "";
                    JSON = File.ReadAllText(filePath);
                    this.Keystore = JsonConvert.DeserializeObject<Dictionary<string, string>>(JSON);

                    string mySeed;

                    if (Keystore.TryGetValue(password, out mySeed))
                    {
                        wallet = new Wallet(mySeed.HexToByteArray());

                        return true;
                    }
                    else
                    {
                        return false;
                    }

                    
                }
                return false;
            });

        }

        public Task<bool> RestoreWallet(string seedWords, string password)
        {


            return Task.Run(delegate
            {
                wallet = new Wallet(seedWords, SEED_PASSWORD);

                StoreCredentials(password);

                return true;
            });

        }


        void StoreCredentials(string password)
        {
            Keystore.Clear();
            Keystore.Add(password, wallet.Seed);

            JSON = JsonConvert.SerializeObject(Keystore, Formatting.Indented);

            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(path, "info.json");
            using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (var strm = new StreamWriter(file))
            {
                strm.Write(JSON);
            }

            //var account = wallet.GetAccount(0);

            //secureStorgage.SetValue(SEED_KEY, wallet.Seed);
            //secureStorgage.SetValue(account.Address, account.PrivateKey);
        }
    }
}
