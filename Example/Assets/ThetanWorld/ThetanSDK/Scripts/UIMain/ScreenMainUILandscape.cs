using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.Scripts.UITab;
using ThetanSDK.Utilities;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class ScreenMainUILandscape : ScreenMainUI
    {
        [SerializeField] private UITabControler _uiTabController;
        [SerializeField] private List<ScreenContentMainUI> _listScreenEachTab;
        [SerializeField] private Transform _contentParentTransform;
        [SerializeField] private ScreenContainer _prefabScreenContainer;
        [SerializeField] private UserProfileAvatar _userProfileAvatar;
        [SerializeField] private TextMeshProUGUI _txtUserName;    

        [Header("Coming Soon")]
        [SerializeField] private Button _btnTournament;

        private Dictionary<int, ScreenContainer> _dicCachedContainer = new Dictionary<int, ScreenContainer>();

        private ScreenContainer _currentActiveScreenContainer;
        private SpriteCacheManager _spriteCacheManager;
        
        private void Awake()
        {
            _uiTabController.OnSelectItemCallback.AddListener(OnChangeTab);
            _btnTournament.onClick.AddListener(ShowComingSoon);
            _spriteCacheManager = new SpriteCacheManager();
            _userProfileAvatar.SetSpriteCacheManager(_spriteCacheManager);
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            
            _uiTabController.SelectTab(0);

            var profileService = ThetanSDKManager.Instance.ProfileService;
            _userProfileAvatar.SetUI(profileService.AvatarId, profileService.AvatarFrameId);
            _txtUserName.SetText(profileService.Username);
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();
            
            foreach (var screenContainer in _dicCachedContainer)
            {
                screenContainer.Value.PopAllScreen();
                
                screenContainer.Value.UnRegisterOnClickCloseScreen(HandleNotifyCloseScreen);
                
                Destroy(screenContainer.Value.gameObject);
            }
            
            _dicCachedContainer.Clear();
            
            _uiHelperContainer.CloseAllPopup();
        }

        private async void OnChangeTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= _listScreenEachTab.Count)
                return;
            
            ScreenContainer screenContainer = null;
            
            if(_currentActiveScreenContainer != null)
                _currentActiveScreenContainer.gameObject.SetActive(false);

            if (!_dicCachedContainer.TryGetValue(tabIndex, out screenContainer))
            {
                screenContainer = Instantiate(_prefabScreenContainer, _contentParentTransform);

                _dicCachedContainer[tabIndex] = screenContainer;
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(screenContainer.transform as RectTransform);

                var initialScreen = _listScreenEachTab[tabIndex];

                var screenInstance = await screenContainer.PushScreen(initialScreen) as ScreenContentMainUI;
                
                screenContainer.EnableButtonCloseScreen();
                screenContainer.RegisterOnClickCloseScreen(HandleNotifyCloseScreen);
                
                screenInstance.Initialize(_uiHelperContainer);

                SetDataScreen(tabIndex, screenInstance);
            }
            else
            {
                if (_currentActiveScreenContainer != screenContainer &&
                    screenContainer.CurrentScreen is ScreenContentMainUI screenContentMainUI)
                {
                    screenContentMainUI.OnReFocusScreen();
                }
            }

            if (screenContainer == null)
                return;

            screenContainer.gameObject.SetActive(true);
            _currentActiveScreenContainer = screenContainer;
        }

        private void SetDataScreen(int tabIndex, ScreenContentMainUI screen)
        {
            // Switch case is bad. But in our situation, we only have maximum of 3 tab, so this method is acceptable
            switch (tabIndex)
            {
                case 0: // Tab overview
                {
                    var screenOverview = screen as ScreenOverview;
                    
                    if(screenOverview != null)
                        screenOverview.SetCallback(ShowTabNFT);
                    break;
                }
            }
        }
        
        private void OnDestroy()
        {
            foreach (var screenContainer in _dicCachedContainer)
            {
                screenContainer.Value.PopAllScreen();
            }
        }

        private void HandleNotifyCloseScreen()
        {
            _screenContainer.NotifyOnClickCloseScreen();
        }
        
        private void ShowComingSoon()
        {
            _uiHelperContainer.ShowToast("Coming Soon");
        }

        public override void ShowTabLuckySpin()
        {
            _uiTabController.SelectTab(0);
        }

        public override void ShowTabNFT()
        {
            _uiTabController.SelectTab(1);
            
            // If list nft tab is in nft detail screen, pop to root for show nft list
            _currentActiveScreenContainer.PopToRoot();
        }

        public override void ShowTabTournament()
        {
            ShowComingSoon();
        }
    }
}