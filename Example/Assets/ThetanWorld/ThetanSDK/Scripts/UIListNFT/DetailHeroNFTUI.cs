using System;
using System.Collections.Generic;
using Cysharp.Text;
using ThetanSDK.SDKServices.Equipment;
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
        [SerializeField] private Button _btnInteractHero;
        [SerializeField] private TextMeshProUGUI _txtBtnInteractHero;
        [SerializeField] private TextMeshProUGUI _txtHeroName;
        [SerializeField] private TextMeshProUGUI _txtWorld;
        [SerializeField] private TextMeshProUGUI _txtRarity;
        [SerializeField] private NFTHeroImg _nftHeroImg;
        [SerializeField] private NFTRarityColorConfig _rarityColorConfig;

        [Header("Color Button")]
        [SerializeField] private Color _colorButtonSelectHero;
        [SerializeField] private Color _colorButtonDeselectHero;
        [SerializeField] private Color _colorButtonGrinding;
        
        [Header("Color Text Button")]
        [SerializeField] private Color _colorTextSelectHero;
        [SerializeField] private Color _colorTextDeselectHero;
        [SerializeField] private Color _colorTextGrinding;

        [Header("Color Text Grind Time")]
        [SerializeField] private Color _colorGrindTimeNormal;
        [SerializeField] private Color _colorGrindTimeMax;
        [SerializeField] private Color _colorGrindTimeEndLifeTime;

        [Header("Detail grind info")]
        [SerializeField] private TextMeshProUGUI _txtGrindTime;
        [SerializeField] private GameObject _contentGrindTimeLimit;
        [SerializeField] private TextMeshProUGUI _txtDailyReward;
        [SerializeField] private TextMeshProUGUI _txtGrindStage;
        [SerializeField] private TextMeshProUGUI _txtMaxGrindStage;
        [SerializeField] private Button _btnInfoGrindStage;
        [SerializeField] private TextMeshProUGUI _txtGrindSpeed;
        [SerializeField] private TextMeshProUGUI _txtGrindAbility;
        [SerializeField] private Button _btnInfoGrindCapacity;
        
        [Header("Equipment Info")]
        [SerializeField] private TextMeshProUGUI _txtEquipment;
        [SerializeField] private TextMeshProUGUI _txtMaxEquipment;
        [SerializeField] private TextMeshProUGUI _txtGrindAbilityBonus;
        [SerializeField] private List<EquipmentItemUI> _listEquipmentUIs;

        [Header("Content Max")]
        [SerializeField] private RectTransform _contentSetSizeBonus;
        [SerializeField] private GameObject _contentMaxLifeTime;
        [SerializeField] private GameObject _contentMaxGrindTimeReset;
        [SerializeField] private TextMeshProUGUI _txtResetIn;
        
        [Header("Size Config")]
        [SerializeField] private float _baseContentSize;
        [SerializeField] private float _maxGrindTimeContentBonusSize;
        [SerializeField] private float _maxLifeTimeContentBonusSize;
        [SerializeField] private float _bonusContentPaddingSize;

        public float MaxGrindTimeContentBonusSize => _maxGrindTimeContentBonusSize;
        public float MaxLifeTimeContentBonusSize => _maxLifeTimeContentBonusSize;
        public float BonusContentPaddingSize => _bonusContentPaddingSize;

        private HeroNftItem _heroData;
        private DetailHeroGrindInfo _detailGrindInfo;
        private string _selectedNftHeroId;
        private string _grindingNftHeroId;
        private Action<HeroNftItem> _onSelectCallback;
        private UIHelperContainer _uiHelperContainer;

        private float _countTime;

        private DateTime _nextResetData;
        
        public void ClearCache()
        {
            _heroData = new HeroNftItem();
            _onSelectCallback = null;
            _uiHelperContainer = null;
        }

        private void Awake()
        {
            _btnInfoGrindStage.onClick.AddListener(ShowInfoGrindStage);
            _btnInfoGrindCapacity.onClick.AddListener(ShowInfoGrindCapacity);
        }

        private void OnEnable()
        {
            ThetanSDKManager.Instance.NftItemService.RegisterOnChangeNftItemData(HandleOnChangeNftHeroData);
        }

        private void OnDisable()
        {
            if(ThetanSDKManager.IsAlive &&
               ThetanSDKManager.Instance.NftItemService != null)
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
            if (!(_heroData.grindInfo.grindTime >= _heroData.grindInfo.maxGrindTime) || 
                _heroData.grindInfo.maxGrindTime == 0 ||
                _heroData.grindInfo.IsMaxLifeTime())
                return;
            
            _countTime -= Time.deltaTime;

            if (_countTime > 0)
            {
                return;
            }
            
            _countTime = 1;
            SetTextResetIn();

            if((_nextResetData - DateTime.UtcNow).TotalSeconds <= 0)
            {
                ThetanSDKManager.Instance.NftItemService.RefreshDataHeroNft(_heroData, null, null);

                // For the next 10 second, do not invoke refresh again to avoid spam
                _countTime = 10;
            }
        }

        private void ShowInfoGrindCapacity()
        {
            _uiHelperContainer.ShowTextTooltip("The amount of Rewards to be staked into an NFT for mounting", _btnInfoGrindCapacity.transform as RectTransform, TooltipAlignment.BottomLeft);
        }

        private void ShowInfoGrindStage()
        {
            _uiHelperContainer.ShowTextTooltip(
                "Each Grind Stage spans 5 hours of Grinding. Higher stages yield lower rewards. No further Grind Rewards after reaching max Stage.",
                _btnInfoGrindStage.transform as RectTransform, TooltipAlignment.BottomLeft);
        }

        public void SetData(HeroNftItem data, UIHelperContainer uiHelperContainer, 
            Action<HeroNftItem> onSelectCallback)
        {
            _onSelectCallback = onSelectCallback;
            _uiHelperContainer = uiHelperContainer;

            var nftItemService = ThetanSDKManager.Instance.NftItemService;
            SetCommonUI(data, nftItemService.SelectedHeroNftId);

            SetUILoadingDetailGrindInfo();
            
            ThetanSDKManager.Instance.NftItemService.GetDetailGrindInfoHeroNft(data, (detailGrindInfo) =>
                {
                    _detailGrindInfo = detailGrindInfo;
                    SetUIDetailGrindInfo(data, _detailGrindInfo);
                }, 
                error =>
            {
                SetUIErrorDetailGrindInfo();
            });
        }

        private void SetTextResetIn()
        {
            if (_txtResetIn)
                _txtResetIn.text = ThetanSDKUtilities.ToStringTimeDigitalDigit(_nextResetData - DateTime.UtcNow);
        }
        
        private void SetCommonUI(HeroNftItem data, string selectedHeroNftId)
        {
            _heroData = data;

            SetDataEquipment(data);

            if (data.grindInfo.IsMaxLifeTime())
            {
                _contentSetSizeBonus.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical, 
                    _baseContentSize + _maxLifeTimeContentBonusSize + _bonusContentPaddingSize);
                
                _contentMaxLifeTime.SetActive(true);
                _contentMaxGrindTimeReset.SetActive(false);
            }
            else if (data.grindInfo.grindTime >= data.grindInfo.maxGrindTime)
            {
                _contentSetSizeBonus.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical, 
                    _baseContentSize + _maxGrindTimeContentBonusSize + _bonusContentPaddingSize);
                
                _nextResetData = data.grindInfo.nextReset;
                SetTextResetIn();
                
                _contentMaxLifeTime.SetActive(false);
                _contentMaxGrindTimeReset.SetActive(true);
                
            }
            else
            {
                _contentSetSizeBonus.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _baseContentSize);
                
                _contentMaxLifeTime.SetActive(false);
                _contentMaxGrindTimeReset.SetActive(false);
            }
            
            if(_nftHeroImg)
                _nftHeroImg.ShowUI(data.ingameInfo);
            
            if(_txtHeroName)
                _txtHeroName.text = data.metaData.name;
            
            if(_txtWorld)
                _txtWorld.text = ThetanSDKUtilities.GetWorldName(data.ingameInfo.gameId);
            
            if(_txtRarity)
            {
                _txtRarity.text = ThetanSDKUtilities.GetHeroRarityName(data.ingameInfo.rarity);
                
                _txtRarity.color = _rarityColorConfig.GetColor(data.ingameInfo.rarity);
            }

            if (_txtGrindTime)
            {
                if (data.grindInfo.IsMaxLifeTime())
                {
                    _txtGrindTime.SetText("--");
                    _txtGrindTime.color = _colorGrindTimeEndLifeTime;
                }
                else
                {
                    _txtGrindTime.SetTextFormat("{0}/{1} m",
                        data.grindInfo.grindTime.ConvertSecondToMinute(),
                        data.grindInfo.maxGrindTime.ConvertSecondToMinute());

                    _txtGrindTime.color = data.grindInfo.grindTime < data.grindInfo.maxGrindTime
                        ? _colorGrindTimeNormal
                        : _colorGrindTimeMax;
                }
                
                _contentGrindTimeLimit.SetActive(!data.grindInfo.IsMaxLifeTime() && 
                                                 data.grindInfo.grindTime >= data.grindInfo.maxGrindTime);
            }

            if (_txtGrindSpeed)
            {
                // Convert THG/s -> THG/h
                _txtGrindSpeed.SetTextFormat("{0}/h", (data.grindInfo.currentGrindSpeed * 3600).FormatUnitCurrency());
            }
            
            if(_txtGrindStage)
                _txtGrindStage.SetText(data.grindInfo.stage);
            
            if(_txtMaxGrindStage)
                _txtMaxGrindStage.SetTextFormat("/{0}", data.grindInfo.maxStage);

            
            if (_txtDailyReward)
                _txtDailyReward.SetText(data.grindInfo.grindPoint.FormatUnitCurrency());
            
            if (_txtGrindAbility)
                _txtGrindAbility.SetTextFormat("{0}%", (data.grindInfo.grindAbility * 100).FormatUnitPercent());
            
            if(_txtGrindAbilityBonus)
                _txtGrindAbilityBonus.SetTextFormat("(+{0}% grind ability)", (data.grindInfo.equipmentEffect * 100).FormatUnitPercent());
            
            SetContentButtonInteractHero(data, selectedHeroNftId);
        }

        private void SetContentButtonInteractHero(HeroNftItem data, string selectedHeroNftId)
        {
            Color colorButton = Color.white;
            _btnInteractHero.onClick.RemoveAllListeners();
            _btnInteractHero.interactable = true;
            _btnInteractHero.image.raycastTarget = true;

            if (data.IsInTransaction())
            {
                colorButton = _colorButtonGrinding;
                _btnInteractHero.interactable = false;
                _btnInteractHero.image.raycastTarget = false;
                
                if(_txtBtnInteractHero)
                {
                    _txtBtnInteractHero.color = _colorTextGrinding;
                    _txtBtnInteractHero.text = "Locked";
                }
            }
            else if (data.id != selectedHeroNftId &&
                !data.grindInfo.IsGrinding())
            {
                colorButton = _colorButtonSelectHero;
                _btnInteractHero.onClick.AddListener(OnClickSelectHero);
                
                if(_txtBtnInteractHero)
                {
                    _txtBtnInteractHero.color = _colorTextSelectHero;
                    _txtBtnInteractHero.text = "Select";
                }
            }
            else if (data.id == selectedHeroNftId &&
                     !data.grindInfo.IsGrinding())
            {
                colorButton = _colorButtonDeselectHero;
                _btnInteractHero.onClick.AddListener(OnClickDeselectHero);
                
                if(_txtBtnInteractHero)
                {
                    _txtBtnInteractHero.color = _colorTextDeselectHero;
                    _txtBtnInteractHero.text = "Unselect";
                }
            }
            else if(data.grindInfo.IsGrinding())
            {
                colorButton = _colorButtonGrinding;
                _btnInteractHero.interactable = true;
                _btnInteractHero.image.raycastTarget = true;
                _btnInteractHero.onClick.AddListener(OnClickInteractWithGrindingHero);
                
                if(_txtBtnInteractHero)
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
        }

        private void SetDataEquipment(HeroNftItem data)
        {
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
                _txtEquipment.SetText(totalSlotEquipped);
            
            if(_txtMaxEquipment)
                _txtMaxEquipment.SetTextFormat("/{0}", totalEquipmentSlot);
        }
        
        private void SetUILoadingDetailGrindInfo()
        {
        }

        private void SetUIDetailGrindInfo(HeroNftItem data, DetailHeroGrindInfo detailHeroGrindInfo)
        {
        }

        private void SetUIErrorDetailGrindInfo()
        {
            if (_txtEquipment)
                _txtEquipment.text = "Error";

            if (_txtMaxEquipment)
                _txtMaxEquipment.text = string.Empty;
    
            if (_txtGrindAbilityBonus)
                _txtGrindAbilityBonus.text = string.Empty;
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
                    ZString.Format(UINftErrorMsg.ERROR_SELECT_HERO_NFT_HERO_IS_GRINDING_IN_ANOTHER_GAME_NAME, _heroData.grindInfo.status.appId),
                    ThetanSDKErrorMsg.Okay);
            }
            else
            {
                _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Warning, 
                    UINftErrorMsg.WARNING_CONFIRM_FORCE_STOP_GRINDING, 
                    ThetanSDKErrorMsg.Decline, ThetanSDKErrorMsg.Confirm,
                    null, () =>
                    {
                        ThetanSDKManager.Instance.StopGrindingHeroItem();
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
                            ZString.Format(UINftErrorMsg.ERROR_DESELECT_HERO_UNKNOWN_ERROR, error.Code, error.Message),
                            ThetanSDKErrorMsg.Okay);
                    });
                    break;
                }
                default:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error,
                        ZString.Format(UINftErrorMsg.ERROR_DESELECT_HERO_UNKNOWN_ERROR, error.Code, error.Message),
                        ThetanSDKErrorMsg.Okay);
                    break;
            }
            
            ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("NFT Detail", "Unselect NFT", true, 
                (NftItemServiceErrorCode)error.Code == NftItemServiceErrorCode.UNKNOWN ? error.DevDebugMessage : error.Message);
        }

        private async void OnClickSelectHero()
        {
            var nftService = ThetanSDKManager.Instance.NftItemService;
            if (_heroData.grindInfo.grindTime >= _heroData.grindInfo.maxGrindTime)
            {
                
                _uiHelperContainer.ShowPopUpMsg(
                    ThetanSDKErrorMsg.Warning,
                    UINftErrorMsg.WARNING_SELECT_HERO_MAX_GRIND_TIME, 
                    "Select other NFT", "Continue",
                    null, CallSelectHeroNFT);
                return;
            }
            else if (_heroData.grindInfo.IsMaxLifeTime())
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

            CallSelectHeroNFT();
        }

        private void ForceStopGrindingAndCallSelectHeroNFT()
        {
            _uiHelperContainer.TurnOnLoading();
            _btnInteractHero.interactable = false;

            var nftService = ThetanSDKManager.Instance.NftItemService;
            
            nftService.EndMatch(() =>
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
                case NftItemServiceErrorCode.NFT_IS_GRINDING_IN_ANOTHER_GAME:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error, 
                        UINftErrorMsg.ERROR_SELECT_HERO_NFT_HERO_IS_GRINDING_IN_ANOTHER_GAME, ThetanSDKErrorMsg.Okay);
                    break;
                case NftItemServiceErrorCode.NFT_DAILY_LIMIT_REACH:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error, 
                        UINftErrorMsg.ERROR_SELECT_HERO_NFT_IS_REACH_DAILY_LIMIT, ThetanSDKErrorMsg.Okay);
                    break;
                case NftItemServiceErrorCode.ANOTHER_NFT_IS_GRINDING:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error, 
                        UINftErrorMsg.ERROR_SELECT_HERO_ANOTHER_IS_GRINDING, ThetanSDKErrorMsg.Okay);
                    break;
                default:
                    _uiHelperContainer.ShowPopUpMsg(ThetanSDKErrorMsg.Error, 
                        ZString.Format(UINftErrorMsg.ERROR_SELECT_HERO_UNKNOWN_ERROR, error.Code, error.Message), 
                        ThetanSDKErrorMsg.Okay);
                    break;
            }
            
            ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("NFT Detail", "Select NFT", 
                true, error.Message);
        }
    }
}