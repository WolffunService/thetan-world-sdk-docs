using System;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.RemoteConfig
{
    /// <summary>
    /// Service for manage Thetan World remote config
    /// </summary>
    public class RemoteConfigService : BaseClassService
    {
        /// <summary>
        /// Data remote config
        /// </summary>
        private ThetanWorldRemoteConfigModel _remoteConfig;
        public ThetanWorldRemoteConfigModel RemoteConfig => _remoteConfig;

        private NetworkClient _networkClient;
        
        /// <summary>
        /// Clear cached data
        /// </summary>
        public override void ClearDataService()
        {
            _remoteConfig = _remoteConfig.SetDefault();
        }

        /// <summary>
        /// Call to init service. Must call before use any service's other functions
        /// </summary>
        /// <param name="networkClient"></param>
        public void InitService(NetworkClient networkClient)
        {
            _networkClient = networkClient;
            
            networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
            if (networkClient.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;

            CallGetRemoteConfig();
        }

        private void OnDestroy()
        {
            if(_networkClient != null)
                _networkClient.UnSubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
        }

        private void HandleOnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.LoggedIn)
                CallGetRemoteConfig();
        }

        /// <summary>
        /// callback success hay chua
        /// - success thi khong can xu ly lai
        /// </summary>
        /// <param name="callback"></param>
        public void CallGetRemoteConfig(Action<bool> callback = null)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.RemoteServiceURL)
                .GetQuery(Utils.GetProperties(new
                {
                    name = "thetanworld.client",
                    env = WolffunUnityHttp.EnvironmentName,
                    type = "struct",
                }));
            WolffunUnityHttp.Instance.MakeAPI<ThetanWorldRemoteConfigModel>(reqCommon,
                data =>
                {
                    _remoteConfig = data;

                    string version = ThetanSDKManager.Instance.Version;
                    
#if STAGING
                    version = version.Replace("_S", string.Empty);
#endif
                    
                    if(data.listTemporaryVersion != null && data.listTemporaryVersion.Contains(version))
                    {
                        NetworkClient.SetTemporaryVersionV2(true);
                        callback?.Invoke(false);
                    }
                    else
                    {
                        callback?.Invoke(true);
                    }
                }, error =>
                {
                    callback?.Invoke(false);
                }, AuthType.TOKEN);
        }
    }
}