using System;
using System.Collections.Generic;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class DetailHeroNFTUI : MonoBehaviour
    {
        private const float UPDATE_UI_FREE_NFT_SECOND_INTERVAL = 5;
        [SerializeField] private Button _btnInteractHero;
        [SerializeField] private TextMeshProUGUI _txtBtnInteractHero;
        [SerializeField] private TextMeshProUGUI _txtHeroName;
        [SerializeField] private TextMeshProUGUI _txtWorld;
        [SerializeField] private GameObject _contentSpecialNFT;
        [SerializeField] private TextMeshProUGUI _txtRarity;
        [SerializeField] private NFTHeroImg _nftHeroImg;
        [SerializeField] private NFTRarityColorConfig _rarityColorConfig;

        [Header("Color Button")]
        [SerializeField] private Sprite _textureBtnSelect;
        [SerializeField] private Sprite _textureBtnNormal;
        [SerializeField] private Color _colorButtonSelectHero;
        [SerializeField] private Color _colorButtonDeselectHero;
        [SerializeField] private Color _colorButtonGrinding;

        [Header("Color Text Button")]
        [SerializeField] private Color _colorTextSelectHero;
        [SerializeField] private Color _colorTextDeselectHero;
        [SerializeField] private Color _colorTextGrinding;



        [Header("Detail grind info")]
        [SerializeField] private TextMeshProUGUI _grindSpeedTxt;
        [SerializeField] private Button _grindSpeedTooltip;

        [SerializeField] private TextMeshProUGUI _victoryRewardSpeedTxt;
        [SerializeField] private Button _victoryRewardSpeedTooltip;

        [SerializeField] private TextMeshProUGUI _totalGrindTimeTxt;
        [SerializeField] private CommonSlider _totalGrindTimeSlider;
        [SerializeField] private GameObject _totalGrindTimeFree;
        [SerializeField] private Button _totalGrindTimeTooltip;

        [SerializeField] private TextMeshProUGUI _todayGrindTimeTitleTxt;
        [SerializeField] private RectTransform _tranformGrindTimeResetContainer;
        [SerializeField] private TextMeshProUGUI _todayGrindTimeRefreshTimeTxt;
        [SerializeField] private CommonSlider _todayGrindTimeSlider;
        [SerializeField] private Button _todayGrindTimeTooltip;
        [SerializeField] private Button _todayGrindTimeResetBtn;



        [Header("Equipment Info")]
        [SerializeField] private GameObject _objEquipment;
        [SerializeField] private TextMeshProUGUI _txtEquipment;
        [SerializeField] private TextMeshProUGUI _txtMaxEquipment;
        [SerializeField] private TextMeshProUGUI _txtGrindAbilityBonus;
        [SerializeField] private List<EquipmentItemUI> _listEquipmentUIs;

        [Header("Size Config")]
        [SerializeField] private float _baseContentSize;
        [SerializeField] private float _maxGrindTimeContentBonusSize;
        [SerializeField] private float _maxLifeTimeContentBonusSize;
        [SerializeField] private float _bonusContentPaddingSize;
        [SerializeField] private float _grindTimeResetContainerSizeOffset;

        public float MaxGrindTimeContentBonusSize => _maxGrindTimeContentBonusSize;
        public float MaxLifeTimeContentBonusSize => _maxLifeTimeContentBonusSize;
        public float BonusContentPaddingSize => _bonusContentPaddingSize;

        private HeroNftItem _heroData;
        private Action<HeroNftItem> _onSelectCallback;
        private UIHelperContainer _uiHelperContainer;

        private float _countTimeUpdateNormalNFT;
        private float _countTimeUpdateFreeNFT;

        private DateTime _nextResetData;

        private void Awake()
        {
            _totalGrindTimeSlider.SetValueConvertToStringFunction(value => value.ConvertSecondToHour());
            _todayGrindTimeSlider.SetValueConvertToStringFunction(value => value.ConvertSecondToMinute());
        }

        public void ClearCache()
        {
            _heroData = new HeroNftItem();
            _onSelectCallback = null;
            _uiHelperContainer = null;
        }

        private void OnEnable() => ThetanSDKManager.Instance.NftItemService.RegisterOnChangeNftItemData(HandleOnChangeNftHeroData);

        private void OnDisable()
        {
            if (ThetanSDKManager.IsAlive && ThetanSDKManager.Instance.NftItemService != null)
                ThetanSDKManager.Instance.NftItemService.UnRegisterOnChangeNftItemData(HandleOnChangeNftHeroData);
        }

        private void HandleOnChangeNftHeroData(HeroNftItem hero)
        {
            if (_heroData.id != hero.id)
                return;

            SetCommonUI(hero, ThetanSDKManager.Instance.NftItemService.SelectedHeroNftId);
        }

        private void Update()
        {
            if (_heroData.nftType == NFTType.NormalNFT)
            {
                UpdateNormalNFT();
                
            }
            else if (_heroData.nftType == NFTType.FreeNFT)
            {
                UpdateFreeNFT();
            }
        }

        private void UpdateNormalNFT()
        {
            if (!(_heroData.grindInfo.grindTime >= _heroData.grindInfo.maxGrindTime) ||
                _heroData.grindInfo.maxGrindTime == 0 ||
                _heroData.grindInfo.IsMaxLifeTime())
                return;

            _countTimeUpdateNormalNFT -= Time.unscaledDeltaTime;

            if (_countTimeUpdateNormalNFT > 0)
            {
                return;
            }

            _countTimeUpdateNormalNFT = 1;
            if (_heroData.grindInfo.grindTime >= _heroData.grindInfo.maxGrindTime &&
                !_heroData.grindInfo.IsMaxLifeTime())
            {
                _todayGrindTimeRefreshTimeTxt.text = $"Resets in: <color=#009C60>{ThetanSDKUtilities.ToStringTimeShort(_nextResetData - DateTime.UtcNow)}</color>";
                SetResetGrindTimeContainer();
            }

            if ((_nextResetData - DateTime.UtcNow).TotalSeconds <= 0)
            {
                ThetanSDKManager.Instance.NftItemService.RefreshDataHeroNft(_heroData, null, null);

                // For the next 10 second, do not invoke refresh again to avoid spam
                _countTimeUpdateNormalNFT = 10;
            }
        }

        private void UpdateFreeNFT()
        {
            var _prevCountTime = _countTimeUpdateFreeNFT;
            _countTimeUpdateFreeNFT += Time.unscaledDeltaTime;

            if (_countTimeUpdateFreeNFT >= UPDATE_UI_FREE_NFT_SECOND_INTERVAL)
            {
                _countTimeUpdateFreeNFT = 0;

                SetUITodayGrindTime(_heroData);
            }

            if ((int)_prevCountTime != (int)_countTimeUpdateFreeNFT)
            {
                var currentTime = DateTime.UtcNow;
                var freeNFTInfo = ThetanSDKManager.Instance.NftItemService.FreeNftInfo;
                var freeGrindTime = ThetanSDKUtilities.ConvertFreeNFTInfoToGrindTimeInfo(freeNFTInfo, _heroData);

                if (freeGrindTime.grindTime >= freeGrindTime.maxTime)
                {
                    _todayGrindTimeRefreshTimeTxt.text = $"Resets in: <color=#009C60>{ThetanSDKUtilities.ToStringTimeShort(freeNFTInfo.nextResetGrindEarn - DateTime.UtcNow)}</color>";
                    if(!_todayGrindTimeResetBtn.gameObject.activeSelf)
                        _todayGrindTimeResetBtn.gameObject.SetActive(true);
                }
                else if(currentTime >= freeNFTInfo.startSectionEarn && currentTime < freeNFTInfo.startSectionRest)
                {
                    _todayGrindTimeRefreshTimeTxt.text =
                        $"Ends in: <color=#D4700D>{ThetanSDKUtilities.ToStringTimeShort(freeNFTInfo.startSectionRest - currentTime)}</color>";
                }
                else
                {
                    _todayGrindTimeRefreshTimeTxt.text = string.Empty;
                }
                SetResetGrindTimeContainer();
            }
            
        }

        public void SetData(HeroNftItem data, UIHelperContainer uiHelperContainer, Action<HeroNftItem> onSelectCallback)
        {
            _onSelectCallback = onSelectCallback;
            _uiHelperContainer = uiHelperContainer;

            var nftItemService = ThetanSDKManager.Instance.NftItemService;

            _nextResetData = data.grindInfo.nextReset;

            SetCommonUI(data, nftItemService.SelectedHeroNftId);
        }

        private void SetCommonUI(HeroNftItem data, string selectedHeroNftId)
        {
            _heroData = data;

            _todayGrindTimeRefreshTimeTxt.text = string.Empty;
            SetResetGrindTimeContainer();
            
            if (data.nftType != NFTType.FreeNFT)
                SetDataEquipment(data);
            else
                _objEquipment.SetActive(false);

            if (_nftHeroImg) _nftHeroImg.ShowUI(data.ingameInfo);

            if (_txtHeroName) _txtHeroName.text = data.metaData.name;

            if (_txtWorld) _txtWorld.text = ThetanSDKUtilities.GetWorldName(data.ingameInfo.gameId);

            if (_txtRarity)
            {
                if(data.nftType == NFTType.NormalNFT)
                {
                    _txtRarity.text = ThetanSDKUtilities.GetHeroRarityName(data.ingameInfo.rarity);
                    _txtRarity.color = _rarityColorConfig.GetColor(data.ingameInfo.rarity);
                }
                else
                {
                    _txtRarity.text = string.Empty;
                }
            }

            if (_contentSpecialNFT)
            {
                _contentSpecialNFT.SetActive(data.nftType == NFTType.FreeNFT);
            }

            var grindInfo = data.grindInfo;


            //==========================================================================================
            _grindSpeedTxt.text = $"{(grindInfo.currentGrindSpeed * 3600).FormatUnitCurrency()}/h";    // Convert THG/s -> THG/h

            //==========================================================================================
            _victoryRewardSpeedTxt.text = string.Format("{0} - {1}/h",
                ((float)grindInfo.estVESpeedLoose).ConvertValuePerSecondToHour().FormatUnitCurrency(),
                ((float)grindInfo.estVESpeedWin).ConvertValuePerSecondToHour().FormatUnitCurrency());


            //==========================================================================================
            if (data.nftType == NFTType.FreeNFT)
            {
                _totalGrindTimeTxt.text = "Unlimited";
                _totalGrindTimeTxt.color = Color.white;
                _totalGrindTimeFree.SetActive(true);
                _countTimeUpdateFreeNFT = UPDATE_UI_FREE_NFT_SECOND_INTERVAL;
            }
            else
            {
                _totalGrindTimeFree.SetActive(false);
                _totalGrindTimeTxt.color = Color.black;
                _totalGrindTimeSlider.SetData(0,
                    grindInfo.maxAllGrindTime,
                    grindInfo.allGrindTime);
                _countTimeUpdateNormalNFT = 0;
            }


            //==========================================================================================
            SetUITodayGrindTime(data);
            _todayGrindTimeTitleTxt.text = data.nftType == NFTType.FreeNFT ? "Current Earning Section:" : "Today's Earning Time:";


            //==========================================================================================
            if (_txtGrindAbilityBonus)
                _txtGrindAbilityBonus.text = $"Total Effect <color=#009C60>+{(data.grindInfo.grindRewardEffect * 100).FormatUnitPercent()}%</color>";

            SetContentButtonInteractHero(data, selectedHeroNftId);
            SetToolTip(data);
        }

        private void SetToolTip(HeroNftItem data)
        {
            _grindSpeedTooltip.onClick.RemoveAllListeners();
            _grindSpeedTooltip.onClick.AddListener(() => _uiHelperContainer.ShowTextTooltip(
                "Estimated Grind Reward per hour",
                _grindSpeedTooltip.transform as RectTransform, 
                TooltipAlignment.BottomRight));

            _victoryRewardSpeedTooltip.onClick.RemoveAllListeners();
            _victoryRewardSpeedTooltip.onClick.AddListener(() => _uiHelperContainer.ShowTextTooltip("Estimated Victory Reward per hour", 
                    _victoryRewardSpeedTooltip.transform as RectTransform, 
                    TooltipAlignment.BottomRight));

            _totalGrindTimeTooltip.onClick.RemoveAllListeners();
            _totalGrindTimeTooltip.onClick.AddListener(() => _uiHelperContainer.ShowTextTooltip(
                "Total number of hours an NFT can be used to earn", 
                _totalGrindTimeTooltip.transform as RectTransform, 
                TooltipAlignment.BottomLeft));

            _todayGrindTimeTooltip.onClick.RemoveAllListeners();
            _todayGrindTimeTooltip.onClick.AddListener(() => _uiHelperContainer.ShowTextTooltip(
                data.nftType == NFTType.FreeNFT ? "Active time for this NFT to generate earnings. No further earnings until the time is reset" : "Today's Earning Time",
                _todayGrindTimeTooltip.transform as RectTransform, TooltipAlignment.BottomRight));

            _todayGrindTimeResetBtn.onClick.RemoveAllListeners();
            _todayGrindTimeResetBtn.onClick.AddListener(() => _uiHelperContainer.ShowTextTooltip("Refresh unavailable within the game", _todayGrindTimeResetBtn.transform as RectTransform, TooltipAlignment.BottomRight));
        }

        private void SetUITodayGrindTime(HeroNftItem data)
        {
            var grindInfo = data.grindInfo;

            var grindTime = grindInfo.grindTime;
            var maxGrindTime = grindInfo.maxGrindTime;
            if (data.nftType == NFTType.FreeNFT)
            {
                var freeGrindTime = ThetanSDKUtilities.ConvertFreeNFTInfoToGrindTimeInfo(ThetanSDKManager.Instance.NftItemService.FreeNftInfo, data);
                maxGrindTime = freeGrindTime.maxTime;
                _todayGrindTimeResetBtn.gameObject.SetActive(freeGrindTime.grindTime >= freeGrindTime.maxTime);
                _todayGrindTimeSlider.SetData(0,
                    maxGrindTime,
                    grindTime);

                if (freeGrindTime.grindTime >= freeGrindTime.maxTime)
                    _todayGrindTimeSlider.UseColorFull();
            }
            else
            {
                _todayGrindTimeResetBtn.gameObject.SetActive(false);   
                _todayGrindTimeSlider.SetData(0,
                    maxGrindTime,
                    grindTime);
            }
            
        }

        private void SetContentButtonInteractHero(HeroNftItem data, string selectedHeroNftId)
        {
            Color colorButton = Color.white;
            Sprite sprBtn = _textureBtnNormal;
            _btnInteractHero.onClick.RemoveAllListeners();
            _btnInteractHero.interactable = true;
            _btnInteractHero.image.raycastTarget = true;

            if (data.IsInTransaction())
            {
                colorButton = _colorButtonGrinding;
                _btnInteractHero.interactable = false;
                _btnInteractHero.image.raycastTarget = false;

                if (_txtBtnInteractHero)
                {
                    _txtBtnInteractHero.color = _colorTextGrinding;
                    _txtBtnInteractHero.text = "Locked";
                }
            }
            else if (data.id != selectedHeroNftId &&
                !data.grindInfo.IsGrinding())
            {
                colorButton = _colorButtonSelectHero;
                sprBtn = _textureBtnSelect;
                _btnInteractHero.onClick.AddListener(OnClickSelectHero);

                if (_txtBtnInteractHero)
                {
                    _txtBtnInteractHero.color = _colorTextSelectHero;
                    _txtBtnInteractHero.text = "Select And Play";
                }
            }
            else if (data.id == selectedHeroNftId &&
                     !data.grindInfo.IsGrinding())
            {
                colorButton = _colorButtonDeselectHero;
                _btnInteractHero.onClick.AddListener(OnClickDeselectHero);

                if (_txtBtnInteractHero)
                {
                    _txtBtnInteractHero.color = _colorTextDeselectHero;
                    _txtBtnInteractHero.text = "Unselect";
                }
            }
            else if (data.grindInfo.IsGrinding())
            {
                colorButton = _colorButtonGrinding;
                _btnInteractHero.interactable = true;
                _btnInteractHero.image.raycastTarget = true;
                _btnInteractHero.onClick.AddListener(OnClickInteractWithGrindingHero);

                if (_txtBtnInteractHero)
                {
                    _txtBtnInteractHero.color = _colorTextGrinding;
                    _txtBtnInteractHero.text = "Grinding";
                }
            }

            var color = _btnInteractHero.colors;
            color.normalColor = colorButton;
            color.highlightedColor = colorButton;
            color.pressedColor = colorButton;
            color.selectedColor = colorButton;
            color.disabledColor = colorButton;
            _btnInteractHero.colors = color;
            _btnInteractHero.image.sprite = sprBtn;
        }

        
        private void SetResetGrindTimeContainer()
        {
            if (string.IsNullOrEmpty(_todayGrindTimeRefreshTimeTxt.text))
            {
                _tranformGrindTimeResetContainer.gameObject.SetActive(false);
            }
            else
            {
                _tranformGrindTimeResetContainer.gameObject.SetActive(true);
                _tranformGrindTimeResetContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                    _todayGrindTimeRefreshTimeTxt.preferredWidth + _grindTimeResetContainerSizeOffset);
            }
        }

        private void SetDataEquipment(HeroNftItem data)
        {
            _objEquipment.SetActive(true);

            int totalEquipmentSlot = 0;
            int totalSlotEquipped = 0;

            var equimentService = ThetanSDKManager.Instance.EquipmentService;

            if (data.equipmentSet != null)
            {
                int equipmentSlotIndex = 0;
                foreach (var kp in data.equipmentSet)
                {
                    var equipmentSlotData = kp.Value;

                    totalEquipmentSlot++;
                    if (!string.IsNullOrEmpty(equipmentSlotData.equippedId))
                        totalSlotEquipped++;

                    if (equipmentSlotIndex >= _listEquipmentUIs.Count)
                        break;

                    if (equimentService.TryGetEquipmentData(equipmentSlotData.equippedId, out var equipmentItemData))
                    {
                        var equipmentSlot = _listEquipmentUIs[equipmentSlotIndex];

                        var isCanUpgrade = equimentService.CheckEquipmentCanUpgrade(equipmentSlotData.equippedId);

                        equipmentSlot.SetUI(new EquipmentItemUIData()
                        {
                            rarity = data.ingameInfo.rarity,
                            equipmentId = equipmentSlotData.requiredTypeId,
                            isEquipped = true,
                            itemStar = equipmentItemData.props.stars,
                            isCanUpgrade = isCanUpgrade && !data.grindInfo.IsMaxLifeTime()
                        }, _uiHelperContainer);
                    }
                    else
                    {
                        var isCanEquip = equimentService.CheckHaveAnyEquipmentForSlot(equipmentSlotData.requiredTypeId);

                        var equipmentSlot = _listEquipmentUIs[equipmentSlotIndex];

                        equipmentSlot.SetUI(new EquipmentItemUIData()
                        {
                            rarity = data.ingameInfo.rarity,
                            equipmentId = equipmentSlotData.requiredTypeId,
                            isEquipped = false,
                            isCanEquip = isCanEquip && !data.grindInfo.IsMaxLifeTime()
                        }, _uiHelperContainer);
                    }

                    equipmentSlotIndex++;
                }
            }

            if (_txtEquipment)
                _txtEquipment.SetText("{0}", totalSlotEquipped);

            if (_txtMaxEquipment)
                _txtMaxEquipment.SetText("/{0}", totalEquipmentSlot);
        }

        private void OnClickInteractWithGrindingHero()
        {
            var appId = ThetanSDKManager.Instance.NetworkConfig.ApplicationID;

            if (_heroData.grindInfo.status == null)
                return;

            if (_heroData.grindInfo.status.appId != appId)
            {
                // Todo: use app name
                _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error,
                    string.Format(UINftErrorMsg.ERROR_SELECT_HERO_NFT_HERO_IS_GRINDING_IN_ANOTHER_GAME_NAME,
                        _heroData.grindInfo.status.appName),
                    ThetanSDKErrorMsg.Okay);
            }
            else
            {
                _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Warning,
                    UINftErrorMsg.WARNING_CONFIRM_FORCE_STOP_GRINDING,
                    ThetanSDKErrorMsg.Decline, ThetanSDKErrorMsg.Confirm,
                    null, () =>
                    {
                        ThetanSDKManager.Instance.StopGrindingHeroItem(new EndMatchInfo()
                        {
                            matchResult = MatchResult.Lose,
                            gameLevel = 0
                        });
                        ThetanSDKManager.Instance.UnlockButtonMain();
                    });
            }
        }

        private void OnClickDeselectHero()
        {
            _uiHelperContainer.TurnOnLoading();

            ThetanSDKManager.Instance.NftItemService.DeselectHeroNft(_heroData, heroNftBack =>
            {
                _uiHelperContainer.TurnOffLoading();
                _onSelectCallback?.Invoke(new HeroNftItem().SetDefault());
            }, error =>
            {
                _uiHelperContainer.TurnOffLoading();
                HandleDeselectHeroError(error);
            });
        }

        private void HandleDeselectHeroError(WolffunResponseError error)
        {
            if (error.Code == (int)WSErrorCode.UserBanned)
            {
                _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.AccountBanned,
                    AuthenErrorMsg.AccountBannedContactSupport, AuthenErrorMsg.Confirm);
                return;
            }

            if (error.Code == (int)WSErrorCode.ServerMaintenance)
            {
                ThetanSDKManager.Instance.HandleMaintenance();
                return;
            }

            switch ((NftItemServiceErrorCode)error.Code)
            {
                case NftItemServiceErrorCode.NETWORK_ERROR:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.NetworkError,
                        UINftErrorMsg.ERROR_NO_CONNECTION, ThetanSDKErrorMsg.Okay);
                    break;
                case NftItemServiceErrorCode.ANOTHER_NFT_IS_GRINDING:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error,
                        UINftErrorMsg.ERROR_DESELECT_HERO_ANOTHER_IS_GRINDING, ThetanSDKErrorMsg.Okay);
                    break;
                case NftItemServiceErrorCode.SELECTED_HERO_ID_NOT_THE_SAME:
                    {
                        _uiHelperContainer.TurnOnLoading();
                        ThetanSDKManager.Instance.NftItemService.GetSelectedHeroNft(() =>
                        {
                            _uiHelperContainer.TurnOffLoading();
                            var nftItemService = ThetanSDKManager.Instance.NftItemService;
                            SetCommonUI(_heroData, nftItemService.SelectedHeroNftId);
                        }, responseError =>
                        {
                            _uiHelperContainer.TurnOffLoading();
                            _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error,
                                string.Format(UINftErrorMsg.ERROR_DESELECT_HERO_UNKNOWN_ERROR, error.Code, error.Message),
                                ThetanSDKErrorMsg.Okay);
                        });
                        break;
                    }
                default:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error,
                        string.Format(UINftErrorMsg.ERROR_DESELECT_HERO_UNKNOWN_ERROR, error.Code, error.Message),
                        ThetanSDKErrorMsg.Okay);
                    break;
            }

            ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("NFT Detail", "Unselect NFT", true,
                (NftItemServiceErrorCode)error.Code == NftItemServiceErrorCode.UNKNOWN ? error.DevDebugMessage : error.Message);
        }

        private async void OnClickSelectHero()
        {
            var nftService = ThetanSDKManager.Instance.NftItemService;
            if (_heroData.grindInfo.IsMaxLifeTime())
            {
                _uiHelperContainer.ShowPopUpMsg(
                    ThetanSDKErrorMsg.Warning,
                    UINftErrorMsg.WARNING_SELECT_HERO_MAX_LIFE_TIME,
                    "Select other NFT", "Continue",
                    null, CallSelectHeroNFT);
                return;
            }
            else if (nftService.IsGrinding() && nftService.GrindingHeroNftId != _heroData.id)
            {
                _uiHelperContainer.ShowPopUpMsg(
                    ThetanSDKErrorMsg.Warning,
                    UINftErrorMsg.WARNING_OTHER_NFT_IS_GRINDING_FORCE_STOP,
                    "Wait NFT grinding", "Force stop grinding",
                    null, ForceStopGrindingAndCallSelectHeroNFT);
                return;
            }
            else if (_heroData.nftType == NFTType.NormalNFT)
            {
                if (_heroData.grindInfo.grindTime >= _heroData.grindInfo.maxGrindTime)
                {
                    _uiHelperContainer.ShowPopUpMsg(
                        ThetanSDKErrorMsg.Warning,
                        UINftErrorMsg.WARNING_SELECT_HERO_MAX_GRIND_TIME,
                        "Select other NFT", "Continue",
                        null, CallSelectHeroNFT);
                    return;
                }
            }
            else if (_heroData.nftType == NFTType.FreeNFT)
            {
                var freeNftGrindTimeInfo = ThetanSDKUtilities.ConvertFreeNFTInfoToGrindTimeInfo(
                    ThetanSDKManager.Instance.NftItemService.FreeNftInfo,
                    _heroData);

                if (freeNftGrindTimeInfo.grindTime >= freeNftGrindTimeInfo.maxTime)
                {
                    _uiHelperContainer.ShowPopUpMsg(
                        ThetanSDKErrorMsg.Warning,
                        UINftErrorMsg.WARNING_SELECT_HERO_MAX_GRIND_TIME,
                        "Select other NFT", "Continue",
                        null, CallSelectHeroNFT);
                    return;
                }
            }

            CallSelectHeroNFT();
        }

        private void ForceStopGrindingAndCallSelectHeroNFT()
        {
            _uiHelperContainer.TurnOnLoading();
            _btnInteractHero.interactable = false;

            var nftService = ThetanSDKManager.Instance.NftItemService;

            nftService.EndMatch(new EndMatchInfo()
            {
                matchResult = MatchResult.Lose,
                gameLevel = 0,
            }, () =>
            {
                _uiHelperContainer.TurnOffLoading();
                _btnInteractHero.interactable = true;

                CallSelectHeroNFT();
            }, error =>
            {
                _uiHelperContainer.TurnOffLoading();
                _btnInteractHero.interactable = true;

                CallSelectHeroNFT();
            });
        }

        private void CallSelectHeroNFT()
        {
            _uiHelperContainer.TurnOnLoading();
            _btnInteractHero.interactable = false;

            ThetanSDKManager.Instance.NftItemService.SelectHeroNft(_heroData, heroNftBack =>
            {
                _uiHelperContainer.TurnOffLoading();
                _btnInteractHero.interactable = false;
                _onSelectCallback?.Invoke(_heroData);
            }, error =>
            {
                _uiHelperContainer.TurnOffLoading();
                _btnInteractHero.interactable = true;

                HandleSelectNftHeroFail(error);
            });
        }

        private void HandleSelectNftHeroFail(WolffunResponseError error)
        {
            ThetanSDKUtilities.HandleSelectNFTError(error, _uiHelperContainer);
        }

        [ContextMenu("AutoGetSize")]
        private void AutoGetSize()
        {
            _grindTimeResetContainerSizeOffset = _tranformGrindTimeResetContainer.sizeDelta.x -
                                                 _todayGrindTimeRefreshTimeTxt.preferredWidth;
        }
    }
}