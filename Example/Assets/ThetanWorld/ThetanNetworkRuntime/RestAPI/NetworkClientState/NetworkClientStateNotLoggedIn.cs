using System;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI.NetworkClientState
{
    /// <summary>
    /// Client state used when user not logged in and has network connection
    /// </summary>
    internal class NetworkClientStateNotLoggedIn : BaseNetworkClientState, IAuthenSuccessListener
    {
        private NetworkClientStateNotLoggedInNoNetwork _stateNotLoggedInNoNetwork;
        private NetworkClientStateLoggedIn _stateLoggedIn;
        private IAuthenticationContainer _authenContainer;
            
        public NetworkClientStateNotLoggedIn(
            IAuthenticationContainer authenContainer,
            NetworkReachabilityDetect networkReachabilityDetect, 
            NetworkClient networkClient, 
            Action<BaseNetworkClientState, BaseNetworkClientState> requestChangeStateCallback) 
            : base(networkReachabilityDetect, networkClient, requestChangeStateCallback)
        {
            _authenContainer = authenContainer;
        }

        public void Initialize(
            NetworkClientStateNotLoggedInNoNetwork stateNotLoggedInNoNetwork,
            NetworkClientStateLoggedIn stateLoggedIn)
        {
            _stateNotLoggedInNoNetwork = stateNotLoggedInNoNetwork;
            _stateLoggedIn = stateLoggedIn;
        }

        public override void OnUpdateState()
        {
            base.OnUpdateState();

            if (!IsConnetedToNetwork())
            {
                RequestChangeState(_stateNotLoggedInNoNetwork);
                return;
            }
        }

        public override ThetanNetworkClientState ClientState => ThetanNetworkClientState.NotLoggedIn;
        public void HandleAuthenSuccess(string accessToken, string refreshToken)
        {
            RequestChangeState(_stateLoggedIn);
        }
    }
}