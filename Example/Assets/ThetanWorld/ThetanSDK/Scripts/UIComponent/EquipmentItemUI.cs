using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.StorageResource;

namespace ThetanSDK.UI
{
    public struct EquipmentItemUIData
    {
        public int equipmentId;
        public NFTRarity rarity;
        public bool isEquipped;
        public int itemStar;
        public bool isCanUpgrade;
        public bool isCanEquip;
    }

    public class EquipmentItemUI : MonoBehaviour
    {
        [SerializeField] private Image _imgItem;
        [SerializeField] private Image _imgRarityBackground;
        [SerializeField] private Image _imgStarBackground;
        [SerializeField] private List<Image> _listImgStars;
        [SerializeField] private Sprite _spriteStarYellow;
        [SerializeField] private Sprite _spriteStarEmpty;
        [SerializeField] private Image _imgBlackCover;
        [SerializeField] private GameObject _objCanUpgrade;
        [SerializeField] private GameObject _objCanEquip;
        [SerializeField] private Button _btnInteract;
        [SerializeField] private RectTransform _textTooltipPosition;

        private SpriteCacheManager _spriteCacheManager;

        private int _currentLoadId;
        private EquipmentItemUIData _data;
        private UIHelperContainer _uiHelperContainer;

        private void Awake()
        {
            _imgItem.SetAlphaImg(0);
            _imgStarBackground.enabled = false;

            for (int i = 0; i < _listImgStars.Count; i++)
            {
                _listImgStars[i].SetAlphaImg(0);
            }
            
            _btnInteract.onClick.AddListener(OnClickInteractWithItemSlot);
        }

        public void SetSpriteCacheManager(SpriteCacheManager spriteCacheManager)
        {
            _spriteCacheManager = spriteCacheManager;
        }

        public void ShowEmptySlotForItem(NFTRarity nftRarity)
        {
            _imgItem.SetAlphaImg(0);
            ShowImgRarityBackground(nftRarity);
            _imgBlackCover.enabled = true;
            _imgStarBackground.enabled = false;
            
            if(_objCanUpgrade)
                _objCanUpgrade.SetActive(false);
            
            if(_objCanEquip)
                _objCanEquip.SetActive(false);
            for (int i = 0; i < _listImgStars.Count; i++)
            {
                _listImgStars[i].SetAlphaImg(0);
            }
        }

        public void SetUI(EquipmentItemUIData data, UIHelperContainer uiHelperContainer)
        {
            _data = data;
            _uiHelperContainer = uiHelperContainer;
            
            LoadImgIcon(data.equipmentId);

            ShowImgRarityBackground(data.rarity);
            
            if(_objCanUpgrade)
                _objCanUpgrade.SetActive(data.isEquipped && data.isCanUpgrade);
            
            if(_objCanEquip)
                _objCanEquip.SetActive(!data.isEquipped && data.isCanEquip);

            if (data.isEquipped)
            {
                _imgBlackCover.enabled = false;

                _imgStarBackground.enabled = true;
                for (int i = 0; i < _listImgStars.Count; i++)
                {
                    _listImgStars[i].SetAlphaImg(1);
                    _listImgStars[i].sprite = i + 1 > data.itemStar ? _spriteStarEmpty : _spriteStarYellow;
                }
            }
            else
            {
                _imgBlackCover.enabled = true;

                _imgStarBackground.enabled = false;
                for (int i = 0; i < _listImgStars.Count; i++)
                {
                    _listImgStars[i].SetAlphaImg(0);
                }
            }
        }

        private void ShowImgRarityBackground(NFTRarity nftRarity)
        {
            _imgRarityBackground.sprite =
                Resources.Load<Sprite>($"TexturesRarityBG/EquipmentRarity_{nftRarity.ToString()}");
        }

        private async void LoadImgIcon(int equipmentId)
        {
            _imgItem.SetAlphaImg(0);

            var loadId = _currentLoadId + 1;
            _currentLoadId = loadId;

            var url = GetUrlEquipmentIcon(equipmentId);

            var textureIcon = await StorageResource.LoadImg(url);

            if (textureIcon == null)
                return;
            
            if (_currentLoadId != loadId)
            {
                return;
            }
#if UNITY_2021_3_OR_NEWER
            textureIcon.ignoreMipmapLimit = true;
#endif

            Sprite spriteIcon = null;

            if (_spriteCacheManager != null)
                spriteIcon = _spriteCacheManager.CreateAndCacheSprite(url, textureIcon);
            else
                spriteIcon = ThetanSDKUtilities.CreateSpriteFromTexture2D(textureIcon);

            _imgItem.sprite = spriteIcon;
            _imgItem.SetAlphaImg(1);
        }

        private string GetUrlEquipmentIcon(int equipmentId)
        {
            return $"/resources/thetan_world/equipment/avatar/{equipmentId}_64.png";
        }
        
        private void OnClickInteractWithItemSlot()
        {
            if (_uiHelperContainer == null)
                return;
            
            if (_data.isCanEquip)
            {
                _uiHelperContainer.ShowTextTooltip("Equipment cannot be attached in-game", 
                    _textTooltipPosition, TooltipAlignment.BottomMiddle);
            }
            else if (_data.isCanUpgrade)
            {
                _uiHelperContainer.ShowTextTooltip("Equipment cannot be upgraded in-game", 
                    _textTooltipPosition, TooltipAlignment.BottomMiddle);
            }
        }
    }
}