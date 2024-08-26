using System;
using Cysharp.Threading.Tasks;
using ThetanSDK.UI;
using ThetanSDK.UI.Connection;
using UnityEngine;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;
using Screen = ThetanSDK.UI.Screen;

namespace ThetanSDK
{
    /// <summary>
    /// Class manage SDK main UI behavior
    /// </summary>
    internal class ThetanSDKManagerHandleUI : IDisposable
    {
        private NetworkClient _networkClient;
        private ScreenContainer _screenContainer;
        private UIHelperContainer _uiHelperContainer;
        private ShowPopupWhenLostConnection _showPopupWhenLostConnection;
        private Transform _rootTransform;
        private AuthenUIControl _prefabAuthenUIControl;
        private AuthenProcessContainer _authenProcessContainer;
        private Action _onOpenMainScreenCallback;
        private Action _onCloseMainScreenCallback;
        
        
        private AuthenUIControl _currentAuthenUIControl;
        private ScreenMainUI _currentScreenMainUI;

        internal ThetanSDKManagerHandleUI(
            NetworkClient networkClient,
            ScreenContainer screenContainer,
            UIHelperContainer uiHelperContainer,
            ShowPopupWhenLostConnection showPopupWhenLostConnection,
            Transform rootTransform,
            AuthenUIControl prefabAuthenUIControl,
            AuthenProcessContainer authenProcessContainer,
            Action onOpenMainScreenCallback,
            Action onCloseMainScreenCallback)
        {
            _networkClient = networkClient;
            _screenContainer = screenContainer;
            _uiHelperContainer = uiHelperContainer;
            _showPopupWhenLostConnection = showPopupWhenLostConnection;
            _rootTransform = rootTransform;
            _prefabAuthenUIControl = prefabAuthenUIControl;
            _authenProcessContainer = authenProcessContainer;
            _onOpenMainScreenCallback = onOpenMainScreenCallback;
            _onCloseMainScreenCallback = onCloseMainScreenCallback;
        }

        private void HandleOnChangeNetworkClientState(ThetanNetworkClientState networkClientState)
        {
            if (networkClientState == ThetanNetworkClientState.NotLoggedIn ||
                networkClientState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                if(_screenContainer.CurrentScreen != null)
                    _screenContainer.PopAllScreen();
            }
        }
        
        internal void OnClickMainAction()
        {
            if (_networkClient.NetworkClientState == ThetanNetworkClientState.NotInitialized)
                return;

            if (!ThetanSDKManager.Instance.IsVersionSupported)
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
                _showPopupWhenLostConnection.ShowPopupNoConnection();
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
        
        internal void ShowLogin(Action<AuthenResultData> onLoginSuccessCallback, Action onUserCancelCallback)
        {
            if (_currentAuthenUIControl != null)
            {
                CommonLog.LogError("Another Login UI has already opened, cannot open another, skipped");
                return;
            }
            
            if (!ThetanSDKManager.Instance.IsVersionSupported)
            {
                _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                    "This Thetan World version is out dated. Please update your application to newest version.", 
                    AuthenErrorMsg.Confirm);
                return;
            }
            
            _currentAuthenUIControl = GameObject.Instantiate(_prefabAuthenUIControl, _rootTransform);
            _currentAuthenUIControl.transform.SetAsLastSibling();
            _currentAuthenUIControl.ShowUISelectLoginMethodWithCloseButton(
                _networkClient, _authenProcessContainer, (authResult) =>
                {
                    GameObject.Destroy(_currentAuthenUIControl.gameObject);
                    _currentAuthenUIControl = null;
                    onLoginSuccessCallback?.Invoke(authResult);
                }, () =>
                {
                    GameObject.Destroy(_currentAuthenUIControl.gameObject);
                    _currentAuthenUIControl = null;
                    onUserCancelCallback?.Invoke();
                });
        }
        
        internal void ShowLinkAccount(Action<AuthenResultData> onSuccessCallback, Action onCancelCallback)
        {
            if (_networkClient.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                Debug.LogError("You must loggin first before use link account");
                onCancelCallback?.Invoke();
                return;
            }
            
            if (!ThetanSDKManager.Instance.IsVersionSupported)
            {
                ShowPopupVersionNotSupported();
                return;
            }
            
            _currentAuthenUIControl = GameObject.Instantiate(_prefabAuthenUIControl, _rootTransform);
            _currentAuthenUIControl.transform.SetAsLastSibling();
            _currentAuthenUIControl.ShowUILinkAccount(_networkClient, _authenProcessContainer, (authResult) =>
                {
                    GameObject.Destroy(_currentAuthenUIControl.gameObject);
                    _currentAuthenUIControl = null;
                    onSuccessCallback?.Invoke(authResult);
                }, () =>
                {
                    GameObject.Destroy(_currentAuthenUIControl.gameObject);
                    _currentAuthenUIControl = null;
                    onCancelCallback?.Invoke();
                });
        }
        
        internal async UniTask<ScreenMainUI> ShowMainUI()
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
                
                _onOpenMainScreenCallback?.Invoke();
                
                return screen;
            }
            else
            {
                _screenContainer.EnableButtonCloseScreen();

                var prefab = Resources.Load<GameObject>("MainUIContainer_Portrait");

                var screenPrefab = prefab.GetComponent<Screen>();
                
                var screen = await _screenContainer.PushScreen(screenPrefab) as ScreenMainUI;

                _currentScreenMainUI = screen;
                
                _onOpenMainScreenCallback?.Invoke();
                
                return screen;
            }
        }

        internal async void OpenScreenSelectNFT()
        {
            var screen = await ShowMainUI();
            
            if(screen != null)
                screen.ShowScreenListNFT();
        }
        
        internal void CloseMainUI()
        {
            _screenContainer.PopAllScreen();
            
            _currentScreenMainUI = null;
            
            _onCloseMainScreenCallback?.Invoke();
        }

        internal void ShowPopupMaintenance()
        {
            _uiHelperContainer.ShowPopUpMsg("Maintenance", 
                "Thetan World is in maintenance. Please come back later.",
                "Confirm", () => _screenContainer.PopAllScreen());
        }

        internal void ShowPopupVersionNotSupported()
        {
            _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                "This Thetan World version is out dated. Please update your application to newest version.", 
                AuthenErrorMsg.Confirm);
        }

        public void Dispose()
        {
            if(_networkClient != null)
                _networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
        }
    }
}