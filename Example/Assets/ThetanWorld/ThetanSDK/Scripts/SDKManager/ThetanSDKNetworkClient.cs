using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK
{
    public class ThetanSDKNetworkClient : NetworkClient
    {
        [SerializeField] private SDKAuthenContainer _authenContainer;
        [SerializeField] private ThetanNetworkConfig _defaultNetworkConfig;
        
        protected override IAuthenticationContainer GetAuthenticationContainer() => _authenContainer;

        protected override LogLevel GetInitLogLevel() => NetworkConfig.LogLevel;

        protected override IWolffunEndpointSetting EndpointSetting
        {
            get
            {
                if (NetworkConfig.IsUseCustomEndpoint)
                    return NetworkConfig.CustomEndpointSetting;
                
                return base.EndpointSetting;
            }
        }
    }
}

