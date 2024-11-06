using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService.LuckySpin;
using ThetanSDK.SDKService.RemoteConfig;
using ThetanSDK.SDKService.UserStatisticService;
using ThetanSDK.SDKServices.Analytic;
using ThetanSDK.SDKServices.Equipment;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.SDKServices.Profile;
using ThetanSDK.UI;
using ThetanSDK.UI.Connection;
using ThetanSDK.VersionCheckService;
using UnityEngine;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.StorageResource;

namespace ThetanSDK
{
    /// <summary>
    /// Class manager sdk initialize behavior
    /// </summary>
    internal class ThetanSDKManagerInitializer
    {
        private const string PREFS_CHECK_AUTO_LOGIN_GUEST = "ThetanWorldCheckAutoLogin";
        
        private NetworkClient _networkClient;
        private RemoteConfigService _remoteConfigService;
        private SDKAnalyticService _analyticService;
        private AuthenProcessContainer _authenProcessContainer;
        private SDKUserProfileService _profileService;
        private NftItemService _nftItemService;
        private EquipmentService _equipmentService;
        private LuckySpinService _luckySpinService;
        private UserStatisticService _userStatisticService;
        private UIMainButtonThetanWorld _btnMainAction;
        private ShowAnimCurrencyFly _showAnimCurrencyFly;
        private Action _onClickMainAction;
        private ShowPopupWhenLostConnection _showPopopWhenLostConnection;
        private UIHelperContainer _uiHelperContainer;
        private ThetanSDKVersionHandle _sdkVersionHandle;
        private Action _onConfirmLostConnectionCallback;

        internal ThetanSDKManagerInitializer(
            NetworkClient networkClient,
            RemoteConfigService remoteConfigService,
            SDKAnalyticService sdkAnalyticService,
            AuthenProcessContainer authenProcessContainer,
            SDKUserProfileService profileService,
            NftItemService nftItemService,
            EquipmentService equipmentService,
            LuckySpinService luckySpinService,
            UserStatisticService userStatisticService,
            UIMainButtonThetanWorld btnMainAction,
            ShowAnimCurrencyFly showAnimCurrencyFly,
            Action onClickMainAction,
            ShowPopupWhenLostConnection showPopopWhenLostConnection,
            UIHelperContainer uiHelperContainer,
            ThetanSDKVersionHandle sdkVersionHandle,
            Action onConfirmLostConnectionCallback)
        {
            _networkClient = networkClient;
            _remoteConfigService = remoteConfigService;
            _analyticService = sdkAnalyticService;
            _authenProcessContainer = authenProcessContainer;
            _profileService = profileService;
            _nftItemService = nftItemService;
            _equipmentService = equipmentService;
            _luckySpinService = luckySpinService;
            _userStatisticService = userStatisticService;
            _btnMainAction = btnMainAction;
            _showAnimCurrencyFly = showAnimCurrencyFly;
            _onClickMainAction = onClickMainAction;
            _showPopopWhenLostConnection = showPopopWhenLostConnection;
            _uiHelperContainer = uiHelperContainer;
            _sdkVersionHandle = sdkVersionHandle;
            _onConfirmLostConnectionCallback = onConfirmLostConnectionCallback;
        }
        
        /// <summary>
        /// Initialize NetworkClient and other services
        /// </summary>
        /// <param name="onDoneCallback">Callback when done initialize.
        /// First param is NetworkClientState.
        /// Second param is this SDK version supported</param>
        public void Initialize(SDKOption option, Action<ThetanNetworkClientState, VersionSupportedStatus> onDoneCallback)
        {
            _networkClient.InitializeNetworkClient(ThetanSDKManager.Instance.Version, 
                (networkClientState, hasTokenFromPrevSession) =>
            {
                _remoteConfigService.CallGetRemoteConfig((success) =>
                {
                    if (success)
                    {
                        PostProcessInitialize(option, networkClientState, hasTokenFromPrevSession, onDoneCallback);
                    }
                    else
                    {
                        AdminLog.LogError("ThetanSDKManager Initialize error");
                        //retry
                        ReInitialize(option, onDoneCallback, 1);
                    }
                });
            });
        }

        /// <summary>
        /// ReInitialize when First initialize fail 
        /// </summary>
        private void ReInitialize(SDKOption option, Action<ThetanNetworkClientState, VersionSupportedStatus> onDoneCallback, int retryCount)
        {
            if(retryCount > 3) // Prevent infinite loop initialize
            {
                PostProcessInitialize(option, _networkClient.NetworkClientState, false, onDoneCallback);
                return;
            }
            
            _networkClient.InitializeNetworkClient(ThetanSDKManager.Instance.Version, 
                (networkClientState, hasTokenFromPrevSession) =>
                {
                    _remoteConfigService.CallGetRemoteConfig((success) =>
                    {
                        if (success)
                        {
                            PostProcessInitialize(option, networkClientState, hasTokenFromPrevSession, onDoneCallback);
                        }
                        else
                        {
                            AdminLog.LogError("ThetanSDKManager Initialize error");
                            //retry
                            retryCount++;
                            ReInitialize(option, onDoneCallback, retryCount);
                        }
                    });
                });
        }
        
        private async void PostProcessInitialize(SDKOption option, ThetanNetworkClientState networkClientState,
            bool hasTokenFromPrevSession,
            Action<ThetanNetworkClientState, VersionSupportedStatus> onDoneCallback)
        {
            StorageResource.Initialize(_networkClient.StorageResourceUrl);
            var versionSupportedStatus = await _sdkVersionHandle.InitializeVersionHandle(_networkClient);

            if (versionSupportedStatus == VersionSupportedStatus.Unsupported)
            {
                _analyticService.InitialzeService(_authenProcessContainer, _networkClient.NetworkConfig);
                _btnMainAction.Initialize(_showAnimCurrencyFly, _nftItemService, _onClickMainAction);
                onDoneCallback?.Invoke(networkClientState, versionSupportedStatus);
                return;
            }
            
            RegisterAuthenProcessToNetworkClient();

            if (!hasTokenFromPrevSession && 
                (!PlayerPrefs.HasKey(PREFS_CHECK_AUTO_LOGIN_GUEST) || PlayerPrefs.GetInt(PREFS_CHECK_AUTO_LOGIN_GUEST) != 1))
            {
                PlayerPrefs.SetInt(PREFS_CHECK_AUTO_LOGIN_GUEST, 1);
                UniTaskCompletionSource<bool> loginGuestAccountCompletionSource = new UniTaskCompletionSource<bool>();
                _authenProcessContainer.WFIDAuthenProcess.LoginWithGuessAcount(authenData =>
                {
                    loginGuestAccountCompletionSource.TrySetResult(true);
                }, error =>
                {
                    loginGuestAccountCompletionSource.TrySetResult(false);
                });

                await loginGuestAccountCompletionSource.Task;
            }
            else
            {
                PlayerPrefs.SetInt(PREFS_CHECK_AUTO_LOGIN_GUEST, 1);
            }

            List<UniTask> listTask = new List<UniTask>();
            
            listTask.Add(_profileService.InitService(_authenProcessContainer, _networkClient));
            listTask.Add(_nftItemService.InitService(_networkClient));
            listTask.Add(_equipmentService.InitService(_networkClient));
            
            //_luckySpinService.InitService(_networkClient);
            _remoteConfigService.InitService(_networkClient);
            _analyticService.InitialzeService(_authenProcessContainer, _networkClient.NetworkConfig);
            _userStatisticService.InitService(_networkClient);

            await UniTask.WhenAll(listTask);
            
            if (option.AutoShowPopupWhenLostConnection)
            {
                _showPopopWhenLostConnection.Initialize(_networkClient, _uiHelperContainer, _onConfirmLostConnectionCallback);
            }
            _btnMainAction.Initialize(_showAnimCurrencyFly, _nftItemService, _onClickMainAction);
            onDoneCallback?.Invoke(networkClientState, versionSupportedStatus);
        }

        
        
        private void RegisterAuthenProcessToNetworkClient()
        {
            _networkClient.RegisterAuthenProcess(_authenProcessContainer.WFIDAuthenProcess);
            _networkClient.RegisterAuthenProcess(_authenProcessContainer.ThetanAppAuthenProcess);
        }
    }
}