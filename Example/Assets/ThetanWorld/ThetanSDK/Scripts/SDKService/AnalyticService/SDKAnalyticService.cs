using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ThetanSDK.SDKService;
using ThetanSDK.Utilities;
using UnityEngine;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.SDKServices.Analytic
{
    /// <summary>
    /// Service for sending analytic info
    /// </summary>
    internal partial class SDKAnalyticService : BaseClassService, IPostAuthenProcessor
    {
        /// <summary>
        /// Hashkey for api analytic
        /// </summary>
        private const string HASH_KEY = "*hi37*@W98CdZsjw";
        
        // List failed analytic request that need to retry
        private Queue<SDKAnalyticRequestModel> _cachedFailAnalyticRequest = new Queue<SDKAnalyticRequestModel>();
        
        private ThetanNetworkConfig _networkConfig;
        
        private Dictionary<string, string> _dataLog = new Dictionary<string, string>();
        
        /// <summary>
        /// Dictionary for caching nft info used for analytic. This reduce api call when need hero nft data for analytic
        /// These info rarely change so don't need refresh cache behavior
        /// </summary>
        private Dictionary<string, (OnChainInfo, NftIngameInfo)> _cachedNftIngameInfo = new Dictionary<string, (OnChainInfo, NftIngameInfo)>();

        /// <summary>
        /// Count time to retry send failed analytic request before, interval 1 second.
        /// </summary>
        private float _countTime;
        
        public void InitialzeService(AuthenProcessContainer authenProcessContainer, ThetanNetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            
            authenProcessContainer.WFIDAuthenProcess.RegisterPostAuthenProcessor(this);
            authenProcessContainer.ThetanAppAuthenProcess.RegisterPostAuthenProcessor(this);
            
        }
        
        private void FixedUpdate()
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;
            
            _countTime += Time.fixedDeltaTime;

            if (_countTime > 1)
            {
                MyUpdate();
                _countTime = 0;
            }
        }

        private void MyUpdate()
        {
            if (_cachedFailAnalyticRequest == null || _cachedFailAnalyticRequest.Count == 0)
                return;

            var prevFailRequest = _cachedFailAnalyticRequest.Dequeue();

            LogEvent(prevFailRequest);
        }

        // Log event analytic
        private void LogEvent(string eventName, Dictionary<string, string> data)
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;
            
            var profileService = ThetanSDKManager.Instance.ProfileService;
            
            var requestData = new SDKAnalyticRequestModel();
            requestData.eventName = eventName;
            requestData.timeStamp = ThetanSDKUtilities.GetLocalTimestamp();
            requestData.userId = profileService.UserId;
            requestData.country = profileService.UserCountry;
            requestData.platform = GetAnalyticPlatform();
            requestData.deviceOS = GetAnalyticPlatform();

            if (data != null)
            {
                requestData.eventParams = new AnalyticPCParamModel[data.Count];

                int index = 0;
                foreach (var param in data)
                {
                    requestData.eventParams[index] = new AnalyticPCParamModel()
                    {
                        key = param.Key,
                        value = param.Value
                    };
                    index++;
                }
            }

            using (var hashValueBuilder = ZString.CreateUtf8StringBuilder())
            {
                hashValueBuilder.Append(requestData.timeStamp);


                foreach (var eventData in data)
                {
                    string eventValue = eventData.Value == null ? string.Empty : Convert.ToString(eventData.Value,CultureInfo.InvariantCulture);
                    hashValueBuilder.Append(eventValue);
                }

                requestData.thetan = ThetanSDKUtilities.Hash(hashValueBuilder.ToString(), HASH_KEY);
            }
            
            LogEvent(requestData);
        }

        private void LogEvent(SDKAnalyticRequestModel requestData)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AnalyticServiceURL + "/rival/analyze")
                .Post(WolffunRequestBody.From(requestData));
            WolffunUnityHttp.Instance.MakeAPI<object>(reqCommon, _ => {}, _ =>
            {
                _cachedFailAnalyticRequest.Enqueue(requestData);
            }, AuthType.TOKEN);
        }

        private string GetAnalyticPlatform()
        {
#if UNITY_EDITOR
            return "Editor";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "Ios";
#elif UNITY_STANDALONE_WIN
            return "Window";
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Get hero nft info for analytic.
        /// This will first try get from cache, if fail, it will call server and cache response
        /// </summary>
        private async UniTask<(OnChainInfo, NftIngameInfo)> GetAndCacheNFTIngameInfo(string heroNftId)
        {
            if (_cachedNftIngameInfo.TryGetValue(heroNftId, out var cachedData))
            {
                return cachedData;
            }
            
            var nftData = await ThetanSDKManager.Instance.GetHeroNftItemInfo(heroNftId);
            _cachedNftIngameInfo[heroNftId] = (nftData.onchainInfo, nftData.ingameInfo);

            return (nftData.onchainInfo, nftData.ingameInfo);
        }

        /// <summary>
        /// Clear cached data
        /// </summary>
        public override void ClearDataService()
        {
            _cachedFailAnalyticRequest.Clear();
            _countTime = 0;
        }

        /// <summary>
        /// Called by authen processor.
        /// Call analytic login success after user logged in
        /// </summary>
        public async UniTask ProcessPostAuthenProcess(PostAuthenSuccessMetaData metaData)
        {
            await UniTask.DelayFrame(1);
            
            var profileService = ThetanSDKManager.Instance.ProfileService;
            if (string.IsNullOrEmpty(profileService.UserId))
                await UniTask.WaitUntil(() => !string.IsNullOrEmpty(profileService.UserId));
            
            LogLoginSuccess(metaData);

            return;
        }
    }
}