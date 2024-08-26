using System;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Log;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.StorageResource;

namespace ThetanSDK.UI
{
    public class UIHeroNftCard : MonoBehaviour
    {
        [SerializeField] private CommonHeroNFTCardInfo _commonHeroNftCardInfo;
        [SerializeField] private Button _btnSelect;
        [SerializeField] private GameObject _imgSelected;
        [SerializeField] private GameObject _contentSpecialNFT;

        [SerializeField] private GameObject _contentMaxGrindTime;
        [SerializeField] private TextMeshProUGUI _txtResetIn;
        [SerializeField] private GameObject _contentMaxLifeTime;
        [SerializeField] private GameObject _contentSelectNft;
        [SerializeField] private Button _btnSelectNFT;

        [SerializeField] private TextMeshProUGUI _txtTitleGrindLimit;

        private const string TITLE_GRIND_LIMIT_NORMAL_NFT = "Reached Daily Grind Limit";
        private const string TITLE_GRIND_LIMIT_FREE_NFT = "Time out. Wait for the reset to start again.";


        private Action _onClick;
        private Action _onSelect;
        private Action _onNeedRefreshDataCallback;

        private uint _loadImgId = 0;

        private HeroNftItem _data;
        
        private DateTime _nextResetData;

        private float _countTime;
        
        private void Awake()
        {
            _btnSelect.onClick.AddListener(OnClickNFTItem);
            _btnSelectNFT.onClick.AddListener(OnClickSelectItem);
        }

        public void SetSpriteCacheManager(SpriteCacheManager _spriteCacheManager)
        {
            _commonHeroNftCardInfo.SetSpriteCacheManager(_spriteCacheManager);
        }

        private void Update()
        {
            // if (_data.grindInfo == null)
            //     return;
            
            if (!(_data.grindInfo.grindTime >= _data.grindInfo.maxGrindTime) || 
                _data.grindInfo.maxGrindTime == 0)
                return;
            
            _countTime -= Time.unscaledDeltaTime;

            if (_countTime > 0)
            {
                return;
            }
            
            _countTime = 1;
            
            var grindTime = _data.grindInfo.grindTime;
            var maxGrindTime = _data.grindInfo.maxGrindTime;
            
            if (_data.nftType == NFTType.FreeNFT)
            {
                var freeNFTGrindTimeInfo = ThetanSDKUtilities.ConvertFreeNFTInfoToGrindTimeInfo(
                    ThetanSDKManager.Instance.NftItemService.FreeNftInfo, _data);
                grindTime = freeNFTGrindTimeInfo.grindTime;
                maxGrindTime = freeNFTGrindTimeInfo.maxTime;
            }
            
            if(grindTime >= maxGrindTime)
                SetTextNextReset();

            if(_onNeedRefreshDataCallback != null && 
               (_nextResetData - DateTime.UtcNow).TotalSeconds <= 0 && 
               _data.nftType == NFTType.NormalNFT)
            {
                _onNeedRefreshDataCallback?.Invoke();

                // For the next 10 second, do not invoke refresh again to avoid spam
                _countTime = 10;
            }
        }
        
        private void OnClickSelectItem()
        {
            _onSelect?.Invoke();
        }
        
        private void OnClickNFTItem()
        {
            _onClick?.Invoke();
        }

        public void SetData(HeroNftItem data, bool isSelected, Action onClick, Action onSelect, Action onNeedRefreshDataCallback = null)
        {
            _onClick = onClick;
            _onSelect = onSelect;
            _onNeedRefreshDataCallback = onNeedRefreshDataCallback;
            _data = data;

            _txtTitleGrindLimit.text = data.nftType == NFTType.NormalNFT
                ? TITLE_GRIND_LIMIT_NORMAL_NFT
                : TITLE_GRIND_LIMIT_FREE_NFT;
            
            _contentSpecialNFT.SetActive(data.nftType == NFTType.FreeNFT);
            _commonHeroNftCardInfo.SetData(data);
            
            // if (data.grindInfo == null)
            // {
            //     if(_txtGrindReward)
            //         _txtGrindReward.text = "Error";
            //
            //     if (_txtEquipment)
            //         _txtEquipment.text = "Error";
            //     
            //     if(_contentMaxGrindTime)
            //         _contentMaxGrindTime.SetActive(false);
            //     
            //     return;
            // }
            var grindTime = data.grindInfo.grindTime;
            var maxGrindTime = data.grindInfo.maxGrindTime;

            if (data.nftType == NFTType.FreeNFT)
            {
                var freeNFTGrindTimeInfo = ThetanSDKUtilities.ConvertFreeNFTInfoToGrindTimeInfo(
                    ThetanSDKManager.Instance.NftItemService.FreeNftInfo, data);
                grindTime = freeNFTGrindTimeInfo.grindTime;
                maxGrindTime = freeNFTGrindTimeInfo.maxTime;
            }
            
            if (data.grindInfo.IsMaxLifeTime())
            {
                _contentMaxGrindTime.SetActive(false);
                _contentMaxLifeTime.SetActive(true);
                _contentSelectNft.SetActive(false);
            }
            else if (grindTime >= maxGrindTime &&
                     maxGrindTime != 0)
            {
                _contentMaxGrindTime.SetActive(true);
                _contentMaxLifeTime.SetActive(false);
                _contentSelectNft.SetActive(false);

                if(data.nftType == NFTType.NormalNFT)
                    _nextResetData = data.grindInfo.nextReset;
                else if (data.nftType == NFTType.FreeNFT)
                    _nextResetData = ThetanSDKManager.Instance.NftItemService.FreeNftInfo.nextResetGrindEarn;
                
                _countTime = 1;
                
                SetTextNextReset();
            }
            else
            {
                _contentMaxGrindTime.SetActive(false);
                _contentMaxLifeTime.SetActive(false);
                
                _contentSelectNft.SetActive(!isSelected);
            }
            
            _imgSelected.gameObject.SetActive(isSelected);
        }

        private void SetTextNextReset()
        {
            _txtResetIn.text =
                $"Reset in: <color=#4845F5>{ThetanSDKUtilities.ToStringTimeShort(_nextResetData - DateTime.UtcNow)}</color>";
        }

        public void ChangeIsSelected(bool isSelected)
        {
            _imgSelected.gameObject.SetActive(isSelected);
        }
    }
}