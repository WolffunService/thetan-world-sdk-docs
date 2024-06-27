using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;
using Screen = ThetanSDK.UI.Screen;

namespace ThetanSDK.UI
{
    public class ScreenAccountInfo : ScreenContentMainUI
    {
        [SerializeField] private UserProfileAvatar _userProfileAvatar;
        [SerializeField] private TextMeshProUGUI _txtUserName;
        [SerializeField] private TextMeshProUGUI _txtUserId;
        [SerializeField] private Button _btnCopyUserId;
        [SerializeField] private GameObject _contentEmail;
        [SerializeField] private TextMeshProUGUI _txtEmail;
        [SerializeField] private TextMeshProUGUI _txtGrindTime;
        [SerializeField] private TextMeshProUGUI _txtGrindReward;
        [SerializeField] private TextMeshProUGUI _txtNftUsed;
        [SerializeField] private TextMeshProUGUI _txtNewSpin;
        [SerializeField] private LogoutUIFunction _logOutUIFunction;
        [SerializeField] private Button _btnMore;

        private bool _needToRefresh = false;
        
        private void Awake()
        {
            _btnCopyUserId.onClick.AddListener(OnClickCopyUserId);
            _btnMore.onClick.AddListener(OnClickMoreInfo);
        }

        
        public override void Initialize(UIHelperContainer uiHelperContainer)
        {
            base.Initialize(uiHelperContainer);
            
            _logOutUIFunction.Initialize(_uiHelperContainer, OnUserConfirmLogOut);
        }
        
        private void OnClickMoreInfo()
        {
            Application.OpenURL(ThetanSDKManager.Instance.RemoteConfigService.RemoteConfig.mkpUrlConfig.urlTabGrind);
        }

        private void OnClickCopyUserId()
        {
            var profileService = ThetanSDKManager.Instance.ProfileService;
            
            profileService.UserId.CopyToClipboard();
            
            _uiHelperContainer.ShowToast("Copied User ID to clipboard");
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();

            bool isEnableBtnMore =
                ThetanSDKManager.Instance.RemoteConfigService.RemoteConfig.mkpUrlConfig.enableBtnMore;
            _btnMore.gameObject.SetActive(isEnableBtnMore);

            var profileService = ThetanSDKManager.Instance.ProfileService;
            
            _userProfileAvatar.SetUI(profileService.AvatarId, profileService.AvatarFrameId);
            _txtUserName.SetText(profileService.Username);
            _txtUserId.SetText(
                ThetanSDKUtilities.ConvertMiddleStringToElipsis(profileService.UserId, 
                    7, 6));

            if (_contentEmail != null)
            {
                if (string.IsNullOrEmpty(profileService.Email))
                {
                    _contentEmail.SetActive(false);
                }
                else
                {
                    _contentEmail.SetActive(true);
                    
                    if(_txtEmail)
                        _txtEmail.SetText(profileService.Email);
                }
            }
            
            ShowUITodayPerformance();
        }

        public override void OnReFocusScreen()
        {
            base.OnReFocusScreen();

            ShowUITodayPerformance();
        }

        private void ShowUITodayPerformance()
        {
            var nftItemService = ThetanSDKManager.Instance.NftItemService;

            var cachedStatisticData = nftItemService.CacheNftStatisticData;

            SetDataStatistic(cachedStatisticData);
            
            nftItemService.FetchGrindNFTStatisticData(SetDataStatistic, HandleFetchDataError);
        }

        private void SetDataStatistic(GrindNFTStatisticData statisticData)
        {
            _txtGrindTime.SetText(ThetanSDKUtilities.ToStringTime(statisticData.grindTimeToday));
            _txtGrindReward.SetText(statisticData.grindRewardToday.FormatUnitCurrency());
            _txtNftUsed.SetText("{0}", statisticData.nftUsed);
            _txtNewSpin.SetText("+{0}", statisticData.incSpinChanceToday);
        }

        private void HandleFetchDataError(WolffunResponseError error)
        {
            if (error.Code == (int)WSErrorCode.UserBanned)
            {
                _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.AccountBanned, 
                    AuthenErrorMsg.AccountBannedContactSupport, AuthenErrorMsg.Confirm, () =>
                    {
                        _screenContainer.NotifyOnClickCloseScreen();
                    });
                return;
            }
            
            if (error.Code == (int)WSErrorCode.ServerMaintenance)
            {
                ThetanSDKManager.Instance.HandleMaintenance();
                return;
            }
        }

        private void OnUserConfirmLogOut()
        {
            _screenContainer.NotifyOnClickCloseScreen();
            ThetanSDKManager.Instance.LogOut();
        }
    }
}
