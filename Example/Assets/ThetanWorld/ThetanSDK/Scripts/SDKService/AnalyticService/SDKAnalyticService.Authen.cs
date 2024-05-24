using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.SDKServices.Analytic
{
    internal partial class SDKAnalyticService
    {
        /// <summary>
        /// Log login success everytime user log in
        /// </summary>
        /// <param name="metaData"></param>
        public async void LogLoginSuccess(PostAuthenSuccessMetaData metaData)
        {
            try
            {
                _dataLog.Clear();
                
                var profileService = ThetanSDKManager.Instance.ProfileService;
                var nftService = ThetanSDKManager.Instance.NftItemService;

                UniTaskCompletionSource<int> getNFTHeroSource = new UniTaskCompletionSource<int>();
                UniTaskCompletionSource<UserWalletInfo> getWalletInfo = new UniTaskCompletionSource<UserWalletInfo>();
                
                nftService.FetchTotalNFTCount(nftCount => getNFTHeroSource.TrySetResult(nftCount), 
                    error => getNFTHeroSource.TrySetResult(0));

                GetUserCurrencyBalance(walletInfo => getWalletInfo.TrySetResult(walletInfo),
                    error => getWalletInfo.TrySetResult(new UserWalletInfo()));
                
                (var countNFTHero, var walletInfo) = await (getNFTHeroSource.Task, getWalletInfo.Task);

                _dataLog["login_type"] = metaData.loginType.ToString();
                _dataLog["client_id"] = _networkConfig.ApplicationID;
                _dataLog["user_id"] = profileService.UserId;
                _dataLog["email"] = profileService.Email;
                _dataLog["country"] = profileService.UserCountry;
                _dataLog["device_id"] = metaData.deviceId;
                _dataLog["device_name"] = SystemInfo.deviceModel;
                _dataLog["wallet_address"] = profileService.WalletAddress;
                _dataLog["wallet_provider"] = profileService.WalletProvider;
                _dataLog["nft_hero_balance"] = countNFTHero.ToString();
                _dataLog["gthg_balance"] = walletInfo.gTHG.ToString();
                
                LogEvent("tw_sdk_login_success", _dataLog);
            }
            catch (Exception ex)
            {
                
            }
            
        }

        private void GetUserCurrencyBalance(Action<UserWalletInfo> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/user/wallet")
                .Get();

            WolffunUnityHttp.Instance.MakeAPI(reqCommon, onSuccessCallback, onErrorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        private struct UserWalletInfo
        {
            public long gTHG;
        }
    }
}