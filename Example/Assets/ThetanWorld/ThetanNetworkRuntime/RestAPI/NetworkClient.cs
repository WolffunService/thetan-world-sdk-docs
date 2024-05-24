using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wolffun.Log;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI;
using Wolffun.RestAPI.NetworkClientState;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI
{
    /// <summary>
    /// An enum descript current client state, can use this to determine whether is user logged in, or is connected to network?
    /// </summary>
    public enum ThetanNetworkClientState
    {
        /// <summary>Client is not initialized yet, must call Initialize </summary>
        NotInitialized = 0,
        
        /// <summary>User is not logged in </summary>
        NotLoggedIn = 1,
        
        /// <summary>User is not logged in and not connected to network</summary>
        NotLoggedInNoNetwork = 2,
        
        /// <summary>User is logged in</summary>
        LoggedIn = 3,
        
        /// <summary>User is logged in but temporary not connected to network</summary>
        LoggedInNoNetwork = 4,
        
        /// <summary>User is banned </summary>
        Banned = 5,
    }

    /// <summary>
    /// Base network client that manager client network state
    /// </summary>
    public abstract class NetworkClient : MonoBehaviour, ITokenErrorAPIHandle, IAuthenSuccessListener
    {
        [SerializeField] private NetworkReachabilityDetect _networkReachabilityDetect;
        
        private IAuthenticationContainer _authenContainer;

        private Action<ThetanNetworkClientState> _onChangeNetworkClientState;

        protected static bool _isUseTemporaryVersion = false;
        
        //[NonSerialized] private StructWolffunEndpointSetting _defaultEndpointSetting =
         //   StructWolffunEndpointSetting.GetDefaultEndpointSetting(_isUseTemporaryVersion);
        
        private ThetanNetworkConfig _networkConfig;

        private bool _isAwaked = false;

        private List<BaseNetworkClientState> _listAvailableNetworkClientStates;
        private BaseNetworkClientState _currentNetworkClientState;
        private float _countTimeUpdateNetworkState;
        private int NETWORK_STATE_UPDATE_INTERVAL_SECOND = 1;

        public ThetanNetworkClientState NetworkClientState
        {
            get => _currentNetworkClientState != null ? _currentNetworkClientState.ClientState : ThetanNetworkClientState.NotInitialized;
        }

        public ThetanNetworkConfig NetworkConfig => _networkConfig;

        public string StorageResourceUrl => EndpointSetting.StorageServiceURL;
        
        protected virtual void Awake()
        {
            _networkConfig = Resources.Load<ThetanNetworkConfig>("ThetanSDKNetworkConfig");

            if (_networkConfig == null)
            {
                Debug.LogError(
                    "Cannot find ThetanSDKNetworkConfig under any Resources folder. " +
                    "Please go to Tools/Wolffun/CreateNetworkConfig to create network config");
            }
            
            _isAwaked = true;
        }

        protected virtual void FixedUpdate()
        {
            if (_currentNetworkClientState == null)
                return;

            _countTimeUpdateNetworkState += Time.fixedDeltaTime;

            if (_countTimeUpdateNetworkState >= NETWORK_STATE_UPDATE_INTERVAL_SECOND)
            {
                _currentNetworkClientState.OnUpdateState();
                _countTimeUpdateNetworkState = 0;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_listAvailableNetworkClientStates != null)
            {
                foreach (var state in _listAvailableNetworkClientStates)
                {
                    state.Dispose();
                }
                _listAvailableNetworkClientStates.Clear();
            }
        }

        public async void InitializeNetworkClient(string version, Action<ThetanNetworkClientState> doneInitializeNetworkCallback)
        {
            if (!_isAwaked)
            {
                await UniTask.WaitUntil(() => _isAwaked);
            }

            _authenContainer = GetAuthenticationContainer();
            _authenContainer.SetAppClientIdAndSecret(_networkConfig.ApplicationID, _networkConfig.ApplicationSecret);

            await _authenContainer.LoadCachedAccessToken();

            if(!WolffunUnityHttp.IsAlive)
                await UniTask.WaitUntil(() => WolffunUnityHttp.IsAlive);

            WolffunUnityHttp.Instance.Initialize(version, _authenContainer, EndpointSetting, GetInitLogLevel(),
                GetDefaultHandleSpecialError(), tokenErrorAPIHandle: this);

            CreateAllClientStateAndStartStateProcess(doneInitializeNetworkCallback);

            //CheckTokenExpired_Internal(doneInitializeNetworkCallback);
        }
        
        private void CreateAllClientStateAndStartStateProcess(Action<ThetanNetworkClientState> doneCallback)
        {
            if (_listAvailableNetworkClientStates != null)
            {
                foreach (var state in _listAvailableNetworkClientStates)
                {
                    state.Dispose();
                }
                _listAvailableNetworkClientStates.Clear();
            }
            
            NetworkClientStateNotInitialize stateNotInitialize = new NetworkClientStateNotInitialize(
                _networkReachabilityDetect,
                this, HandleRequestChangeState);
            NetworkClientStateLoggedIn stateLoggedIn = new NetworkClientStateLoggedIn(
                _authenContainer,
                _networkReachabilityDetect,
                this, HandleRequestChangeState);
            NetworkClientStateLoggedInNoNetwork stateLoggedInNoNetwork = new NetworkClientStateLoggedInNoNetwork(
                _authenContainer,
                _networkReachabilityDetect,
                this, HandleRequestChangeState);
            NetworkClientStateNotLoggedIn stateNotLoggedIn = new NetworkClientStateNotLoggedIn(
                _authenContainer,
                _networkReachabilityDetect,
                this, HandleRequestChangeState);
            NetworkClientStateNotLoggedInNoNetwork stateNotLoggedInNoNetwork = new NetworkClientStateNotLoggedInNoNetwork(
                _authenContainer,
                _networkReachabilityDetect,
                this, HandleRequestChangeState);
            NetworkClientStateBanned stateBanned = new NetworkClientStateBanned(
                _networkReachabilityDetect,
                this, HandleRequestChangeState);

            _listAvailableNetworkClientStates = new List<BaseNetworkClientState>();
            _listAvailableNetworkClientStates.Add(stateNotInitialize);
            _listAvailableNetworkClientStates.Add(stateLoggedIn);
            _listAvailableNetworkClientStates.Add(stateLoggedInNoNetwork);
            _listAvailableNetworkClientStates.Add(stateNotLoggedIn);
            _listAvailableNetworkClientStates.Add(stateNotLoggedInNoNetwork);
            _listAvailableNetworkClientStates.Add(stateBanned);
            
            stateNotInitialize.Initialize(stateLoggedIn, stateLoggedInNoNetwork, stateNotLoggedIn, stateNotLoggedInNoNetwork, stateBanned);
            stateLoggedIn.Initialize(stateLoggedInNoNetwork, stateNotLoggedIn, stateNotLoggedInNoNetwork, stateBanned);
            stateLoggedInNoNetwork.Initialize(stateBanned, stateLoggedIn, stateNotLoggedIn, stateNotLoggedInNoNetwork);
            stateNotLoggedIn.Initialize(stateNotLoggedInNoNetwork, stateLoggedIn);
            stateNotLoggedInNoNetwork.Initialize(stateNotLoggedIn, stateLoggedIn);
            stateBanned.Initialize(stateNotLoggedIn, stateLoggedIn);

            _currentNetworkClientState = stateNotInitialize;
            stateNotInitialize.StartProcess(_authenContainer, doneCallback);
        }

        private void HandleRequestChangeState(BaseNetworkClientState prevState, BaseNetworkClientState nextState)
        {
            CommonLog.Log($"HandleRequestChangeState from {prevState.ClientState} to {nextState.ClientState}");
            _countTimeUpdateNetworkState = 0;
            _currentNetworkClientState = nextState;
            prevState.OnExitState(nextState);
            nextState.OnEnterState(prevState);
            _onChangeNetworkClientState?.Invoke(nextState.ClientState);
        }

        public void LogOut()
        {
            _authenContainer.SaveAccessTokenToCache(string.Empty, string.Empty);
            
            if(_currentNetworkClientState != null)
                _currentNetworkClientState.OnUserRequestLoggout();
        }

        /*
        public void RecheckTokenExpired(Action<ThetanNetworkClientState> onDoneCallback)
        {
            CheckTokenExpired_Internal(onDoneCallback);
        }
        */

        #region Initialize WolffunUnityHttp

        protected virtual IWolffunHandleSpecialError GetDefaultHandleSpecialError() => null;

        protected virtual LogLevel GetInitLogLevel() => LogLevel.Error;

        protected abstract IAuthenticationContainer GetAuthenticationContainer();

        protected virtual IWolffunEndpointSetting EndpointSetting
        {
            get
            {
                if (_networkConfig.IsUseCustomEndpoint)
                    return _networkConfig.CustomEndpointSetting;
                
                return StructWolffunEndpointSetting.GetDefaultEndpointSetting(_isUseTemporaryVersion);
            }
        }

        public string GetAccessToken()
        {
            return _authenContainer == null? string.Empty : _authenContainer.GetAccessToken();
        }
        #endregion

        #region Auth Functions

        /*
        protected void CheckTokenExpired_Internal(Action<ThetanNetworkClientState> onDoneCallback)
        {
            var cachedAccessToken = _authenContainer.GetAccessToken();

            if (string.IsNullOrEmpty(cachedAccessToken))
            {
                NetworkClientState = ThetanNetworkClientState.NotLoggedIn;
                onDoneCallback?.Invoke(_networkClientState);
                return;
            }

            if (Wolffun.RestAPI.JWT.JsonWebToken.IsTokenExpire(cachedAccessToken, REFRESH_TOKEN_WHEN_REMAINING))
            {
                NetworkClientState = ThetanNetworkClientState.TokenExpired;
                RefreshToken(onDoneCallback);
                return;
            }

            LoginWithCachedAccessToken(onDoneCallback);
        }

        protected void LoginWithCachedAccessToken(Action<ThetanNetworkClientState> onDoneCallback)
        {
            NetworkClientState = ThetanNetworkClientState.LoggingIn;
            
            ThetaAuthAPI.LoginByToken((response =>
            {
                NetworkClientState = ThetanNetworkClientState.LoggedIn;
                onDoneCallback?.Invoke(NetworkClientState);
            }), (error =>
            {
                if (error.Code == (int)WSErrorCode.TokenExpired)
                    RefreshToken(onDoneCallback);
                else
                {
                    NetworkClientState = ThetanNetworkClientState.NotLoggedIn;
                    onDoneCallback?.Invoke(NetworkClientState);
                }
            }));
        }

        protected void RefreshToken(Action<ThetanNetworkClientState> onDoneCallback)
        {
            NetworkClientState = ThetanNetworkClientState.RefreshingExpiredToken;
            ThetaAuthAPI.RefreshToken(new RefreshTokenRequestModel
                {
                    refreshToken = _authenContainer.GetRefreshToken()
                },
                (result) =>
                {
                    _authenContainer.SaveAccessTokenToCache(result.accessToken, result.refreshToken);
                    NetworkClientState = ThetanNetworkClientState.LoggedIn;
                    onDoneCallback?.Invoke(NetworkClientState);
                },
                (error) =>
                {
                    NetworkClientState = ThetanNetworkClientState.NotLoggedIn;
                    onDoneCallback?.Invoke(NetworkClientState);
                });
        }
        */

        #endregion
        
        #region Event Subcribe Functions

        public void SubcribeOnChangeNetworkClientState(Action<ThetanNetworkClientState> callback)
        {
            _onChangeNetworkClientState += callback;
        }

        public void UnSubcribeOnChangeNetworkClientState(Action<ThetanNetworkClientState> callback)
        {
            _onChangeNetworkClientState -= callback;
        }

        #endregion

        #region Handle Token Error when call API
        
        public void HandleTokenError()
        {
            if (_currentNetworkClientState != null &&
                _currentNetworkClientState is ITokenErrorAPIHandle tokenErrorAPIHandle)
            {
                tokenErrorAPIHandle.HandleTokenError();
            }
        }

        public void HandleTokenExpire()
        {
            if (_currentNetworkClientState is ITokenErrorAPIHandle tokenErrorAPIHandle)
            {
                tokenErrorAPIHandle.HandleTokenExpire();
            }
        }

        public void HandleAccountBanned()
        {
            _currentNetworkClientState.OnUserBanned();
        }
        
        #endregion

        #region Authen Process
        
        public void HandleAuthenSuccess(string accessToken, string refreshToken)
        {
            _authenContainer.SaveAccessTokenToCache(accessToken, refreshToken);
            if (_currentNetworkClientState != null &&
                _currentNetworkClientState is IAuthenSuccessListener authenSuccessListener)
            {
                authenSuccessListener.HandleAuthenSuccess(accessToken, refreshToken);
            }
        }

        public void RegisterAuthenProcess(IAuthenSuccessCallback authenSuccessCallback)
        {
            authenSuccessCallback.RegisterAuthenSuccessCalback(this);
        }

        public void UnregisterAuthenProcess(IAuthenSuccessCallback authenSuccessCallback)
        {
            authenSuccessCallback.RegisterAuthenSuccessCalback(this);
        }
        #endregion

        /// <summary>
        /// call từ ứng dụng
        /// </summary>
        /// <param name="isTemporary"></param>
        public static  void SetTemporaryVersionV2(bool isTemporary)
        {
            _isUseTemporaryVersion = isTemporary;
        }

        public static bool IsUseTemporaryVersion => _isUseTemporaryVersion;
    }
}

