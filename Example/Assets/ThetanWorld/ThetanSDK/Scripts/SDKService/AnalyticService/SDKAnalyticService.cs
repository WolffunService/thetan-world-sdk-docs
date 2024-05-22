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
    internal partial class SDKAnalyticService : BaseClassService, IPostAuthenProcessor
    {
        private const string HASH_KEY = "*hi37*@W98CdZsjw";
        
        private Queue<SDKAnalyticRequestModel> _cachedFailAnalyticRequest = new Queue<SDKAnalyticRequestModel>();
        
        private ThetanNetworkConfig _networkConfig;
        
        private Dictionary<string, string> _dataLog = new Dictionary<string, string>();
        private float _countTime;
        private Dictionary<string, (OnChainInfo, NftIngameInfo)> _cachedNftIngameInfo = new Dictionary<string, (OnChainInfo, NftIngameInfo)>();

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

        public override void ClearDataService()
        {
            _cachedFailAnalyticRequest.Clear();
            _countTime = 0;
        }

        public async UniTask ProcessPostAuthenProcess(PostAuthenSuccessMetaData metaData)
        {
            var profileService = ThetanSDKManager.Instance.ProfileService;
            if (string.IsNullOrEmpty(profileService.UserId))
                await UniTask.WaitUntil(() => !string.IsNullOrEmpty(profileService.UserId));
            
            LogLoginSuccess(metaData);

            return;
        }
    }
}