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
    public struct SDKOption
    {
        public bool AutoShowPopupWhenLostConnection;
        public bool UseFullscreenLogin;
    }
    
    public class ThetanSDKManager : MonoSingleton<ThetanSDKManager>
    {
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
        
        #region Public Event Callback

        public event Action OnUserLogOutCallback;

        public event Action<ThetanNetworkClientState> OnChangeNetworkClientState;

        public event Action OnOpenMainUI;

        public event Action OnCloseMainUI;
        
        #endregion
        
        #region Version
        private const string _version = "0.9.2";

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

        private AuthenUIControl _currentAuthenUIControl;

        private ScreenMainUI _currentScreenMainUI;

        private SDKOption _option;
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
            
            _networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
            
            
            _screenContainer.RegisterOnClickCloseScreen(OnClickCloseMainScreen);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _networkClient.UnSubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
        }

        private void HandleOnChangeNetworkClientState(ThetanNetworkClientState networkClientState)
        {
            OnChangeNetworkClientState?.Invoke(networkClientState);
            
            if (networkClientState == ThetanNetworkClientState.NotLoggedIn ||
                networkClientState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                if(_screenContainer.CurrentScreen != null)
                    _screenContainer.PopAllScreen();
            }
        }

        private void OnClickMainAction()
        {
            if (_networkClient.NetworkClientState == ThetanNetworkClientState.NotInitialized)
                return;

            if (!_isVersionSupported)
            {
                _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                    "This Thetan World version is out dated. Please update your application to newest version.", 
                    AuthenErrorMsg.Confirm);
                return;
            }
            
            if (_networkClient.NetworkClientState == ThetanNetworkClientState.Banned)
            {
                _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.AccountBanned, 
                    AuthenErrorMsg.AccountBannedContactSupport, AuthenErrorMsg.Confirm);
            }
            else if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork ||
                     _networkClient.NetworkClientState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                //_uiHelperContainer.ShowPopUpMsg("You are not connected to network",
                //    "Please check your network connection before open Thetan World", 
                //    "Confirm");
                _showPopopWhenLostConnection.ShowPopupNoConnection();
            }
            else if (_networkClient.NetworkClientState == ThetanNetworkClientState.NotLoggedIn)
            {
                ShowLogin(null, null);
            }
            else if(_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedIn)
            {
                ShowMainUI();
            }
        }

        private async UniTask<ScreenMainUI> ShowMainUI()
        {
            if (_screenContainer.CurrentScreen == null)
                _currentScreenMainUI = null;
            
            if (_currentScreenMainUI != null)
                return _currentScreenMainUI;

            if (Utils.GetCurrentScreenType() == Utils.ScreenType.Landscape)
            {
                _screenContainer.EnableButtonCloseScreen();

                var prefab = Resources.Load<GameObject>("MainUIContainer_Landscape");

                var screenPrefab = prefab.GetComponent<Screen>();
                
                var screen = await _screenContainer.PushScreen(screenPrefab) as ScreenMainUI;

                _currentScreenMainUI = screen;
                
                OnOpenMainUI?.Invoke();
                
                return screen;
            }
            else
            {
                _screenContainer.EnableButtonCloseScreen();

                var prefab = Resources.Load<GameObject>("MainUIContainer_Portrait");

                var screenPrefab = prefab.GetComponent<Screen>();
                
                var screen = await _screenContainer.PushScreen(screenPrefab) as ScreenMainUI;

                _currentScreenMainUI = screen;
                
                OnOpenMainUI?.Invoke();
                
                return screen;
            }
        }

        public async void OpenScreenSelectNFT()
        {
            var screen = await ShowMainUI();
            
            if(screen != null)
                screen.ShowTabNFT();
        }

        private void OnClickCloseMainScreen()
        {
            _screenContainer.PopAllScreen();
            
            _currentScreenMainUI = null;
            
            OnCloseMainUI?.Invoke();
        }

        public void Initialize(SDKOption option, Action<ThetanNetworkClientState> onDoneCallback)
        {
            _option = option;
            _networkClient.InitializeNetworkClient(this.Version, (networkClientState) =>
            {
                _remoteConfigService.CallGetRemoteConfig((success) =>
                {
                    if (success)
                    {
                        PostProcessInitialize(networkClientState, onDoneCallback);
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

        public void SetMatchMaxDuration(int matchMaxDuration)
        {
            _matchMaxDuration = matchMaxDuration;
        }

        private async void PostProcessInitialize(ThetanNetworkClientState networkClientState, Action<ThetanNetworkClientState> onDoneCallback)
        {
            StorageResource.Initialize(_networkClient.StorageResourceUrl);

            _isVersionSupported = await CheckVersion();
            
            //Todo: delete me
            _isVersionSupported = true;
            
            _analyticService.InitialzeService(_authenProcessContainer, _networkClient.NetworkConfig);
            
            if (!_isVersionSupported)
            {
                _btnMainAction.Initialize(_showAnimCurrencyFly, _nftItemService, OnClickMainAction);
                onDoneCallback?.Invoke(networkClientState);
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
            
            _btnMainAction.Initialize(_showAnimCurrencyFly, _nftItemService, OnClickMainAction);

            _isInitialized = true;

            if (_option.AutoShowPopupWhenLostConnection)
            {
                _showPopopWhenLostConnection.Initialize(_networkClient, _uiHelperContainer, () =>
                {
                    _screenContainer.PopAllScreen();
                });
            }
            
            _analyticService.LogLoginSuccess(new PostAuthenSuccessMetaData());
            onDoneCallback?.Invoke(networkClientState);
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

                var version = Version;

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
        
        public void ShowLogin(Action<AuthenResultData> onLoginSuccessCallback, Action onUserCancelCallback)
        {
            if (_currentAuthenUIControl != null)
            {
                CommonLog.LogError("Another Login UI has already opened, cannot open another, skipped");
                return;
            }
            
            if (!_isVersionSupported)
            {
                _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                    "This Thetan World version is out dated. Please update your application to newest version.", 
                    AuthenErrorMsg.Confirm);
                return;
            }
            
            _currentAuthenUIControl = Instantiate(_prefabAuthenUIControl, this.transform);
            _currentAuthenUIControl.transform.SetAsLastSibling();
            _currentAuthenUIControl.ShowUISelectLoginMethodWithCloseButton(_option.UseFullscreenLogin,
                _networkClient, _authenProcessContainer, (authResult) =>
            {
                Destroy(_currentAuthenUIControl.gameObject);
                _currentAuthenUIControl = null;
                onLoginSuccessCallback?.Invoke(authResult);
            }, () =>
            {
                Destroy(_currentAuthenUIControl.gameObject);
                _currentAuthenUIControl = null;
                onUserCancelCallback?.Invoke();
            });
        }
        
        public void ShowLinkAccount(Action<AuthenResultData> onSuccessCallback, Action onCancelCallback)
        {
            if (_networkClient.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                Debug.LogError("You must loggin first before use link account");
                onCancelCallback?.Invoke();
                return;
            }
            
            if (!_isVersionSupported)
            {
                _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                    "This Thetan World version is out dated. Please update your application to newest version.", 
                    AuthenErrorMsg.Confirm);
                return;
            }
            
            _currentAuthenUIControl = Instantiate(_prefabAuthenUIControl, this.transform);
            _currentAuthenUIControl.transform.SetAsLastSibling();
            _currentAuthenUIControl.ShowUILinkAccount(_option.UseFullscreenLogin,
                _networkClient, _authenProcessContainer, (authResult) =>
            {
                Destroy(_currentAuthenUIControl.gameObject);
                _currentAuthenUIControl = null;
                onSuccessCallback?.Invoke(authResult);
            }, () =>
            {
                Destroy(_currentAuthenUIControl.gameObject);
                _currentAuthenUIControl = null;
                onCancelCallback?.Invoke();
            });
        }

        public void LogOut()
        {
            _networkClient.LogOut();
            _analyticService.ClearDataService();
            _profileService.ClearDataService();
            _nftItemService.ClearDataService();
            OnUserLogOutCallback?.Invoke();
        }

        public void ShowButtonMainAction()
        {
            _btnMainAction.gameObject.SetActive(true);
        }

        public void HideButtonMainAction()
        {
            _btnMainAction.gameObject.SetActive(false);
            _screenContainer.PopAllScreen();
        }

        #region NFT HERO SERVICE API
        public string SelectedHeroNftId => _nftItemService.SelectedHeroNftId;

        public bool IsSelectedAnyHeroNftItem => !string.IsNullOrEmpty(_nftItemService.SelectedHeroNftId);

        public string GrindingHeroNftId => _nftItemService.GrindingHeroNftId;

        public bool IsGrindingAnyHeroNftItem => !string.IsNullOrEmpty(_nftItemService.GrindingHeroNftId);

        public bool CheckHeroIsSelected(string heroId) => _nftItemService.IsHeroNftSelected(heroId);

        public async UniTask<HeroNftItem> GetHeroNftItemInfo(string id)
        {
            if (_nftItemService.ListHeroNftItems != null)
            {
                foreach (var item in _nftItemService.ListHeroNftItems)
                {
                    if (item.id == id)
                        return item;
                }
            }
            
            if (!_isVersionSupported)
            {
                return new HeroNftItem().SetDefault();
            }

            UniTaskCompletionSource<HeroNftItem> loadItemCompletionSource = new UniTaskCompletionSource<HeroNftItem>();
            
            _nftItemService.GetInfoDataHeroNftOnServer(id, data =>
            {
                loadItemCompletionSource.TrySetResult(data);
            }, error => loadItemCompletionSource.TrySetResult(new HeroNftItem()));

            return await loadItemCompletionSource.Task;
        }

        public HeroNftItem TryGetCachedHeroNftItemByIngameId(string ingameHeroId)
        {
            return _nftItemService.GetCachedHeroNftItemByIngameId(ingameHeroId);
        }

        public void RefreshListNFTHero()
        {
            if (!_isVersionSupported)
            {
                return;
            }
            
            _nftItemService.RefetchListHeroNFT(null, null);
        }

        public void RefreshHeroNftData(string nftId, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (!_isVersionSupported)
            {
                return;
            }
            
            _nftItemService.RefreshDataHeroNft(nftId, onSuccessCallback, onErrorCallback);
        }
        
        public void RegisterOnChangeSelectedHeroNft(Action<string> callback) =>
            _nftItemService.RegisterOnChangeSelectedNftHeroCallback(callback);
        
        public void UnRegisterOnChangeSelectedHeroNft(Action<string> callback) =>
            _nftItemService.UnRegisterOnChangeSelectedNftHeroCallback(callback);
        
        public void PrepareMatchForSelectedNFT(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (!_isVersionSupported)
            {
                _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                    "This Thetan World version is out dated. Please update your application to newest version.", 
                    AuthenErrorMsg.Confirm);
                onErrorCallback?.Invoke(new WolffunResponseError((int)NftItemServiceErrorCode.SDK_VERSION_NOT_SUPPORTED,
                    "This Thetan World version is out dated. Please update your application to newest version."));
                return;
            }
            
            _nftItemService.StartMatch(_matchMaxDuration, _ => onSuccessCallback?.Invoke(), error =>
            {
                onErrorCallback?.Invoke(error);
                HandleAnalyticErrorPrepareBattle(error, false);
            });
        }

        public void PrepareMatchForSelectedNFTAutoHandleError(Action<NftItemServiceErrorCode> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            _nftItemService.StartMatch(_matchMaxDuration, _ => onSuccessCallback?.Invoke(NftItemServiceErrorCode.SUCCESS),
                error =>
                {
                    if (error.Code == (int)WSErrorCode.ServerMaintenance)
                    {
                        _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_SERVER_MAINTAIN,
                            "Continue", "Go back",
                            () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                            () =>
                            {
                                onErrorCallback?.Invoke(error);
                            });
                        return;
                    }
                    
                    switch ((NftItemServiceErrorCode)error.Code)
                    {
                        case NftItemServiceErrorCode.NFT_IS_GRINDING_IN_ANOTHER_GAME:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_GRINDING_IN_ANOTHER_GAME,
                                "Continue", "Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.ANOTHER_NFT_IS_GRINDING:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_OTHER_NFT_IS_GRINDING,
                                "Continue","Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.HERO_MAX_GRIND_STAGE:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_MAX_GRIND_STAGE,
                                "Continue","Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.NFT_DAILY_LIMIT_REACH:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_DAILY_LIMIT,
                                "Continue","Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.NOT_SELECTED_NFT_HERO:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NOT_SELECT_NFT,
                                "Continue", "Select NFT",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.NFT_NOT_MINT:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_NOT_MINT,
                                "Continue", "Select NFT",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.USER_NOT_OWN_NFT:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_NOT_OWNED,
                                "Continue", "Select other NFT",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        default:
                            _uiHelperContainer.ShowPopUpMsg("Error", 
                                ZString.Format(NFTServiceErrorMsg.ERROR_PREPARE_MATCH_UNKNOWN_ERROR, error.Code, error.Message),
                                "Go back", "Continue",
                                () =>
                                {
                                    onErrorCallback?.Invoke(error);
                                }, () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code));
                            break;
                    }

                    HandleAnalyticErrorPrepareBattle(error, true);
                });
        }

        private void HandleAnalyticErrorPrepareBattle(WolffunResponseError error, bool isShowPopup)
        {
            var errorType = (NftItemServiceErrorCode)error.Code;

            if (errorType == NftItemServiceErrorCode.NFT_DAILY_LIMIT_REACH)
                return;

            var analyticService = ThetanSDKManager.Instance._analyticService;
            
            analyticService.LogErrorOccured("Prepare Battle", "Prepare Battle", isShowPopup, 
                errorType == NftItemServiceErrorCode.UNKNOWN ? error.DevDebugMessage : error.Message);
        }

        public void StartGrindingHeroItem()
        {
            _nftItemService.StartGrind();
        }

        public void PauseGrindingHeroItem()
        {
            _nftItemService.StopGrind();
        }

        public void StopGrindingHeroItem()
        {
            _nftItemService.EndMatch(null, null);
        }

        public void UnlockButtonMain()
        {
            _btnMainAction.UnlockButtonAndDoAnimReward();
        }
        #endregion

        internal void HandleMaintenance()
        {
            _uiHelperContainer.ShowPopUpMsg("Maintenance", 
                "Thetan World is in maintenance. Please come back later.",
                "Confirm", () => _screenContainer.PopAllScreen());
        }
        
        private void RegisterAuthenProcessToNetworkClient()
        {
            _networkClient.RegisterAuthenProcess(_authenProcessContainer.WFIDAuthenProcess);
            _networkClient.RegisterAuthenProcess(_authenProcessContainer.ThetanAppAuthenProcess);
        }
    }
}

