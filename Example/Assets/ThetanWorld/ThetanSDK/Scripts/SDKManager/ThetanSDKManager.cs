using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService;
using ThetanSDK.SDKService.LuckySpin;
using ThetanSDK.SDKService.RemoteConfig;
using ThetanSDK.SDKServices.Analytic;
using ThetanSDK.SDKServices.Equipment;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.SDKServices.Profile;
using ThetanSDK.UI;
using ThetanSDK.UI.Connection;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Log;
using Wolffun.MultiPlayer;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.StorageResource;
using Screen = ThetanSDK.UI.Screen;

namespace ThetanSDK
{
    /// <summary>
    /// SDK option for configure SDK behavior. This option is passed into SDK when initialized.
    /// </summary>
    public struct SDKOption
    {
        /// <summary>
        /// Allow SDK auto show popup lost connection when user is not connected to network
        /// </summary>
        public bool AutoShowPopupWhenLostConnection;
        /// <summary>
        /// Make SDK use full screen or popup login UI
        /// </summary>
        public bool UseFullscreenLogin;
    }
    
    public class ThetanSDKManager : MonoSingleton<ThetanSDKManager>
    {
        #region Serialize Field
        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private NetworkClient _networkClient;
        [SerializeField] private AuthenProcessContainer _authenProcessContainer;
        [SerializeField] private AuthenUIControl _prefabAuthenUIControl;
        [SerializeField] private UIMainButtonThetanWorld _btnMainAction;
        [SerializeField] private ShowAnimCurrencyFly _showAnimCurrencyFly;
        
        [Header("UI")]
        //[SerializeField] private FloatingWindowContainer _floatingWindowContainer;
        [SerializeField] private UIHelperContainer _uiHelperContainer;
        [SerializeField] private ScreenContainer _screenContainer;
        //[SerializeField] private ScreenMainUILandscape _screenMainUILandscape;

        [Header("Services")]
        [SerializeField] private NftItemService _nftItemService;
        [SerializeField] private SDKUserProfileService _profileService;
        [SerializeField] private SDKAnalyticService _analyticService;
        [SerializeField] private EquipmentService _equipmentService;
        [SerializeField] private LuckySpinService _luckySpinService;
        [SerializeField] private RemoteConfigService _remoteConfigService;

        [Header("Network Availability")]
        [SerializeField] private NetworkReachabilityDetect _networkReachabilityDetect;
        [SerializeField] private ShowPopupWhenLostConnection _showPopopWhenLostConnection;
        #endregion

        #region Public Properties
        internal NftItemService NftItemService => _nftItemService;
        internal SDKUserProfileService ProfileService => _profileService;
        internal SDKAnalyticService AnalyticService => _analyticService;

        internal EquipmentService EquipmentService => _equipmentService;

        internal LuckySpinService LuckySpinService => _luckySpinService;

        internal RemoteConfigService RemoteConfigService => _remoteConfigService;
        
        internal UIHelperContainer RootUIHelperContainer => _uiHelperContainer;
        
        public AuthenProcessContainer AuthenProcessContainer => _authenProcessContainer;

        internal ThetanNetworkConfig NetworkConfig => _networkClient.NetworkConfig;

        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public ThetanNetworkClientState NetworkClientState => _networkClient.NetworkClientState;

        internal SDKOption SdkOption => _option;
        #endregion
        
        #region Public Event Callback

        /// <summary>
        /// Callback when user log out
        /// </summary>
        public event Action OnUserLogOutCallback;

        /// <summary>
        /// Callback when client changed its internal client state
        /// </summary>
        public event Action<ThetanNetworkClientState> OnChangeNetworkClientState;

        /// <summary>
        /// Callback when user open main Thetan World UI
        /// </summary>
        public event Action OnOpenMainUI;

        /// <summary>
        /// Callback when Thetan World UI is closed
        /// </summary>
        public event Action OnCloseMainUI;
        
        #endregion
        
        #region Version
        private const string _version = "0.9.9";

        public string Version
        {
            get 
            {
                return _version;
            }
        }

        private bool _isVersionSupported;

        public bool IsVersionSupported => _isVersionSupported;
        #endregion
        
        #region private Properties
        private int _matchMaxDuration = 0;

        private SDKOption _option;

        private NftItemServiceHelper _nftItemServiceHelper;

        private ThetanSDKManagerHandleUI _sdkManagerHandleUI;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            
            if (Utils.GetCurrentScreenType() == Utils.ScreenType.Landscape)
            {
                _canvasScaler.referenceResolution = new Vector2(1920, 1080);
            }
            else
            {
                _canvasScaler.referenceResolution = new Vector2(1080, 1920);
            }
            
            _btnMainAction.gameObject.SetActive(false);

            _sdkManagerHandleUI = new ThetanSDKManagerHandleUI(_networkClient, _screenContainer,
                _uiHelperContainer, _showPopopWhenLostConnection, this.transform, _prefabAuthenUIControl,
                _authenProcessContainer,
                () => OnOpenMainUI?.Invoke(),
                () => OnCloseMainUI?.Invoke());
            
            if(_networkClient != null)
                _networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
            
            _screenContainer.RegisterOnClickCloseScreen(OnClickCloseMainScreen);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if(_sdkManagerHandleUI != null)
                _sdkManagerHandleUI.Dispose();
            
            if(_networkClient != null)
                _networkClient.UnSubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
        }

        #region Initialize SDK
        /// <summary>
        /// Initialize SDK before client can start using any SDK function
        /// </summary>
        /// <param name="option">an option for configure SDK behavior</param>
        /// <param name="onDoneCallback">a callback with ThetanNetworkClientState when SDK is done initialized</param>
        public void Initialize(SDKOption option, Action<ThetanNetworkClientState> onDoneCallback)
        {
            _option = option;

            ThetanSDKManagerInitializer thetanSDKManagerInitializer = new ThetanSDKManagerInitializer(
                _networkClient, _remoteConfigService, _analyticService, _authenProcessContainer, _profileService,
                _nftItemService, _equipmentService,
                _luckySpinService, _btnMainAction,
                _showAnimCurrencyFly, OnClickMainAction, _showPopopWhenLostConnection,
                _uiHelperContainer, () =>
                {
                    _screenContainer.PopAllScreen();
                });
            
            thetanSDKManagerInitializer.Initialize(option, (networkClientState, isVersionSupported) =>
            {
                OnDoneInitialize(networkClientState, isVersionSupported);
                onDoneCallback?.Invoke(networkClientState);
            });
        }
        
        /// <summary>
        /// Called when thetanSDKManagerInitializer done initialize
        /// </summary>
        private async void OnDoneInitialize(ThetanNetworkClientState networkClientState, bool isVersionSupported)
        {
            _isVersionSupported = isVersionSupported;

            _nftItemServiceHelper = new NftItemServiceHelper(_uiHelperContainer, _nftItemService, _analyticService);
            
            _isInitialized = true;
        }
        #endregion
        
        /// <summary>
        /// Log out current account.
        /// </summary>
        public void LogOut()
        {
            _networkClient.LogOut();
            _analyticService.ClearDataService();
            _profileService.ClearDataService();
            _nftItemService.ClearDataService();
            OnUserLogOutCallback?.Invoke();
        }
        
        #region UI FUNCTIONS
        /// <summary>
        /// Show UI Login Thetan World
        /// </summary>
        public void ShowLogin(Action<AuthenResultData> onLoginSuccessCallback, Action onUserCancelCallback)
        {
            _sdkManagerHandleUI.ShowLogin(onLoginSuccessCallback, onUserCancelCallback);
        }
        
        /// <summary>
        /// Show UI Link Account when user use play as guess function.
        /// Only use this function when your game support user play as guess through WFIDAuthenProcessor
        /// </summary>
        /// <param name="onSuccessCallback"></param>
        /// <param name="onCancelCallback"></param>
        public void ShowLinkAccount(Action<AuthenResultData> onSuccessCallback, Action onCancelCallback)
        {
            _sdkManagerHandleUI.ShowLinkAccount(onSuccessCallback, onCancelCallback);
        }

        /// <summary>
        /// Show button Thetan World for user interact with Thetan World UI
        /// </summary>
        public void ShowButtonMainAction()
        {
            _btnMainAction.gameObject.SetActive(true);
        }

        /// <summary>
        /// Completely hide button Thetan World and close all current openned thetan world UI.
        /// </summary>
        public void HideButtonMainAction()
        {
            _btnMainAction.gameObject.SetActive(false);
            
            if(_sdkManagerHandleUI != null)
                _sdkManagerHandleUI.CloseMainUI();
        }
        
        /// <summary>
        /// Invoked when user click on button Thetan World from home
        /// </summary>
        private void OnClickMainAction()
        {
            if(_sdkManagerHandleUI != null)
                _sdkManagerHandleUI.OnClickMainAction();
        }

        /// <summary>
        /// Show UI Thetan World
        /// </summary>
        /// <returns></returns>
        private async UniTask<ScreenMainUI> ShowMainUI()
        {
            if (_sdkManagerHandleUI != null)
                return await _sdkManagerHandleUI.ShowMainUI();

            return null;
        }

        /// <summary>
        /// Open UI Thetan World and auto navigate to screen list NFT
        /// </summary>
        public void OpenScreenSelectNFT()
        {
            if (_sdkManagerHandleUI != null)
                _sdkManagerHandleUI.OpenScreenSelectNFT();
        }

        /// <summary>
        /// Invoked when user click on button close or some feature request close UI Thetan World
        /// </summary>
        private void OnClickCloseMainScreen()
        {
            if(_sdkManagerHandleUI != null)
                _sdkManagerHandleUI.CloseMainUI();
        }
        #endregion

        #region NFT HERO SERVICE API
        
        /// <summary>
        /// Get selected hero NFT id
        /// </summary>
        public string SelectedHeroNftId => _nftItemService.SelectedHeroNftId;

        /// <summary>
        /// Check is selected any hero NFT
        /// </summary>
        public bool IsSelectedAnyHeroNftItem => !string.IsNullOrEmpty(_nftItemService.SelectedHeroNftId);

        /// <summary>
        /// Get grinding hero NFT id
        /// </summary>
        public string GrindingHeroNftId => _nftItemService.GrindingHeroNftId;

        /// <summary>
        /// Check is grinding any hero NFT
        /// </summary>
        public bool IsGrindingAnyHeroNftItem => !string.IsNullOrEmpty(_nftItemService.GrindingHeroNftId);

        /// <summary>
        /// Check if user is selecting hero NFT with id heroId
        /// </summary>
        /// <param name="heroId">hero NFT id need to check</param>
        /// <returns>Return true if user is selecting NFT with heroId. Otherwise, return false</returns>
        public bool CheckHeroIsSelected(string heroId) => _nftItemService.IsHeroNftSelected(heroId);

        /// <summary>
        /// Get HeroNFTItem base on HeroNFTId. 
        /// This function will first try get HeroNFTItem from NFTItemService's cache,
        /// if cannot get from cache, it will call server to get data
        /// </summary>
        public async UniTask<HeroNftItem> GetHeroNftItemInfo(string nftId)
        {
            if (!_isVersionSupported)
            {
                return new HeroNftItem().SetDefault();
            }
            
            if (_nftItemServiceHelper == null)
                return new HeroNftItem().SetDefault();

            return await _nftItemServiceHelper.GetHeroNftItemInfo(nftId);
        }

        /// <summary>
        /// Get HeroNFTItem base on HeroNFTId from NFTItemService's cache. 
        /// If cannot find in cache, will return default value
        /// </summary>
        public HeroNftItem TryGetCachedHeroNftItemByIngameId(string ingameHeroId)
        {
            if (!_isVersionSupported)
            {
                return new HeroNftItem().SetDefault();
            }
            
            return _nftItemService.GetCachedHeroNftItemByIngameId(ingameHeroId);
        }

        /// <summary>
        /// Refresh list cache HeroNFTId in NFTItemService
        /// </summary>
        public void RefreshListNFTHero()
        {
            if (!_isVersionSupported)
            {
                return;
            }
            
            _nftItemService.RefetchListHeroNFT(null, null);
        }

        /// <summary>
        /// Call server to get new data of hero NFT with nftId and save new data to NFTItemService's cache
        /// </summary>
        /// <param name="nftId">hero NFT id need to check</param>
        /// <param name="onSuccessCallback">callback when success refresh data, return new HeroNftItem data</param>
        /// <param name="onErrorCallback">error callback when refresh data fail</param>
        public void RefreshHeroNftData(string nftId, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (!_isVersionSupported)
            {
                return;
            }
            
            _nftItemService.RefreshDataHeroNft(nftId, onSuccessCallback, onErrorCallback);
        }
        
        /// <summary>
        /// Register callback when user select/unselect an hero NFT
        /// </summary>
        /// <param name="callback">A callback with heroNFTId when user select new hero NFT. This callback also be invoked with string.empty or null when user unselect hero NFT</param>
        /// <returns></returns>
        public void RegisterOnChangeSelectedHeroNft(Action<string> callback) =>
            _nftItemService.RegisterOnChangeSelectedNftHeroCallback(callback);
        
        /// <summary>
        /// UnRegister callback when user select/unselect an hero NFT
        /// </summary>
        /// <param name="callback">A callback used when RegisterOnChangeSelectedHeroNft</param>
        public void UnRegisterOnChangeSelectedHeroNft(Action<string> callback) =>
            _nftItemService.UnRegisterOnChangeSelectedNftHeroCallback(callback);
        
        
        /// <summary>
        /// Set max grinding session time out.
        /// Each 5 seconds game client will ping server to notify grinding,
        /// if server cannot receive grinding signal for max grinding session time out, server will automatically end grinding session and unlock NFT.
        /// Default value is 300 seconds.
        /// </summary>
        /// <param name="matchMaxDuration">match max duration time out (count in seconds) to unlock NFT and end grinding session when receive no grinding signal from game client</param>
        public void SetMatchMaxDuration(int matchMaxDuration)
        {
            if(_nftItemServiceHelper != null)
                _nftItemServiceHelper.SetMatchMaxDuration(matchMaxDuration);
        }
        
        /// <summary>
        /// Lock NFT and prepare NFT for grinding session. Also, this function will lock interaction for UI Thetan World,
        /// UI can only be unlocked after you call UnlockButtonMain at the end of grinding session.
        /// onErrorCallback will be call when cannot prepare match for NFT, error callback contain 1 of these error codes, can be access via WolffunResponseError.Code
        /// </summary>
        /// <param name="onSuccessCallback">callback when success prepare match with selected NFT</param>
        /// <param name="onErrorCallback">callback contain error info when prepare match is not success.</param>
        public void PrepareMatchForSelectedNFT(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if(_nftItemServiceHelper != null)
                _nftItemServiceHelper.PrepareMatchForSelectedNFT(onSuccessCallback, onErrorCallback);
        }

        /// <summary>
        /// Lock NFT and prepare NFT for grinding session.
        /// This function is similar to PrepareMatchForSelectedNFT but it auto show error message and ask user if user want to continue playing without grinding when there is an error.
        /// </summary>
        /// <param name="onSuccessCallback">callback when call prepare match for NFT success or there is error but user confirm start match without grind NFT.</param>
        /// <param name="onErrorCallback">callback when call prepare grind has error and user confirm to go back.</param>
        public void PrepareMatchForSelectedNFTAutoHandleError(Action<NftItemServiceErrorCode> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if(_nftItemServiceHelper != null)
                _nftItemServiceHelper.PrepareMatchForSelectedNFTAutoHandleError(onSuccessCallback, onErrorCallback);
        }

        /// <summary>
        /// Start grinding NFT after prepare match for NFT is succedd.
        /// If call this before call PrepareMatchForSelectedNFT, it won't do anything.
        /// This function also can be used to resume grinding after call PauseGrindingHeroItem
        /// </summary>
        public void StartGrindingHeroItem()
        {
            _nftItemService.StartGrind();
        }

        /// <summary>
        /// Temporary pause grinding hero NFT after called StartGrindingHeroItem.
        /// Used for case when your game is paused and you need to pause grinding too.
        /// </summary>
        public void PauseGrindingHeroItem()
        {
            _nftItemService.StopGrind();
        }

        /// <summary>
        /// End grinding session and unlock selected NFT.
        /// You have to call this at the end of the game to unlock NFT and stop grinding session.
        /// Otherwise, user cannot select and grind other NFT.
        /// </summary>
        public void StopGrindingHeroItem()
        {
            _nftItemService.EndMatch(null, null);
        }

        /// <summary>
        /// Unlock Interaction for UI Thetan World.
        /// You may call this right after StopGrindingHeroItem or anytime that suit your game flow but it must be called after StopGrindingHeroItem.
        /// Otherwise, user cannot interact with UI Thetan World.
        /// </summary>
        public void UnlockButtonMain()
        {
            _btnMainAction.UnlockButtonAndDoAnimReward();
        }
        #endregion

        /// <summary>
        /// Handle when receive error code maintenance from server
        /// </summary>
        internal void HandleMaintenance()
        {
            if(_sdkManagerHandleUI != null)
                _sdkManagerHandleUI.ShowPopupMaintenance();
        }
        
        private void HandleOnChangeNetworkClientState(ThetanNetworkClientState networkClientState)
        {
            OnChangeNetworkClientState?.Invoke(networkClientState);
        }
    }
}