using System;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.SDKServices.Profile
{
    /// <summary>
    /// Service for manage user profile
    /// </summary>
    internal class SDKUserProfileService : BaseClassService, IPostAuthenProcessor
    {
        /// <summary>
        /// Cache user profile save file name.
        /// Used to load cached user profile when user enter Thetan World without network
        /// </summary>
        private const string SAVE_FILE_NAME = "ThetanWorldCacheProfileData";
        
        #region Dependency Properties
        private AuthenProcessContainer _authenProcessContainer;
        private NetworkClient _networkClient;
        #endregion
        
        /// <summary>
        /// User profile data
        /// </summary>
        private SDKUserProfileModel _profileData;

        internal Action<SDKUserProfileModel> OnChangeProfileDataCallback;

        /// <summary>
        /// Previous network client state
        /// </summary>
        private ThetanNetworkClientState _prevNetworkClientState;
        
        /// <summary>
        /// File loader to load cached user profile from local file
        /// </summary>
        [NonSerialized] private LocalDataLoadSaver<SDKUserProfileModel> _localDataLoadSaver;
        
        public string UserId => _profileData.id;
        public string UserCountry => _profileData.country;
        public int AvatarId => _profileData.avatar;
        public int AvatarFrameId => _profileData.avatarFrame;
        public string Username => _profileData.nickname;
        public string Email => _profileData.email;

        public string WalletAddress => _profileData.address;
        public string WalletProvider => _profileData.walletProvider;

        /// <summary>
        /// Call to init service before use
        /// </summary>
        public async UniTask InitService(AuthenProcessContainer authenProcessContainer, NetworkClient networkClient)
        {
            _localDataLoadSaver = new LocalDataLoadSaver<SDKUserProfileModel>();
            _authenProcessContainer = authenProcessContainer;
            _networkClient = networkClient;
            authenProcessContainer.WFIDAuthenProcess.RegisterPostAuthenProcessor(this);
            authenProcessContainer.ThetanAppAuthenProcess.RegisterPostAuthenProcessor(this);
            _networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
            _networkClient.SubcribeOnReAuthenCallback(HandleOnReAuthen);
            _prevNetworkClientState = _networkClient.NetworkClientState;
            
            if (_networkClient.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
                {
                    // Use cache from previous session
                    _profileData = _localDataLoadSaver.LoadDataLocal(SAVE_FILE_NAME);
                }
                else
                {
                    _profileData = new SDKUserProfileModel().SetDefault();
                }
                return;
            }
            
            _profileData = new SDKUserProfileModel().SetDefault();
            UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
            CallGetUserProfile(_ =>
            {
                completionSource.TrySetResult();
            }, error =>
            {
                // Todo: consider if we should handle error when load profile error
                completionSource.TrySetResult();
            });

            await completionSource.Task;
        }

        private void HandleOnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (_prevNetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork &&
                newState == ThetanNetworkClientState.LoggedIn)
            {
                CallGetUserProfile(null, null);
            }
            else if ((newState == ThetanNetworkClientState.NotLoggedIn ||
                      newState == ThetanNetworkClientState.NotLoggedInNoNetwork) &&
                     (_prevNetworkClientState == ThetanNetworkClientState.LoggedIn || 
                     _prevNetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork))
            {
                OnUserLogOut();
            }

            _prevNetworkClientState = newState;
        }

        private void HandleOnReAuthen()
        {
            OnUserLogOut();
            CallGetUserProfile(null, null);
        }

        private void OnDestroy()
        {
            if (ThetanSDKManager.IsAlive)
                ThetanSDKManager.Instance.OnUserLogOutCallback -= OnUserLogOut;

            if (_authenProcessContainer != null)
            {
                if(_authenProcessContainer.WFIDAuthenProcess != null)
                    _authenProcessContainer.WFIDAuthenProcess.RegisterPostAuthenProcessor(this);
                
                if(_authenProcessContainer.ThetanAppAuthenProcess != null)
                    _authenProcessContainer.ThetanAppAuthenProcess.RegisterPostAuthenProcessor(this);
            }
        }

        private void OnUserLogOut()
        {
            _profileData = _profileData.SetDefault();
            _localDataLoadSaver.SaveDataLocal(new SDKUserProfileModel().SetDefault(), SAVE_FILE_NAME);
            ClearDataService();
        }

        /// <summary>
        /// Call server to load user profile.
        /// Return true if load success, otherwise return false
        /// </summary>
        public UniTask<bool> GetUserProfile()
        {
            UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
            CallGetUserProfile(_ =>
            {
                completionSource.TrySetResult(true);
            }, error =>
            {
                completionSource.TrySetResult(false);
            });

            return completionSource.Task;
        }

        /// <summary>
        /// Call server to load user profile
        /// </summary>
        private void CallGetUserProfile(Action<SDKUserProfileModel> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;

            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/user")
                .Get();
            WolffunUnityHttp.Instance.MakeAPI<SDKUserProfileModel>(reqCommon, userProfile =>
            {
                _profileData = userProfile;
                _localDataLoadSaver.SaveDataLocal(_profileData, SAVE_FILE_NAME);
                OnChangeProfileDataCallback?.Invoke(_profileData);
                onSuccessCallback?.Invoke(userProfile);
            }, onErrorCallback, AuthType.TOKEN);
        }

        /// <summary>
        /// Invoked by authen processor after user logged in.
        /// Load user profile right after user logged in
        /// </summary>
        public UniTask ProcessPostAuthenProcess(PostAuthenSuccessMetaData metaData)
        {
            _profileData = new SDKUserProfileModel().SetDefault();
            
            UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
            CallGetUserProfile(_ =>
            {
                completionSource.TrySetResult();
            }, error =>
            {
                // Todo: consider if we should handle error when load profile error
                completionSource.TrySetResult();
            });

            return completionSource.Task;
        }

        /// <summary>
        /// Clear cached data
        /// </summary>
        public override void ClearDataService()
        {
            _profileData = new SDKUserProfileModel();
        }
    }
}