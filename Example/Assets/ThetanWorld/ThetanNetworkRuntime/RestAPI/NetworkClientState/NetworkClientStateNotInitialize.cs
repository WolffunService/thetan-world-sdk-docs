using System;
using UnityEngine;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI.NetworkClientState
{
    /// <summary>
    /// Client state used when client app is not initialized yet
    /// </summary>
    internal class NetworkClientStateNotInitialize : BaseNetworkClientState
    {
        /// <summary>
        /// Total second remain on token to start refresh token
        /// </summary>
        private const int REFRESH_TOKEN_WHEN_REMAINING = 1 * 24 * 3600;
        
        private NetworkClientStateLoggedIn _stateLoggedIn;
        private NetworkClientStateLoggedInNoNetwork _stateLoggedInNoNetwork;
        private NetworkClientStateNotLoggedIn _stateNotLoggedIn;
        private NetworkClientStateNotLoggedInNoNetwork _stateNotLoggedInNoNetwork;
        private NetworkClientStateBanned _stateBanned;
        
        public NetworkClientStateNotInitialize(
            NetworkReachabilityDetect networkReachabilityDetect,
            NetworkClient networkClient, 
            Action<BaseNetworkClientState, BaseNetworkClientState> requestChangeStateCallback) 
            : base(networkReachabilityDetect, networkClient, requestChangeStateCallback)
        {
        }
        
        public void Initialize(
            NetworkClientStateLoggedIn stateLoggedIn,
            NetworkClientStateLoggedInNoNetwork stateLoggedInNoNetwork,
            NetworkClientStateNotLoggedIn stateNotLoggedIn,
            NetworkClientStateNotLoggedInNoNetwork stateNotLoggedInNoNetwork,
            NetworkClientStateBanned stateBanned)
        {
            _stateLoggedIn = stateLoggedIn;
            _stateLoggedInNoNetwork = stateLoggedInNoNetwork;
            _stateNotLoggedIn = stateNotLoggedIn;
            _stateNotLoggedInNoNetwork = stateNotLoggedInNoNetwork;
            _stateBanned = stateBanned;
        }

        public void StartProcess(IAuthenticationContainer authenContainer, Action<ThetanNetworkClientState> onDoneCallback)
        {
            var cachedAccessToken = authenContainer.GetAccessToken();

            if (string.IsNullOrEmpty(cachedAccessToken))
            {
                if (IsConnetedToNetwork())
                {
                    DoneInitialize(_stateNotLoggedIn, onDoneCallback);
                }
                else
                {
                    DoneInitialize(_stateNotLoggedInNoNetwork, onDoneCallback);
                }
                return;
            }

            
            if (JWT.JsonWebToken.IsTokenExpire(cachedAccessToken, 30))
            {
                bool isRefreshTokenExpired =
                    JWT.JsonWebToken.IsTokenExpire(authenContainer.GetRefreshToken(), REFRESH_TOKEN_WHEN_REMAINING);

                if (isRefreshTokenExpired)
                {
                    if (IsConnetedToNetwork())
                    {
                        DoneInitialize(_stateNotLoggedIn, onDoneCallback);
                    }
                    else
                    {
                        DoneInitialize(_stateNotLoggedInNoNetwork, onDoneCallback);
                    }
                }
                else
                {
                    if (IsConnetedToNetwork())
                    {
                        RefreshToken(authenContainer, onDoneCallback);
                    }
                    else
                    {
                        DoneInitialize(_stateLoggedInNoNetwork, onDoneCallback);
                    }
                }
                return;
            }

            if (IsConnetedToNetwork())
                LoginWithCachedAccessToken(authenContainer, onDoneCallback);
            else
                DoneInitialize(_stateLoggedInNoNetwork, onDoneCallback);
        }

        private void RefreshToken(IAuthenticationContainer authenContainer, Action<ThetanNetworkClientState> onDoneCallback)
        {
            ThetaAuthAPI.RefreshToken(new RefreshTokenRequestModel
                {
                    refreshToken = authenContainer.GetRefreshToken()
                },
                (result) =>
                {
                    authenContainer.SaveAccessTokenToCache(result.accessToken, result.refreshToken);
                    
                    DoneInitialize(_stateLoggedIn, onDoneCallback);
                },
                (error) =>
                {
                    if ((WSErrorCode)error.Code == WSErrorCode.UserBanned)
                    {
                        DoneInitialize(_stateBanned, onDoneCallback);
                    }
                    else if (IsConnetedToNetwork() && 
                        (WSErrorCode)error.Code != WSErrorCode.UnityHttpRequestNetworkError)
                    {
                        DoneInitialize(_stateNotLoggedIn, onDoneCallback);
                    }
                    else
                    {
                        DoneInitialize(_stateNotLoggedInNoNetwork, onDoneCallback);
                    }
                });
        }

        private void LoginWithCachedAccessToken(IAuthenticationContainer authenticationContainer,
            Action<ThetanNetworkClientState> onDoneCallback)
        {
            ThetaAuthAPI.LoginByToken((response =>
            {
                DoneInitialize(_stateLoggedIn, onDoneCallback);
            }), (error =>
            {
                if (error.Code == (int)WSErrorCode.UserBanned)
                {
                    DoneInitialize(_stateBanned, onDoneCallback);
                }
                else if (error.Code == (int)WSErrorCode.UnityHttpRequestNetworkError || 
                    !IsConnetedToNetwork())
                {
                    // In-case cannot log in because of network, we still consider this user have logged in from previous session
                    DoneInitialize(_stateLoggedInNoNetwork, onDoneCallback);
                }
                else if (error.Code == (int)WSErrorCode.TokenExpired)
                {
                    RefreshToken(authenticationContainer, onDoneCallback);
                }
                else
                {
                    DoneInitialize(_stateNotLoggedIn, onDoneCallback);
                }
            }));
        }

        private void DoneInitialize(BaseNetworkClientState networkClientState,
            Action<ThetanNetworkClientState> onDoneCallback)
        {
            RequestChangeState(networkClientState);
            onDoneCallback?.Invoke(networkClientState.ClientState);
        }
        
        public override ThetanNetworkClientState ClientState => ThetanNetworkClientState.NotInitialized;
    }
}