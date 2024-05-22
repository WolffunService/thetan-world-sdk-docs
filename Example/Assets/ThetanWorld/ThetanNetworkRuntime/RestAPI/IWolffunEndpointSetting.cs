using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wolffun.RestAPI
{
    public interface IWolffunEndpointSetting
    {
        public string RemoteServiceURL { get; }
        public string AuthServiceURL { get; }
        public string AuthServiceWsURL { get; }
        public string DataServiceURL { get; }
        public string VerifyServiceURL { get; }
        public string AnalyticServiceURL { get; }
        public string LeaderBoardServiceURL { get; }
        public string StorageServiceURL { get; }
        public string ThetanRivalsServiceURL { get; }
        public string ImmortalServiceURL { get; }
        public string ThetanCreatorServiceURL { get; }
        public string ThetanSupportServiceURL { get; }
        public string ThetanNewsServiceURL { get; }
        public string ThetanMatchServiceURL { get; }
        public string IpLobby { get; }
        public ushort PortLobby { get; }
        public string WebSocketURL { get; }
        public string RestAPIWSURL { get; }
        public string MatchRivalServiceURL { get; }
        public string ThetanImmortalWebSocket { get; }
        public string ThetanWorldURL { get; }
        
        public string MarketPlaceURL { get; }
    }
}

