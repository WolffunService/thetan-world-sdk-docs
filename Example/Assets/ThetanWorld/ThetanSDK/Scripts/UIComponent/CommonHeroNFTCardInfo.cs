using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class CommonHeroNFTCardInfo : MonoBehaviour
    {
        private const float UPDATE_FREE_NFT_UI_SECOND_INTERVAL = 5;
        [SerializeField] private NFTHeroAvatar _nftHeroAvatar;
        [SerializeField] private CommonSlider _sliderGrindTime;
        [SerializeField] private Button _btnMoreInfo;
        [SerializeField] private TextMeshProUGUI _txtNFTName;
        [SerializeField] private TextMeshProUGUI _txtGrindTimeStatus;

        [SerializeField] private string _txtFreeNFTTimeOut = "Time Out";

        private const string TXT_MAX_GRIND_TIME = "Max Grind Time";
        private const string TXT_MAX_LIFE_TIME = "Max Total Grind Time";
        
        private HeroNftItem _heroNftItem;

        private float _countTimeUpdateFreeNFTUI;

        private void Awake()
        {
            if(_btnMoreInfo)
                _btnMoreInfo.onClick.AddListener(OnClickMoreInfo);
            _sliderGrindTime.SetValueConvertToStringFunction(value => value.ConvertSecondToMinute());
        }

        private void Update()
        {
            if (_heroNftItem.IsEmpty() || _heroNftItem.nftType != NFTType.FreeNFT)
                return;

            _countTimeUpdateFreeNFTUI += Time.unscaledDeltaTime;

            if (_countTimeUpdateFreeNFTUI >= UPDATE_FREE_NFT_UI_SECOND_INTERVAL)
            {
                _countTimeUpdateFreeNFTUI = 0;
                
                SetUIGrindTime(_heroNftItem);
            }
        }

        private void OnClickMoreInfo()
        {
            if (_heroNftItem.IsEmpty())
                return;
            
            // Todo: implement show hero detail
        }

        public void SetSpriteCacheManager(SpriteCacheManager spriteCacheManager)
        {
            _nftHeroAvatar.SetSpriteCacheManager(spriteCacheManager);
        }

        public void SetData(HeroNftItem heroNFT)
        {
            _countTimeUpdateFreeNFTUI = 0;
            _heroNftItem = heroNFT;
            _nftHeroAvatar.ShowUI(heroNFT.ingameInfo);
            _txtNFTName.text = heroNFT.metaData.name;

            SetUIGrindTime(heroNFT);
        }

        private void SetUIGrindTime(HeroNftItem heroNFT)
        {
            if (heroNFT.nftType == NFTType.FreeNFT)
            {
                var freeNFTInfo = ThetanSDKManager.Instance.NftItemService.FreeNftInfo;

                var freeNFTGrindTime = ThetanSDKUtilities.ConvertFreeNFTInfoToGrindTimeInfo(freeNFTInfo, heroNFT);
                
                _sliderGrindTime.SetData(0,
                    freeNFTGrindTime.maxTime, 
                    heroNFT.grindInfo.grindTime);
                
                if(freeNFTGrindTime.grindTime >= freeNFTGrindTime.maxTime)
                    _sliderGrindTime.UseColorFull();

                if (_txtGrindTimeStatus)
                {
                    if(freeNFTGrindTime.grindTime >= freeNFTGrindTime.maxTime)
                        _txtGrindTimeStatus.text = _txtFreeNFTTimeOut;
                    else
                        _txtGrindTimeStatus.text = string.Empty;
                }
            }
            else
            {
                _sliderGrindTime.SetData(0, 
                    heroNFT.grindInfo.maxGrindTime, 
                    heroNFT.grindInfo.grindTime);
                
                if (_txtGrindTimeStatus)
                {
                    if (heroNFT.grindInfo.IsMaxLifeTime())
                        _txtGrindTimeStatus.text = TXT_MAX_LIFE_TIME;
                    else if(heroNFT.grindInfo.grindTime >= heroNFT.grindInfo.maxGrindTime)
                        _txtGrindTimeStatus.text = TXT_MAX_GRIND_TIME;
                    else
                        _txtGrindTimeStatus.text = string.Empty;
                }
            }
        }
    }
}

