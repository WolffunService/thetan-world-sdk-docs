using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.RestAPI;

/// <summary>
/// Struct that implement endpoint setting that contain url to common api endpoint
/// </summary>
[Serializable]
public struct StructWolffunEndpointSetting : IWolffunEndpointSetting
{
    private static StructWolffunEndpointSetting StagingEndpointSetting = new StructWolffunEndpointSetting(
        "https://thetan-support.staging.thetanarena.com/api/remote-config/view",
        "https://thetan-auth.staging.thetanarena.com/api/v1",
        "",
        "",
        "https://analytic.staging.thetanarena.com/analytic/v1",
        "",
        "https://storage.googleapis.com/thetan-staging-asset",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        0,
        "",
        "",
        "",
        "",
        "https://data.staging.thetanworld.com/api/v1",
        "wss://thetan-auth.staging.thetanarena.com/ws",
        "https://marketplace.staging.thetanworld.com"
    );
    
    private static StructWolffunEndpointSetting ProdEndpointSetting = new StructWolffunEndpointSetting(
        "https://thetan-support.thetanarena.com/api/remote-config/view",
        "https://thetan-auth.thetanarena.com/api/v1",
        "",
        "",
        "https://analytic.thetanarena.com/analytic/v1",
        "",
        "https://storage.googleapis.com/thetan-staging-asset",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        0,
        "",
        "",
        "",
        "",
        "https://data.thetanworld.com/api/v1",
        "wss://thetan-auth.thetanarena.com/ws",
        "https://marketplace.thetanworld.com"
    );
    
    public static StructWolffunEndpointSetting GetDefaultEndpointSetting(bool isUseTemporaryVersion = false)
    {
#if STAGING || BPF
        return StagingEndpointSetting;
#endif
        if (isUseTemporaryVersion)
        {
            return StagingEndpointSetting;
        }
        return ProdEndpointSetting;
    }
    
    public StructWolffunEndpointSetting(
        string remoteServiceURL, 
        string authServiceURL,
        string dataServiceURL, 
        string verifyServiceURL, 
        string analyticServiceURL, 
        string leaderBoardServiceURL, 
        string storageServiceURL, 
        string thetanRivalsServiceURL, 
        string immortalServiceURL, 
        string thetanCreatorServiceURL, 
        string thetanSupportServiceURL, 
        string thetanNewsServiceURL, 
        string thetanMatchServiceURL, 
        string ipLobby, 
        ushort portLobby, 
        string webSocketURL, 
        string restApiwsurl, 
        string matchRivalServiceURL, 
        string thetanImmortalWebSocket,
        string thetanWorldURL,
        string authServiceWsURL,
        string marketplaceURL)
    {
        _remoteServiceURL = remoteServiceURL;
        _authServiceURL = authServiceURL;
        _authServiceWsUrl = authServiceWsURL;
        _dataServiceURL = dataServiceURL;
        _verifyServiceURL = verifyServiceURL;
        _analyticServiceURL = analyticServiceURL;
        _leaderBoardServiceURL = leaderBoardServiceURL;
        _storageServiceURL = storageServiceURL;
        _thetanRivalsServiceURL = thetanRivalsServiceURL;
        _immortalServiceURL = immortalServiceURL;
        _thetanCreatorServiceURL = thetanCreatorServiceURL;
        _thetanSupportServiceURL = thetanSupportServiceURL;
        _thetanNewsServiceURL = thetanNewsServiceURL;
        _thetanMatchServiceURL = thetanMatchServiceURL;
        _ipLobby = ipLobby;
        _portLobby = portLobby;
        _webSocketURL = webSocketURL;
        _restAPIWSURL = restApiwsurl;
        _matchRivalServiceURL = matchRivalServiceURL;
        _thetanImmortalWebSocket = thetanImmortalWebSocket;
        _thetanWorldURL = thetanWorldURL;
        _marketPlaceURL = marketplaceURL;
    }

    public string RemoteServiceURL => _remoteServiceURL;
    public string AuthServiceURL => _authServiceURL;
    public string AuthServiceWsURL => _authServiceWsUrl;
    public string DataServiceURL => _dataServiceURL;
    public string VerifyServiceURL => _verifyServiceURL;
    public string AnalyticServiceURL => _analyticServiceURL;
    public string LeaderBoardServiceURL => _leaderBoardServiceURL;
    public string StorageServiceURL => _storageServiceURL;
    public string ThetanRivalsServiceURL => _thetanRivalsServiceURL;
    public string ImmortalServiceURL => _immortalServiceURL;
    public string ThetanCreatorServiceURL => _thetanCreatorServiceURL;
    public string ThetanSupportServiceURL => _thetanSupportServiceURL;
    public string ThetanNewsServiceURL => _thetanNewsServiceURL;
    public string ThetanMatchServiceURL => _thetanMatchServiceURL;
    public string IpLobby => _ipLobby;
    public ushort PortLobby => _portLobby;
    public string WebSocketURL => _webSocketURL;
    public string RestAPIWSURL => _restAPIWSURL;
    public string MatchRivalServiceURL => _matchRivalServiceURL;
    public string ThetanImmortalWebSocket => _thetanImmortalWebSocket;
    public string ThetanWorldURL => _thetanWorldURL;
    public string MarketPlaceURL => _marketPlaceURL;
    
    #region field
    public string _remoteServiceURL;
    public string _authServiceURL;
    public string _authServiceWsUrl;
    public string _dataServiceURL;
    public string _verifyServiceURL;
    public string _analyticServiceURL;
    public string _leaderBoardServiceURL;
    public string _storageServiceURL;
    public string _thetanRivalsServiceURL;
    public string _immortalServiceURL;
    public string _thetanCreatorServiceURL;
    public string _thetanSupportServiceURL;
    public string _thetanNewsServiceURL;
    public string _thetanMatchServiceURL;
    public string _ipLobby;
    public ushort _portLobby;
    public string _webSocketURL;
    public string _restAPIWSURL;
    public string _matchRivalServiceURL;
    public string _thetanImmortalWebSocket;
    public string _thetanWorldURL;
    public string _marketPlaceURL;

    #endregion
}
