﻿using System;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.LuckySpin
{
    /// <summary>
    /// Service to contain lucky spin data
    /// </summary>
    internal class LuckySpinService : BaseClassService
    {
        /// <summary>
        /// Current user lucky spin data
        /// </summary>
        private LuckySpinData _cachedData;
        
        /// <summary>
        /// Current lucky spin config data
        /// </summary>
        private LuckySpinConfig _luckySpinConfig;

        public LuckySpinData CacheData => _cachedData;
        public LuckySpinConfig LuckySpinConfig => _luckySpinConfig;
        
        private NetworkClient _networkClient;
        
        /// <summary>
        /// Clear all cached data
        /// </summary>
        public override void ClearDataService()
        {
            _cachedData = _cachedData.SetDefault();
        }

        /// <summary>
        /// Call to init service. Must call before use any service's other functions
        /// </summary>
        public void InitService(NetworkClient networkClient)
        {
            _networkClient = networkClient;
            
            _cachedData = _cachedData.SetDefault();
            
            networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);

            if(networkClient.NetworkClientState == ThetanNetworkClientState.LoggedIn)
            {
                CallGetDataLuckySpin(null, null);
                CallGetLuckySpinConfig(null, null);
            }
        }

        private void OnDestroy()
        {
            ClearDataService();
            
            if(_networkClient != null)
                _networkClient.UnSubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
        }

        private void HandleOnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.LoggedIn)
            {
                CallGetLuckySpinConfig(null, null);
                CallGetDataLuckySpin(null, null);
            }
            else if(newState == ThetanNetworkClientState.NotLoggedIn ||
                    newState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                ClearDataService();
            }
        }

        /// <summary>
        /// Call server to get user lucky spin data
        /// </summary>
        public void CallGetDataLuckySpin(Action<LuckySpinData> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/spin")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI<LuckySpinData>(reqCommon, data =>
            {
                _cachedData = data;
                onSuccessCallback?.Invoke(data);
            }, onErrorCallback, AuthType.TOKEN);
        }

        /// <summary>
        /// Call server to get lucky spin config data
        /// </summary>
        public void CallGetLuckySpinConfig(Action<LuckySpinConfig> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/spin/config")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI<LuckySpinConfig>(reqCommon, data =>
            {
                _luckySpinConfig = data;
                onSuccessCallback?.Invoke(data);
            }, onErrorCallback, AuthType.TOKEN);
        }
    }
}