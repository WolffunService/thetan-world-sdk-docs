using System;
using UnityEngine;
using Wolffun.Log;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI.NetworkClientState
{
    /// <summary>
    /// Client state used when user has logged in and has network connection
    /// </summary>
    internal class NetworkClientStateLoggedIn : BaseNetworkClientState, ITokenErrorAPIHandle, IAuthenSuccessListener
    {
        private int TOTAL_SECOND_REMAIN_TO_REFRESH_TOKEN = 7 * 60; // Refresh token if access token only have 7 minutes left
        private IAuthenticationContainer _authenContainer;

        private NetworkClientStateLoggedInNoNetwork _stateLoggedInNoNetwork;
        private NetworkClientStateNotLoggedIn _stateNotLoggedIn;
        private NetworkClientStateNotLoggedInNoNetwork _stateNotLoggedInNoNetwork;
        private NetworkClientStateBanned _stateBanned;

        private DateTime _timeTokenExpire;

        private bool _isRefreshingToken;
        
        public NetworkClientStateLoggedIn(
            IAuthenticationContainer authenContainer,
            NetworkReachabilityDetect networkReachabilityDetect, 
            NetworkClient networkClient, 
            Action<BaseNetworkClientState, BaseNetworkClientState> requestChangeStateCallback) 
            : base(networkReachabilityDetect, networkClient, requestChangeStateCallback)
        {
            _authenContainer = authenContainer;
        }

        public void Initialize(
            NetworkClientStateLoggedInNoNetwork stateLoggedInNoNetwork,
            NetworkClientStateNotLoggedIn stateNotLoggedIn,
            NetworkClientStateNotLoggedInNoNetwork stateNotLoggedInNoNetwork,
            NetworkClientStateBanned stateBanned)
        {
            _stateLoggedInNoNetwork = stateLoggedInNoNetwork;
            _stateNotLoggedIn = stateNotLoggedIn;
            _stateNotLoggedInNoNetwork = stateNotLoggedInNoNetwork;
            _stateBanned = stateBanned;
        }

        public override ThetanNetworkClientState ClientState => ThetanNetworkClientState.LoggedIn;

        public override void OnUpdateState()
        {
            base.OnUpdateState();

            if (_isRefreshingToken)
                return;
            
            if(string.IsNullOrEmpty(_authenContainer.GetAccessToken()))
            {
                if (IsConnetedToNetwork())
                    RequestChangeState(_stateNotLoggedIn);
                else
                    RequestChangeState(_stateNotLoggedInNoNetwork);
                return;
            }

            if (!IsConnetedToNetwork())
            {
                if (JWT.JsonWebToken.IsTokenExpire(_authenContainer.GetAccessToken(), 0) &&
                    JWT.JsonWebToken.IsTokenExpire(_authenContainer.GetRefreshToken(), 0))
                {
                    RequestChangeState(_stateNotLoggedInNoNetwork);
                }
                else
                {
                    RequestChangeState(_stateLoggedInNoNetwork);
                }
            }
            else
            {
                var utcNow = DateTime.UtcNow;
                if ((_timeTokenExpire - utcNow).TotalSeconds < TOTAL_SECOND_REMAIN_TO_REFRESH_TOKEN)
                {
                    RefreshToken();
                }
            }
        }

        public void HandleTokenError()
        {
            RefreshToken();
        }

        public void HandleTokenExpire()
        {
            RefreshToken();
        }

        public void HandleAccountBanned()
        {
        }

        private void RefreshToken()
        {
            _isRefreshingToken = true;
            ThetaAuthAPI.RefreshToken(new RefreshTokenRequestModel
                {
                    refreshToken = _authenContainer.GetRefreshToken()
                },
                (result) =>
                {
                    _isRefreshingToken = false;
                    _authenContainer.SaveAccessTokenToCache(result.accessToken, result.refreshToken);
                    _timeTokenExpire = JWT.JsonWebToken.GetTimeExpire(result.accessToken);
                },
                (error) =>
                {
                    _isRefreshingToken = false;

                    if (error.Code == (int)WSErrorCode.UserBanned)
                    {
                        RequestChangeState(_stateBanned);
                    }
                    else if (IsConnetedToNetwork() && 
                        (WSErrorCode)error.Code != WSErrorCode.UnityHttpRequestNetworkError)
                    {
                        RequestChangeState(_stateNotLoggedIn);
                    }
                    else
                    {
                        if (JWT.JsonWebToken.IsTokenExpire(_authenContainer.GetAccessToken(), 0) &&
                            JWT.JsonWebToken.IsTokenExpire(_authenContainer.GetRefreshToken(), 0))
                        {
                            _authenContainer.SaveAccessTokenToCache(string.Empty, string.Empty);
                            RequestChangeState(_stateNotLoggedInNoNetwork);
                        }
                        else
                        {
                            RequestChangeState(_stateLoggedInNoNetwork);
                        }
                    }
                });
        }

        public override void OnEnterState(BaseNetworkClientState prevState)
        {
            base.OnEnterState(prevState);

            _isRefreshingToken = false;
            
            if(string.IsNullOrEmpty(_authenContainer.GetAccessToken()))
            {
                if (IsConnetedToNetwork())
                    RequestChangeState(_stateNotLoggedIn);
                else
                    RequestChangeState(_stateNotLoggedInNoNetwork);
                return;
            }

            _timeTokenExpire = JWT.JsonWebToken.GetTimeExpire(_authenContainer.GetAccessToken());
        }

        public void HandleAuthenSuccess(string accessToken, string refreshToken)
        {
            _timeTokenExpire = JWT.JsonWebToken.GetTimeExpire(accessToken);
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
                RequestChangeState(_stateNotLoggedIn);
            else
                RequestChangeState(_stateNotLoggedInNoNetwork);
            return;
        }
    }
}