using System;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI.NetworkClientState
{
    // Todo: implement me
    internal class NetworkClientStateNotLoggedInNoNetwork : BaseNetworkClientState, IAuthenSuccessListener
    {
        private NetworkClientStateNotLoggedIn _stateNotLoggedIn;
        private NetworkClientStateLoggedIn _stateLoggedIn;
        private IAuthenticationContainer _authenContainer;
        
        public NetworkClientStateNotLoggedInNoNetwork(
            IAuthenticationContainer authenContainer,
            NetworkReachabilityDetect networkReachabilityDetect, 
            NetworkClient networkClient, 
            Action<BaseNetworkClientState, BaseNetworkClientState> requestChangeStateCallback) 
            : base(networkReachabilityDetect, networkClient, requestChangeStateCallback)
        {
            _authenContainer = authenContainer;
        }
        
        public void Initialize(
            NetworkClientStateNotLoggedIn stateNotLoggedIn,
            NetworkClientStateLoggedIn stateLoggedIn)
        {
            _stateNotLoggedIn = stateNotLoggedIn;
            _stateLoggedIn = stateLoggedIn;
        }

        public override void OnUpdateState()
        {
            base.OnUpdateState();

            if (IsConnetedToNetwork())
            {
                RequestChangeState(_stateNotLoggedIn);
                return;
            }
        }

        public override ThetanNetworkClientState ClientState => ThetanNetworkClientState.NotLoggedInNoNetwork;
        public void HandleAuthenSuccess(string accessToken, string refreshToken)
        {
            RequestChangeState(_stateLoggedIn);
        }
    }
}