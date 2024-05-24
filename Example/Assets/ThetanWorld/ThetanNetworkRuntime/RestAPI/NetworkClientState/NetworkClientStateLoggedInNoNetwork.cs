using System;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI.NetworkClientState
{
    /// <summary>
    /// Client state used when user has logged in but network connection is temporary not available
    /// </summary>
    internal class NetworkClientStateLoggedInNoNetwork : BaseNetworkClientState
    {
        private IAuthenticationContainer _authenContainer;
        private NetworkClientStateBanned _stateBanned;
        private NetworkClientStateLoggedIn _stateLoggedIn;
        private NetworkClientStateNotLoggedIn _stateNotLoggedIn;
        private NetworkClientStateNotLoggedInNoNetwork _stateNotLoggedInNoNetwork;
        
        private DateTime _timeTokenExpire;

        private bool _isLoggingIn;
        
        public NetworkClientStateLoggedInNoNetwork(
            IAuthenticationContainer authenContainer,
            NetworkReachabilityDetect networkReachabilityDetect, 
            NetworkClient networkClient, 
            Action<BaseNetworkClientState, BaseNetworkClientState> requestChangeStateCallback) 
            : base(networkReachabilityDetect, networkClient, requestChangeStateCallback)
        {
            _authenContainer = authenContainer;
        }

        public void Initialize(
            NetworkClientStateBanned stateBanned,
            NetworkClientStateLoggedIn stateLoggedIn,
            NetworkClientStateNotLoggedIn stateNotLoggedIn,
            NetworkClientStateNotLoggedInNoNetwork stateNotLoggedInNoNetwork)
        {
            _stateBanned = stateBanned;
            _stateLoggedIn = stateLoggedIn;
            _stateNotLoggedIn = stateNotLoggedIn;
            _stateNotLoggedInNoNetwork = stateNotLoggedInNoNetwork;
        }

        public override void OnUpdateState()
        {
            base.OnUpdateState();

            if (_isLoggingIn)
                return;
            
            if (IsConnetedToNetwork())
            {
                LoginWithCachedAccessToken();
                return;
            }

            if (DateTime.UtcNow > _timeTokenExpire)
            {
                if (IsConnetedToNetwork())
                {
                    RequestChangeState(_stateNotLoggedIn);
                }
                else
                {
                    RequestChangeState(_stateNotLoggedInNoNetwork);
                }
            }
        }

        private void LoginWithCachedAccessToken()
        {
            _isLoggingIn = true;
            ThetaAuthAPI.LoginByToken((response =>
            {
                _isLoggingIn = false;
                RequestChangeState(_stateLoggedIn);
            }), (error =>
            {
                _isLoggingIn = false;
                if (error.Code == (int)WSErrorCode.UserBanned)
                {
                    RequestChangeState(_stateBanned);
                }
                else if (error.Code == (int)WSErrorCode.UnityHttpRequestNetworkError || 
                    !IsConnetedToNetwork())
                {
                    // In-case cannot log in because of network, we still consider this user have logged in from previous session
                    if (DateTime.UtcNow > _timeTokenExpire)
                        return;
                    else
                        RequestChangeState(_stateNotLoggedInNoNetwork);
                }
                else
                {
                    RequestChangeState(_stateNotLoggedIn);
                }
            }));
        }

        public override void OnUserBanned()
        {
            base.OnUserBanned();

            RequestChangeState(_stateBanned);
        }

        public override void OnUserRequestLoggout()
        {
            base.OnUserRequestLoggout();

            if (IsConnetedToNetwork())
            {
                RequestChangeState(_stateNotLoggedIn);
            }
            else
            {
                RequestChangeState(_stateNotLoggedInNoNetwork);
            }
        }

        public override void OnEnterState(BaseNetworkClientState prevState)
        {
            base.OnEnterState(prevState);
            _isLoggingIn = false;
            if (string.IsNullOrEmpty(_authenContainer.GetAccessToken()))
            {
                if (IsConnetedToNetwork())
                {
                    RequestChangeState(_stateNotLoggedIn);
                }
                else
                {
                    RequestChangeState(_stateNotLoggedInNoNetwork);
                }
            }
            
            _timeTokenExpire = JWT.JsonWebToken.GetTimeExpire(_authenContainer.GetAccessToken());
        }

        public override ThetanNetworkClientState ClientState => ThetanNetworkClientState.LoggedInNoNetwork;
    }
}