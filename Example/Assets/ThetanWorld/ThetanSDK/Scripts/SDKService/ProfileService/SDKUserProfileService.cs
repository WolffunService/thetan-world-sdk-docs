using System;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.SDKServices.Profile
{
    internal class SDKUserProfileService : BaseClassService, IPostAuthenProcessor
    {
        private const string SAVE_FILE_NAME = "ThetanWorldCacheProfileData";
        
        #region Dependency Properties
        private AuthenProcessContainer _authenProcessContainer;
        private NetworkClient _networkClient;
        #endregion
        
        private SDKUserProfileModel _profileData;
        private ThetanNetworkClientState _prevNetworkClientState;
        
        [NonSerialized] private LocalDataLoadSaver<SDKUserProfileModel> _localDataLoadSaver;
        
        public string UserId => _profileData.id;
        public string UserCountry => _profileData.country;
        public int AvatarId => _profileData.avatar;
        public int AvatarFrameId => _profileData.avatarFrame;
        public string Username => _profileData.nickname;
        public string Email => _profileData.email;

        public string WalletAddress => _profileData.address;
        public string WalletProvider => _profileData.walletProvider;

        public async UniTask InitService(AuthenProcessContainer authenProcessContainer, NetworkClient networkClient)
        {
            _localDataLoadSaver = new LocalDataLoadSaver<SDKUserProfileModel>();
            _authenProcessContainer = authenProcessContainer;
            _networkClient = networkClient;
            authenProcessContainer.WFIDAuthenProcess.RegisterPostAuthenProcessor(this);
            authenProcessContainer.ThetanAppAuthenProcess.RegisterPostAuthenProcessor(this);
            _networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
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
        }

        public UniTask<bool> GetUserProfile()
        {
            UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
            CallGetUserProfile(_ =>
            {
                completionSource.TrySetResult(true);
            }, error =>
            {
                // Todo: consider if we should handle error when load profile error
                completionSource.TrySetResult(false);
            });

            return completionSource.Task;
        }

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
                onSuccessCallback?.Invoke(userProfile);
            }, onErrorCallback, AuthType.TOKEN);
        }

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

        public override void ClearDataService()
        {
            _profileData = new SDKUserProfileModel();
        }
    }
}