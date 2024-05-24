using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using ThetanSDK.UI;
using ThetanSDK.UI.LuckySpin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class ScreenMainUIPortrait : ScreenMainUI
    {
        [SerializeField] private Canvas _mainContentCanvas;
        [SerializeField] private ScreenContainer _contentScreenContainer;
        [SerializeField] private Button _btnSelectNow;
        
        [Header("Profile")]
        [SerializeField] private Button _btnProfile;
        [SerializeField] private Screen _prefabScreenProfile;
        [SerializeField] private UserProfileAvatar _profileAvatar;
        [SerializeField] private TextMeshProUGUI _txtUserName;
        
        [Header("NFT Tab")]
        [SerializeField] private Button _btnNFT;
        //[SerializeField] private NFTHeroAvatar _nftHeroAvatar;
        [SerializeField] private Screen _prefabScreenListNFT;
        
        [Header("Lucy Spin Tab")]
        [SerializeField] private Button _btnLuckySpin;
        [SerializeField] private Screen _prefabScreenLuckySpin;
        
        [Header("Tournament Tab")]
        [SerializeField] private Button _btnTournament;

        private void Awake()
        {
            _btnLuckySpin.onClick.AddListener(ShowTabLuckySpin);
            _btnNFT.onClick.AddListener(ShowTabNFT);
            _btnSelectNow.onClick.AddListener(ShowTabNFT);
            _btnTournament.onClick.AddListener(ShowTabTournament);
            _btnProfile.onClick.AddListener(ShowTabProfile);
            
            
            _contentScreenContainer.RegisterOnClickCloseScreen(OnUserCloseScreen);
            _contentScreenContainer.OnBeforePopScreen += CheckShowMainContentBeforePop;
            _contentScreenContainer.OnAfterPushScreen += CheckShowMainContentAfterPush;
        }

        private void CheckShowMainContentBeforePop(Screen screen)
        {
            if (_contentScreenContainer != null)
            {
                _mainContentCanvas.enabled = _contentScreenContainer.CountTotalScreenInStack <= 1;
            }
        }
        
        private void CheckShowMainContentAfterPush(Screen screen)
        {
            _mainContentCanvas.enabled = false;
        }

        private void OnDestroy()
        {
            _contentScreenContainer.UnRegisterOnClickCloseScreen(OnUserCloseScreen);
            
            _contentScreenContainer.OnBeforePopScreen -= CheckShowMainContentBeforePop;
            _contentScreenContainer.OnAfterPushScreen -= CheckShowMainContentAfterPush;
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            ThetanSDKManager.Instance.RegisterOnChangeSelectedHeroNft(OnChangeSelectedHeroNFT);
            _contentScreenContainer.EnableButtonCloseScreen();
            
            OnChangeSelectedHeroNFT(ThetanSDKManager.Instance.SelectedHeroNftId);
            ShowUIProfile();
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();
            
            ThetanSDKManager.Instance.UnRegisterOnChangeSelectedHeroNft(OnChangeSelectedHeroNFT);
            
            _contentScreenContainer.PopAllScreen();
        }

        private async void OnChangeSelectedHeroNFT(string nftId)
        {
            /*
            if (string.IsNullOrEmpty(nftId))
            {
                _nftHeroAvatar.gameObject.SetActive(false);
            }
            else
            {
                _nftHeroAvatar.gameObject.SetActive(false);
                var nftHeroInfo = await ThetanSDKManager.Instance.GetHeroNftItemInfo(nftId);
                _nftHeroAvatar.ShowUI(nftHeroInfo.ingameInfo);
                _nftHeroAvatar.gameObject.SetActive(true);
            }
            */
        }

        private void ShowUIProfile()
        {
            var profileService = ThetanSDKManager.Instance.ProfileService;
            _profileAvatar.SetUI(profileService.AvatarId, profileService.AvatarFrameId);
            _txtUserName.SetText(profileService.Username);
        }

        private async void ShowTabProfile()
        {
            var screen = await _contentScreenContainer.PushScreen(_prefabScreenProfile) as ScreenContentMainUI;
            screen.Initialize(_uiHelperContainer);
        }
        
        public override async void ShowTabLuckySpin()
        {
            var screen = await _contentScreenContainer.ReplaceScreenStackByScreen(_prefabScreenLuckySpin) as ScreenLuckySpin;
            screen.Initialize(_uiHelperContainer);
        }

        public override async void ShowTabNFT()
        {
            var screen = await _contentScreenContainer.ReplaceScreenStackByScreen(_prefabScreenListNFT) as ScreenContentMainUI;
            screen.Initialize(_uiHelperContainer);
        }

        public override void ShowTabTournament()
        {
            _uiHelperContainer.ShowToast("Coming Soon");
        }
        
        private void OnUserConfirmLogOut()
        {
            _screenContainer.NotifyOnClickCloseScreen();
            ThetanSDKManager.Instance.LogOut();
        }

        private void OnUserCloseScreen()
        {
            _screenContainer.NotifyOnClickCloseScreen();
        }
    }
}
