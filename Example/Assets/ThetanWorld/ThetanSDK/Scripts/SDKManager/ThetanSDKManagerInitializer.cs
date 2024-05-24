using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService.LuckySpin;
using ThetanSDK.SDKService.RemoteConfig;
using ThetanSDK.SDKServices.Analytic;
using ThetanSDK.SDKServices.Equipment;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.SDKServices.Profile;
using ThetanSDK.UI;
using ThetanSDK.UI.Connection;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;
using Wolffun.StorageResource;

namespace ThetanSDK
{
    /// <summary>
    /// Class manager sdk initialize behavior
    /// </summary>
    internal class ThetanSDKManagerInitializer
    {
        private NetworkClient _networkClient;
        private RemoteConfigService _remoteConfigService;
        private SDKAnalyticService _analyticService;
        private AuthenProcessContainer _authenProcessContainer;
        private SDKUserProfileService _profileService;
        private NftItemService _nftItemService;
        private EquipmentService _equipmentService;
        private LuckySpinService _luckySpinService;
        private UIMainButtonThetanWorld _btnMainAction;
        private ShowAnimCurrencyFly _showAnimCurrencyFly;
        private Action _onClickMainAction;
        private ShowPopupWhenLostConnection _showPopopWhenLostConnection;
        private UIHelperContainer _uiHelperContainer;
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
            UIMainButtonThetanWorld btnMainAction,
            ShowAnimCurrencyFly showAnimCurrencyFly,
            Action onClickMainAction,
            ShowPopupWhenLostConnection showPopopWhenLostConnection,
            UIHelperContainer uiHelperContainer,
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
            _btnMainAction = btnMainAction;
            _showAnimCurrencyFly = showAnimCurrencyFly;
            _onClickMainAction = onClickMainAction;
            _showPopopWhenLostConnection = showPopopWhenLostConnection;
            _uiHelperContainer = uiHelperContainer;
            _onConfirmLostConnectionCallback = onConfirmLostConnectionCallback;
        }
        
        /// <summary>
        /// Initialize NetworkClient and other services
        /// </summary>
        /// <param name="onDoneCallback">Callback when done initialize.
        /// First param is NetworkClientState.
        /// Second param is this SDK version supported</param>
        public void Initialize(SDKOption option, Action<ThetanNetworkClientState, bool> onDoneCallback)
        {
            _networkClient.InitializeNetworkClient(ThetanSDKManager.Instance.Version, (networkClientState) =>
            {
                _remoteConfigService.CallGetRemoteConfig((success) =>
                {
                    if (success)
                    {
                        PostProcessInitialize(option, networkClientState, onDoneCallback);
                    }
                    else
                    {
                        AdminLog.LogError("ThetanSDKManager Initialize error");
                        //retry
                        Initialize(option, onDoneCallback);
                    }
                });
            });
        }
        
        private async void PostProcessInitialize(SDKOption option, ThetanNetworkClientState networkClientState,
            Action<ThetanNetworkClientState, bool> onDoneCallback)
        {
            StorageResource.Initialize(_networkClient.StorageResourceUrl);

            var isVersionSupported = await CheckVersion();
            
            //Todo: delete me
            isVersionSupported = true;

            _analyticService.InitialzeService(_authenProcessContainer, _networkClient.NetworkConfig);
            
            if (!isVersionSupported)
            {
                _btnMainAction.Initialize(_showAnimCurrencyFly, _nftItemService, _onClickMainAction);
                onDoneCallback?.Invoke(networkClientState, isVersionSupported);
                return;
            }
            
            RegisterAuthenProcessToNetworkClient();

            List<UniTask> listTask = new List<UniTask>();
            
            listTask.Add(_profileService.InitService(_authenProcessContainer, _networkClient));
            listTask.Add(_nftItemService.InitService(_networkClient));
            listTask.Add(_equipmentService.InitService(_networkClient));
            
            _luckySpinService.InitService(_networkClient);
            _remoteConfigService.InitService(_networkClient);

            await UniTask.WhenAll(listTask);
            
            if (option.AutoShowPopupWhenLostConnection)
            {
                _showPopopWhenLostConnection.Initialize(_networkClient, _uiHelperContainer, _onConfirmLostConnectionCallback);
            }
            _btnMainAction.Initialize(_showAnimCurrencyFly, _nftItemService, _onClickMainAction);
            _analyticService.LogLoginSuccess(new PostAuthenSuccessMetaData());
            onDoneCallback?.Invoke(networkClientState, isVersionSupported);
        }
        
        private UniTask<bool> CheckVersion()
        {
            UniTaskCompletionSource<bool> checkVersionCompletionSource = new UniTaskCompletionSource<bool>();
            
            WolffunRequestCommon req = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/partner/app/config")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI<VersionDataModel>(req, versionDataModel =>
            {
                if (versionDataModel.supportedVersions == null ||
                    versionDataModel.supportedVersions.Length == 0)
                {
                    checkVersionCompletionSource.TrySetResult(false);
                    return;
                }

                var version = ThetanSDKManager.Instance.Version;

                foreach (var supportedVersion in versionDataModel.supportedVersions)
                {
                    if (version == supportedVersion)
                    {
                        checkVersionCompletionSource?.TrySetResult(true);
                        return;
                    }
                }
                
                checkVersionCompletionSource?.TrySetResult(false);

            }, error =>
            {
                checkVersionCompletionSource.TrySetResult(false);
            }, AuthType.TOKEN);

            return checkVersionCompletionSource.Task;
        }
        
        private void RegisterAuthenProcessToNetworkClient()
        {
            _networkClient.RegisterAuthenProcess(_authenProcessContainer.WFIDAuthenProcess);
            _networkClient.RegisterAuthenProcess(_authenProcessContainer.ThetanAppAuthenProcess);
        }
    }
}