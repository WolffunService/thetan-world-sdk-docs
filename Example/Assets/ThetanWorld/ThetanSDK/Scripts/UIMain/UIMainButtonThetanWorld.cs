using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ThetanSDK;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.UI;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    internal class UIMainButtonThetanWorld : MonoBehaviour
    {
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

        [SerializeField] private float grindingAnimInterval = 2.5f;
        
        [SerializeField] private float _scaleDownValue = 0.85f;
        [SerializeField] private float _scaleDuration = 0.15f;
        [SerializeField] private List<MoveImageToDestination> _listAnimImgGrinding;
        
        [SerializeField] private float _currencyScaleDelay;
        [SerializeField] private float _currencyScaleDelta;
        [SerializeField] private float _currencyScaleDuration;

        [SerializeField] private float forceOpenThetanWorldConsecutiveClick = 3;
        [SerializeField] private float forceOpenThetanWorldClicMaxInterval = 0.4f;
        
        private Sequence _curCurrencyFlySequence;

        private Action _onClickOpenThetanWorld;

        private float _countTimePlayGrindingAnim;
        private Sequence _curAnimSequence;
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
            SetContentNoInternet(false);
            SetUpUINotLoggedIn();

            _btnOpenThetanWorld.onClick.AddListener(OnClickBtnThetanWorld);

            ThetanSDKManager.Instance.OnOpenMainUI += OnOpenMainUI;
            ThetanSDKManager.Instance.OnCloseMainUI += OnCloseMainUI;
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
        private void Update()
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;
        
            var nftService = ThetanSDKManager.Instance.NftItemService;

            if (nftService.IsGrinding() &&
                nftService.IsAllowPingGrindingServer)
            {
                _countTimePlayGrindingAnim += Time.deltaTime;

                if (_countTimePlayGrindingAnim >= grindingAnimInterval)
                {
                    _countTimePlayGrindingAnim = 0;
                    PlayAnimGrinding();
                }
            }
            else
            {
                _countTimePlayGrindingAnim = 0;
            }
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
            _lastTimeClickButton = Time.time;
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

        public void Initialize(ShowAnimCurrencyFly animCurrencyFly, NftItemService nftItemService, Action onClickOpenThetanWorld)
        {
            _onClickOpenThetanWorld = onClickOpenThetanWorld;
            _showAnimCurrencyFly = animCurrencyFly;
            _isGrinding = false;
            _nftItemService = nftItemService;
            nftItemService._onChangeGrindingStatus += OnChangeGrindingStatus;
            
            ThetanSDKManager.Instance.OnChangeNetworkClientState += OnChangeNetworkClientState;
            nftItemService.RegisterOnChangeNftItemData(OnChangeNFTItemData);
            nftItemService.RegisterOnChangeSelectedNftHeroCallback(OnChangeSelectedNFT);

            OnChangeNetworkClientState(ThetanSDKManager.Instance.NetworkClientState);
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
                ChangeUIToNormal();
            }

            _isLockButton = false;

            if (_hasPendingRewardToShowUI)
                DoAnimReward();
        }

        private void DoAnimReward()
        {
            _hasPendingRewardToShowUI = false;

            var defaullContentSize = _contentScale.localScale;
            _showAnimCurrencyFly.DoAnimCurrencyFly(this.transform as RectTransform);
            if(_curCurrencyFlySequence != null && !_curCurrencyFlySequence.IsComplete())
                _curCurrencyFlySequence.Kill();

            _curCurrencyFlySequence = DOTween.Sequence();

            _curCurrencyFlySequence.Append(DOVirtual.DelayedCall(_currencyScaleDelay, () => { }));
            for (int i = 0; i < 2; i++)
            {
                _curCurrencyFlySequence.Append(_contentScale.DOScale(defaullContentSize * _currencyScaleDelta, _currencyScaleDuration * 0.5f)
                    .SetEase(Ease.OutQuad));
                _curCurrencyFlySequence.Append(_contentScale.DOScale(defaullContentSize, _currencyScaleDuration * 0.5f)
                    .SetEase(Ease.OutQuad));
            }
            
            _curCurrencyFlySequence.Play();
        }

        private void OnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.NotLoggedIn)
            {
                _hasPendingRewardToShowUI = false;
                _isGrinding = false;
                _isLockButton = false;
                gameObject.SetActive(true);
                SetUpUINotLoggedIn();
                SetContentNoInternet(false);
                return;
            }
            else if (newState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                _hasPendingRewardToShowUI = false;
                _isGrinding = false;
                _isLockButton = false;
                gameObject.SetActive(true);
                SetUpUINotLoggedIn();
                SetContentNoInternet(true);
                return;
            }


            if (newState == ThetanNetworkClientState.LoggedIn)
            {
                gameObject.SetActive(true);
                SetUpUISDKLoggedIn();
                SetContentNoInternet(false);
            }
            else if (newState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                gameObject.SetActive(true);
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

        private void SetUISelectedNFT(HeroNftItem data)
        {
            _selectedHeroNFTInfo = data;
            _objIconSelectedHeroNFT.SetActive(true);
            _imgIconThetanWorld.enabled = false;
            _avatarNFT.ShowUI(data.ingameInfo);

            bool isMaxDailyGrindTime = data.grindInfo.grindTime >= data.grindInfo.maxGrindTime;

            if (_imgGrindTimeInDay != null)
            {
                var percentGrindTime = data.grindInfo.grindTime / data.grindInfo.maxGrindTime;
                percentGrindTime = Mathf.Clamp01(percentGrindTime);
                _imgGrindTimeInDay.fillAmount = percentGrindTime;
                _imgGrindTimeInDay.color = isMaxDailyGrindTime ? _colorGrindTimeLimit : _colorGrindTimeNormal;
            }

            if (_contentMaxDailyGrindTime != null)
            {
                _contentMaxDailyGrindTime.SetActive(isMaxDailyGrindTime || data.grindInfo.IsMaxLifeTime());
            }

            SetContentGreyScale();
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
                ChangeUIToGrinding();
            }
            else
            {
                ChangeUIToNormal();
            }
        }

        private void ChangeUIToGrinding()
        {
            _isPendingUnlockButton = false;
            _isLockButton = true;
            
            if(_curAnimSequence != null && !_curAnimSequence.IsComplete())
                _curAnimSequence.Complete();
            
            _curAnimSequence = DOTween.Sequence();

            _curAnimSequence.Append(
                _contentScale.DOScale(new Vector3(_scaleDownValue, _scaleDownValue, _scaleDownValue), _scaleDuration)
                    .SetEase(Ease.OutBack));

            _curAnimSequence.Join(
                _canvasGroupBackground.DOFade(0, _scaleDuration * 0.8f)
                    .SetEase(Ease.OutQuad));

            _curAnimSequence.Join(
                _canvasGroupGrindTime.DOFade(0, _scaleDuration * 0.8f)
                    .SetEase(Ease.OutQuad));

            _curAnimSequence.Play();
        }

        private void ChangeUIToNormal()
        {
            CommonLog.Log("ChangeUIToNormal");
            
            if(_curAnimSequence != null && !_curAnimSequence.IsComplete())
                _curAnimSequence.Complete();
            
            _curAnimSequence = DOTween.Sequence();

            _curAnimSequence.Append(
                _contentScale.DOScale(new Vector3(1, 1, 1), _scaleDuration)
                    .SetEase(Ease.OutBack));

            _curAnimSequence.Join(
                _canvasGroupBackground.DOFade(1, _scaleDuration * 0.8f)
                    .SetDelay(_scaleDuration * 0.2f)
                    .SetEase(Ease.OutQuad));

            _curAnimSequence.Join(
                _canvasGroupGrindTime.DOFade(1, _scaleDuration * 0.8f)
                    .SetDelay(_scaleDuration * 0.2f)
                    .SetEase(Ease.OutQuad));

            _curAnimSequence.Play();

            if (_isPendingUnlockButton)
            {
                _isPendingUnlockButton = false;
                
                _isLockButton = false;
                
                if (_hasPendingRewardToShowUI)
                    DoAnimReward();
            }
        }
        
        private void PlayAnimGrinding()
        {
            _hasPendingRewardToShowUI = true;

            if (!_isLockButton)
            {
                ChangeUIToGrinding();
            }
            
            if(_curAnimSequence != null && !_curAnimSequence.IsComplete())
                _curAnimSequence.Complete();

            var curContentScaleValue = _contentScale.localScale;

            var targetScaleValue = curContentScaleValue * 0.8f;

            var scaleDownDuration = 0.15f;
            var scaleUpDuration = 0.15f;
            var scaleUpDelay = 0.1f;
            
            _curAnimSequence.Append(
                _contentScale.DOScale(targetScaleValue, scaleDownDuration)
                    .SetEase(Ease.Linear));
            
            _curAnimSequence.Append(
                _contentScale.DOScale(curContentScaleValue, scaleUpDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(scaleUpDelay));

            _curAnimSequence.Play();
            
            if(_listAnimImgGrinding != null)
                foreach(var anim in _listAnimImgGrinding)
                    anim.PlayAnim();
        }
    }
}