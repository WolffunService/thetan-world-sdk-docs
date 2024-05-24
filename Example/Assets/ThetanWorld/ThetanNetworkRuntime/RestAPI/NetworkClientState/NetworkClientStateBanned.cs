using System;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI.NetworkClientState
{
    /// <summary>
    /// Client state used when user use account that has been banned
    /// </summary>
    internal class NetworkClientStateBanned : BaseNetworkClientState, IAuthenSuccessListener
    {
        private NetworkClientStateNotLoggedIn _stateNotLoggedIn;
        private NetworkClientStateLoggedIn _stateLoggedIn;
            
        public NetworkClientStateBanned(
            NetworkReachabilityDetect networkReachabilityDetect, 
            NetworkClient networkClient, 
            Action<BaseNetworkClientState, BaseNetworkClientState> requestChangeStateCallback) 
            : base(networkReachabilityDetect, networkClient, requestChangeStateCallback)
        {
        }

        public void Initialize(
            NetworkClientStateNotLoggedIn stateNotLoggedIn,
            NetworkClientStateLoggedIn stateLoggedIn)
        {
            _stateNotLoggedIn = stateNotLoggedIn;
            _stateLoggedIn = stateLoggedIn;
        }

        public override void OnUserRequestLoggout()
        {
            base.OnUserRequestLoggout();

            RequestChangeState(_stateNotLoggedIn);
        }

        public override ThetanNetworkClientState ClientState => ThetanNetworkClientState.Banned;
        public void HandleAuthenSuccess(string accessToken, string refreshToken)
        {
            RequestChangeState(_stateLoggedIn);
        }
    }
}