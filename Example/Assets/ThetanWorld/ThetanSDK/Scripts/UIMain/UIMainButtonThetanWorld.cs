using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.Tweening;

namespace ThetanSDK.UI
{
    internal class UIMainButtonThetanWorld : MonoBehaviour
    {
        private enum ButtonUIState
        {
            None = -1, // Not Initialized
            Normal = 0,
            Hide = 1, // When UIMainButton is hiding while not grinding
            Grinding = 2, // When UIMainButton is grinding
            GrindingHide = 3, // When UIMainButton is grinding while still need to hide
        }
        
        private const float UPDATE_UI_FREE_NFT_SECOND_INTERVAL = 5;
        [SerializeField] private Canvas _canvasButton;
        [SerializeField] private Image _imgIconThetanWorld;
        [SerializeField] private Button _btnOpenThetanWorld;

        [Header("Content Selected NFT")] [SerializeField]
        private GameObject _objIconSelectedHeroNFT;

        [SerializeField] private NFTHeroAvatar _avatarNFT;
        [SerializeField] private Image _imgGrindTimeInDay;
        [SerializeField] private GameObject _contentMaxDailyGrindTime;
        [SerializeField] private Color _colorGrindTimeNormal;
        [SerializeField] private Color _colorGrindTimeLimit;

        [Header("Content No Internet")] 
        [SerializeField] private GameObject _contentNoInternet;
        [SerializeField] private Image _imgRarityBG;
        [SerializeField] private Image _imgIconNFT;
        [SerializeField] private Material _materialGreyScale;

        [Header("Grinding Section")] 
        [SerializeField] private CanvasGroup _canvasGroupBackground;
        [SerializeField] private CanvasGroup _canvasGroupGrindTime;
        [SerializeField] private RectTransform _contentScale;
        [SerializeField] private AnimationIconVictory _animVictoryIcon;

        [SerializeField] private float grindingAnimInterval = 2.5f;
        
        [SerializeField] private float _scaleDownValue = 0.85f; // Use for ButtonUIState.Grinding
        [SerializeField] private float _scaleDownHideValue = 0.6f; // Use for ButtonUIState.GringindHide
        [SerializeField] private float _scaleDuration = 0.15f;
        [SerializeField] private List<MoveImageToDestination> _listAnimImgGrinding;
        
        [SerializeField] private float _currencyScaleDelay;
        [SerializeField] private float _currencyScaleDelta;
        [SerializeField] private float _currencyScaleDuration;

        [SerializeField] private float forceOpenThetanWorldConsecutiveClick = 3;
        [SerializeField] private float forceOpenThetanWorldClicMaxInterval = 0.4f;

        private ButtonUIState _curBtnUIState = ButtonUIState.None;
        
        private TweenSequence _curCurrencyFlySequence;

        private Action _onClickOpenThetanWorld;

        private float _countTimePlayGrindingAnim;
        private float _countTimeUpdateUIFreeNFT;
        private TweenSequence _curAnimSequence;
        private NftItemService _nftItemService;
        private bool _isGrinding;
        private bool _isLockButton;
        private bool _isPendingUnlockButton;
        private HeroNftItem _selectedHeroNFTInfo;
        private bool _isNoInternet;
        private bool _hasPendingRewardToShowUI;
        private ShowAnimCurrencyFly _showAnimCurrencyFly;

        private int _countConsecutiveClick;
        private double _lastTimeClickButton;

        private void Awake()
        {
            _btnOpenThetanWorld.onClick.AddListener(OnClickBtnThetanWorld);

            ThetanSDKManager.Instance.OnOpenMainUI += OnOpenMainUI;
            ThetanSDKManager.Instance.OnCloseMainUI += OnCloseMainUI;
        }
        
        public void Initialize(ShowAnimCurrencyFly animCurrencyFly, NftItemService nftItemService, Action onClickOpenThetanWorld)
        {
            _curBtnUIState = ButtonUIState.None;
            
            SetContentNoInternet(false);
            SetUpUINotLoggedIn();
            
            _onClickOpenThetanWorld = onClickOpenThetanWorld;
            _showAnimCurrencyFly = animCurrencyFly;
            _isGrinding = false;
            _nftItemService = nftItemService;
            nftItemService._onChangeGrindingStatus += OnChangeGrindingStatus;
            nftItemService._onPingGrindSuccess += OnPingGrindSuccess;
            nftItemService._onRefreshFreeNFTInfo += OnFreeNFTChangedData;
            nftItemService._onReceiveVictoryMatch += OnVictoryMatch;
            nftItemService._onListNFTFetchSuccessCallback += OnRefetchListNFT;
            
            ThetanSDKManager.Instance.OnChangeNetworkClientState += OnChangeNetworkClientState;
            nftItemService.RegisterOnChangeNftItemData(OnChangeNFTItemData);
            nftItemService.RegisterOnChangeSelectedNftHeroCallback(OnChangeSelectedNFT);

            OnChangeNetworkClientState(ThetanSDKManager.Instance.NetworkClientState);
        }

        private void Update()
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;

            if (_nftItemService.IsGrinding() &&
                _nftItemService.IsAllowPingGrindingServer)
            {
                _countTimePlayGrindingAnim += Time.unscaledDeltaTime;

                if (_countTimePlayGrindingAnim >= grindingAnimInterval)
                {
                    _countTimePlayGrindingAnim = 0;
                    PlayAnimGrinding();

                    if (_nftItemService.DicHeroNftIdToListIndex != null &&
                        _nftItemService.DicHeroNftIdToListIndex.TryGetValue(_selectedHeroNFTInfo.id, out var index) &&
                        index >= 0 && index < _nftItemService.ListHeroNftItems.Count &&
                        _nftItemService.ListHeroNftItems[index].id == _selectedHeroNFTInfo.id)
                    {
                        _selectedHeroNFTInfo.grindInfo = _nftItemService.ListHeroNftItems[index].grindInfo;
                    }
                    SetUISlider(_selectedHeroNFTInfo);
                }
            }
            else
            {
                _countTimePlayGrindingAnim = 0;
            }

            if (!_selectedHeroNFTInfo.IsEmpty() &&
                _selectedHeroNFTInfo.nftType == NFTType.FreeNFT &&
                !_nftItemService.IsGrinding())
            {
                _countTimeUpdateUIFreeNFT += Time.unscaledDeltaTime;

                if (_countTimeUpdateUIFreeNFT >= UPDATE_UI_FREE_NFT_SECOND_INTERVAL)
                {
                    _countTimeUpdateUIFreeNFT = 0;
                    
                    SetUISlider(_selectedHeroNFTInfo);
                }
            }
        }
        
        private void OnDestroy()
        {
            if (ThetanSDKManager.IsAlive)
            {
                ThetanSDKManager.Instance.OnChangeNetworkClientState -= OnChangeNetworkClientState;

                var nftItemService = ThetanSDKManager.Instance.NftItemService;

                if (nftItemService != null)
                {
                    nftItemService.UnRegisterOnChangeNftItemData(OnChangeNFTItemData);
                    nftItemService.UnRegisterOnChangeSelectedNftHeroCallback(OnChangeSelectedNFT);
                    
                    nftItemService._onChangeGrindingStatus -= OnChangeGrindingStatus;
                    nftItemService._onRefreshFreeNFTInfo -= OnFreeNFTChangedData;
                    nftItemService._onReceiveVictoryMatch -= OnVictoryMatch;
                    nftItemService._onListNFTFetchSuccessCallback -= OnRefetchListNFT;
                }
            }

            if (_nftItemService != null)
            {
                _nftItemService._onChangeGrindingStatus -= OnChangeGrindingStatus;
            }
            
            if(ThetanSDKManager.IsAlive)
            {
                ThetanSDKManager.Instance.OnOpenMainUI -= OnOpenMainUI;
                ThetanSDKManager.Instance.OnCloseMainUI -= OnCloseMainUI;
            }
        }

        public void Show()
        {
            if(_curBtnUIState == ButtonUIState.Hide)
                ChangeUIState(ButtonUIState.Normal);
            else if (_curBtnUIState == ButtonUIState.GrindingHide)
                ChangeUIState(ButtonUIState.Grinding);
            else
                ChangeUIState(ButtonUIState.Normal);
        }

        public void Hide()
        {
            if(_curBtnUIState == ButtonUIState.Normal)
                ChangeUIState(ButtonUIState.Hide);
            else if (_curBtnUIState == ButtonUIState.Grinding)
                ChangeUIState(ButtonUIState.GrindingHide);
            else
                ChangeUIState(ButtonUIState.Hide);
        }

        private void OnCloseMainUI()
        {
            _canvasButton.enabled = true;
        }

        private void OnOpenMainUI()
        {
            _canvasButton.enabled = false;
        }

        private void OnClickBtnThetanWorld()
        {
            var prevTimeClickButon = _lastTimeClickButton;
            _lastTimeClickButton = Time.unscaledTime;
            if (_isLockButton)
            {
                // if (Time.time - prevTimeClickButon <= forceOpenThetanWorldClicMaxInterval ||
                //     _countConsecutiveClick <= 0)
                // {
                //     _countConsecutiveClick++;
                //     Debug.Log("_countConsecutiveClick " + _countConsecutiveClick);
                //
                //     if (_countConsecutiveClick >= forceOpenThetanWorldConsecutiveClick)
                //     {
                //         _countConsecutiveClick = 0;
                //         
                //         ThetanSDKManager.Instance.RootUIHelperContainer.ShowPopUpMsg("Warning", 
                //             "There is NFT is grinding, do you want to force stop grinding and open Thetan World",
                //             "No", "Force stop grinding",
                //             null, () =>
                //             {
                //                 ThetanSDKManager.Instance.StopGrindingHeroItem();
                //                 _onClickOpenThetanWorld?.Invoke();
                //             });
                //         
                //         return;
                //     }
                // }
                // else
                // {
                //     _countConsecutiveClick = 1;
                // }

                return;
            }

            _countConsecutiveClick = 0;
            
            _onClickOpenThetanWorld?.Invoke();
        }

        private void OnPingGrindSuccess()
        {
            _isGrinding = true;

            _hasPendingRewardToShowUI = true;

            if (_curBtnUIState == ButtonUIState.Hide)
                ChangeUIState(ButtonUIState.GrindingHide);
        }

        public void UnlockButtonAndDoAnimReward()
        {
            if (_nftItemService.IsGrinding())
            {
                _isPendingUnlockButton = true;
                return;
            }
            
            if (_isGrinding)
            {
                _isGrinding = false;
                if(_curBtnUIState == ButtonUIState.Grinding)
                    ChangeUIState(ButtonUIState.Normal);
                else if(_curBtnUIState == ButtonUIState.GrindingHide)
                    ChangeUIState(ButtonUIState.Hide);
                else
                    ChangeUIState(ButtonUIState.Normal);
            }

            _isLockButton = false;

            if (_hasPendingRewardToShowUI)
                DoAnimReward();
        }

        private void DoAnimReward()
        {
            _hasPendingRewardToShowUI = false;

            var defaullContentSize = Vector3.one;
            _showAnimCurrencyFly.DoAnimCurrencyFly(this.transform as RectTransform);
            if(_curCurrencyFlySequence != null && !_curCurrencyFlySequence.IsComplete)
                _curCurrencyFlySequence.Kill();

            _curCurrencyFlySequence = WolfTween.GetSequence();

            _curCurrencyFlySequence.Append(DOVirtual.DelayedCall(_currencyScaleDelay, () => { }));
            for (int i = 0; i < 2; i++)
            {
                _curCurrencyFlySequence.Append(_contentScale
                    .DOScale(defaullContentSize * _currencyScaleDelta, _currencyScaleDuration * 0.5f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));
                _curCurrencyFlySequence.Append(_contentScale
                    .DOScale(defaullContentSize, _currencyScaleDuration * 0.5f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));
            }

            _curCurrencyFlySequence.SetUpdate(true);
        }

        private void OnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.NotLoggedIn)
            {
                _hasPendingRewardToShowUI = false;
                _isGrinding = false;
                _isLockButton = false;
                // gameObject.SetActive(true); Do not need to control the state of button, it will be automatic handled by ChangeUIState Flow
                SetUpUINotLoggedIn();
                SetContentNoInternet(false);
                return;
            }
            else if (newState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                _hasPendingRewardToShowUI = false;
                _isGrinding = false;
                _isLockButton = false;
                // gameObject.SetActive(true);
                SetUpUINotLoggedIn();
                SetContentNoInternet(true);
                return;
            }


            if (newState == ThetanNetworkClientState.LoggedIn)
            {
                //gameObject.SetActive(true);
                SetUpUISDKLoggedIn();
                SetContentNoInternet(false);
            }
            else if (newState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                //gameObject.SetActive(true);
                SetUpUISDKLoggedIn();
                SetContentNoInternet(true);
            }
        }

        private async void SetUpUISDKLoggedIn()
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                return;
            }

            var nftService = ThetanSDKManager.Instance.NftItemService;

            if (nftService.IsSelectedAnyHeroNFT())
            {
                nftService.GetInfoDataHeroNftOnServer(nftService.SelectedHeroNftId, SetUISelectedNFT,
                    error => { SetUpUISelectNFT(); });
            }
            else
            {
                SetUpUISelectNFT();
            }
        }

        private void SetUpUINotLoggedIn()
        {
            _objIconSelectedHeroNFT.SetActive(false);
            _imgIconThetanWorld.enabled = true;
        }

        private void SetUpUISelectNFT()
        {
            _selectedHeroNFTInfo = default;
            _objIconSelectedHeroNFT.SetActive(false);
            _imgIconThetanWorld.enabled = true;
        }

        private async void OnChangeSelectedNFT(string selectedNFTId)
        {
            if (string.IsNullOrEmpty(selectedNFTId))
            {
                _selectedHeroNFTInfo = default;
                SetUpUISelectNFT();
                SetContentGreyScale();
            }
            else
            {
                var heroInfo = await ThetanSDKManager.Instance.GetHeroNftItemInfo(selectedNFTId);

                if (heroInfo.IsEmpty()) // Some error occured
                    SetUpUISelectNFT();
                else
                    SetUISelectedNFT(heroInfo);
                
                SetContentGreyScale();
            }
            
        }

        private void OnChangeNFTItemData(HeroNftItem data)
        {
            var selectedNFTId = ThetanSDKManager.Instance.SelectedHeroNftId;

            if (selectedNFTId == data.id)
            {
                SetUISelectedNFT(data);
                SetContentGreyScale();
            }
        }
        
        private async void OnFreeNFTChangedData(FreeNFTInfo freeNFT)
        {
            if (_nftItemService.SelectedHeroNftId == freeNFT.nftId && 
                !string.IsNullOrEmpty(freeNFT.nftId))
            {
                var nftItemInfo = await ThetanSDKManager.Instance.GetHeroNftItemInfo(freeNFT.nftId);
                _selectedHeroNFTInfo = nftItemInfo;
                SetUISelectedNFT(nftItemInfo);
                SetContentGreyScale();
            }
        }

        private void OnVictoryMatch()
        {
            _animVictoryIcon.PlayAnimation();
        }

        private async void OnRefetchListNFT()
        {
            
            if (!string.IsNullOrEmpty(ThetanSDKManager.Instance.SelectedHeroNftId))
            {
                var nftItemInfo = await ThetanSDKManager.Instance.GetHeroNftItemInfo(ThetanSDKManager.Instance.SelectedHeroNftId);
                _selectedHeroNFTInfo = nftItemInfo;
                SetUISelectedNFT(nftItemInfo);
                SetContentGreyScale();
            }
        }

        private void SetUISelectedNFT(HeroNftItem data)
        {
            _selectedHeroNFTInfo = data;
            _objIconSelectedHeroNFT.SetActive(true);
            _imgIconThetanWorld.enabled = false;
            _avatarNFT.ShowUI(data.ingameInfo);

            SetUISlider(data);

            SetContentGreyScale();
        }

        private void SetUISlider(HeroNftItem data)
        {
            var grindTime = data.grindInfo.grindTime;
            var maxGrindTime = data.grindInfo.maxGrindTime;
            bool isMaxDailyGrindTime = grindTime >= maxGrindTime;
            if (data.nftType == NFTType.FreeNFT)
            {
                var freeNftGrindTimeInfo = ThetanSDKUtilities.ConvertFreeNFTInfoToGrindTimeInfo(
                    ThetanSDKManager.Instance.NftItemService.FreeNftInfo,
                    data);

                maxGrindTime = freeNftGrindTimeInfo.maxTime;
                isMaxDailyGrindTime = freeNftGrindTimeInfo.grindTime >= maxGrindTime;
            }
            

            if (_imgGrindTimeInDay != null)
            {
                var percentGrindTime = grindTime/ maxGrindTime;
                percentGrindTime = Mathf.Clamp01(percentGrindTime);
                _imgGrindTimeInDay.fillAmount = percentGrindTime;
                _imgGrindTimeInDay.color = isMaxDailyGrindTime ? _colorGrindTimeLimit : _colorGrindTimeNormal;
            }

            if (_contentMaxDailyGrindTime != null)
            {
                _contentMaxDailyGrindTime.SetActive(isMaxDailyGrindTime || data.grindInfo.IsMaxLifeTime());
            }
        }

        private void SetContentNoInternet(bool isNoInternet)
        {
            _contentNoInternet.SetActive(isNoInternet);
            _isNoInternet = isNoInternet;

            SetContentGreyScale();
        }

        private void SetContentGreyScale()
        {
            // bool isMaxDailyGrindTime = !string.IsNullOrEmpty(_selectedHeroNFTInfo.id) &&
            //                            _selectedHeroNFTInfo.grindInfo.grindTime >=
            //                            _selectedHeroNFTInfo.grindInfo.maxGrindTime;
            bool isLimitStage = _selectedHeroNFTInfo.grindInfo.IsMaxLifeTime();
            var materialImg = isLimitStage || _isNoInternet ? _materialGreyScale : null;

            _imgRarityBG.material = materialImg;
            _imgIconThetanWorld.material = materialImg;
            _imgIconNFT.material = materialImg;

            if (_imgGrindTimeInDay)
                _imgGrindTimeInDay.material = materialImg;
        }

        private void OnChangeGrindingStatus(bool isGrinding)
        {
            // if (_isGrinding == isGrinding)
            //     return;

            _isGrinding = isGrinding;
            if (isGrinding)
            {
                if(_curBtnUIState == ButtonUIState.Normal)
                    ChangeUIState(ButtonUIState.Grinding);
                else if (_curBtnUIState == ButtonUIState.Hide)
                    ChangeUIState(ButtonUIState.GrindingHide);
                else
                    ChangeUIState(ButtonUIState.Grinding);
            }
            else
            {
                if(_curBtnUIState == ButtonUIState.Grinding)
                    ChangeUIState(ButtonUIState.Normal);
                else if (_curBtnUIState == ButtonUIState.GrindingHide)
                    ChangeUIState(ButtonUIState.Hide);
                else
                    ChangeUIState(ButtonUIState.Normal);
            }
        }

        private void ChangeUIState(ButtonUIState newState)
        {
            if (_curBtnUIState == newState)
                return;

            _curBtnUIState = newState;

            switch (newState)
            {
                case ButtonUIState.Normal:
                    ChangeUIToNormal();
                    break;
                case ButtonUIState.Hide:
                    ChangeUIToHide();
                    break;
                case ButtonUIState.Grinding:
                    ChangeUIToGrinding();
                    break;
                case ButtonUIState.GrindingHide:
                    ChangeUIToGrindingHide();
                    break;
                
            }

        }

        private void ChangeUIToGrinding()
        {
            _isPendingUnlockButton = false;
            _isLockButton = true;
            
            if(_curAnimSequence != null && !_curAnimSequence.IsComplete)
                _curAnimSequence.Complete();
            
            _curAnimSequence = WolfTween.GetSequence();

            _curAnimSequence.Append(
                _contentScale.DOScale(new Vector3(_scaleDownValue, _scaleDownValue, _scaleDownValue), _scaleDuration)
                    .SetUpdate(true)
                    .SetEase(Ease.OutBack));

            _curAnimSequence.Join(
                _canvasGroupBackground.DOFade(0, _scaleDuration * 0.8f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));

            /*
            _curAnimSequence.Join(
                _canvasGroupGrindTime.DOFade(0, _scaleDuration * 0.8f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));
            */

            _curAnimSequence.SetUpdate(true);
        }

        private void ChangeUIToNormal()
        {
            CommonLog.Log("ChangeUIToNormal");
            
            if(!gameObject.activeSelf)
                gameObject.SetActive(true);
            
            if(_curAnimSequence != null && !_curAnimSequence.IsComplete)
                _curAnimSequence.Complete();
            
            _curAnimSequence = WolfTween.GetSequence();

            _curAnimSequence.Append(
                _contentScale.DOScale(new Vector3(1, 1, 1), _scaleDuration)
                    .SetUpdate(true)
                    .SetEase(Ease.OutBack));

            _curAnimSequence.Join(
                _canvasGroupBackground.DOFade(1, _scaleDuration * 0.8f)
                    .SetDelay(_scaleDuration * 0.2f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));

            /*
            _curAnimSequence.Join(
                _canvasGroupGrindTime.DOFade(1, _scaleDuration * 0.8f)
                    .SetDelay(_scaleDuration * 0.2f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));
            */

            _curAnimSequence.SetUpdate(true);

            if (_isPendingUnlockButton)
            {
                _isPendingUnlockButton = false;
                
                _isLockButton = false;
                
                if (_hasPendingRewardToShowUI)
                    DoAnimReward();
            }
        }

        private void ChangeUIToGrindingHide()
        {
            _isPendingUnlockButton = false;
            _isLockButton = true;
            
            if(_curAnimSequence != null && !_curAnimSequence.IsComplete)
                _curAnimSequence.Complete();
            
            if(!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                _contentScale.localScale = new Vector3(_scaleDownHideValue, _scaleDownHideValue, _scaleDownHideValue);
                _canvasGroupBackground.alpha = 0;
                return;
            }
            
            _curAnimSequence = WolfTween.GetSequence();

            _curAnimSequence.Append(
                _contentScale.DOScale(new Vector3(_scaleDownHideValue, _scaleDownHideValue, _scaleDownHideValue), _scaleDuration)
                    .SetUpdate(true)
                    .SetEase(Ease.OutBack));

            _curAnimSequence.Join(
                _canvasGroupBackground.DOFade(0, _scaleDuration * 0.8f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));

            /*
            _curAnimSequence.Join(
                _canvasGroupGrindTime.DOFade(0, _scaleDuration * 0.8f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad));
            */

            _curAnimSequence.SetUpdate(true);
        }

        private void ChangeUIToHide()
        {
            gameObject.SetActive(false);
        }
        
        private void PlayAnimGrinding()
        {
            _hasPendingRewardToShowUI = true;

            if (!_isLockButton)
            {
                if(_curBtnUIState == ButtonUIState.Normal)
                    ChangeUIState(ButtonUIState.Grinding);
                else if (_curBtnUIState == ButtonUIState.Hide)
                    ChangeUIState(ButtonUIState.GrindingHide);
                else
                    ChangeUIState(ButtonUIState.Grinding);
            }
            
            if(_curAnimSequence != null && !_curAnimSequence.IsComplete)
                _curAnimSequence.Complete();

            Vector3 curContentScaleValue = Vector3.one;
            
            if(_curBtnUIState == ButtonUIState.Grinding)
                curContentScaleValue = new Vector3(_scaleDownValue, _scaleDownValue, _scaleDownValue);
            else if(_curBtnUIState == ButtonUIState.GrindingHide)
                curContentScaleValue = new Vector3(_scaleDownHideValue, _scaleDownHideValue, _scaleDownHideValue); 

            var targetScaleValue = curContentScaleValue * 0.8f;

            var scaleDownDuration = 0.15f;
            var scaleUpDuration = 0.15f;
            var scaleUpDelay = 0.1f;

            _curAnimSequence = WolfTween.GetSequence();
            
            _curAnimSequence.Append(
                _contentScale.DOScale(targetScaleValue, scaleDownDuration)
                    .SetUpdate(true)
                    .SetEase(Ease.Linear));
            
            _curAnimSequence.Append(
                _contentScale.DOScale(curContentScaleValue, scaleUpDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .SetDelay(scaleUpDelay));

            _curAnimSequence.SetUpdate(true);
            
            if(_listAnimImgGrinding != null)
                foreach(var anim in _listAnimImgGrinding)
                    anim.PlayAnim();
        }
    }
}