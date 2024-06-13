using System;
using Cysharp.Text;
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
        [SerializeField] private NFTHeroImg nftHeroImg;
        [SerializeField] private TextMeshProUGUI _txtGrindSpeed;
        [SerializeField] private TextMeshProUGUI _txtGrindAbility;
        [SerializeField] private Button _btnSelect;
        [SerializeField] private Image _imgSelected;

        [SerializeField] private GameObject _contentMaxGrindTime;
        [SerializeField] private TextMeshProUGUI _txtResetIn;
        [SerializeField] private GameObject _contentMaxLifeTime;
        [SerializeField] private Image _imgBgBottom;
        [SerializeField] private Color _colorImgBottomNormal;
        [SerializeField] private Color _colorImgBottomMaxTime;


        private Action _onSelect;
        private Action _onNeedRefreshDataCallback;

        private uint _loadImgId = 0;

        private HeroNftItem _data;
        
        private DateTime _nextResetData;

        private float _countTime;
        
        private void Awake()
        {
            _btnSelect.onClick.AddListener(OnClickSelect);
        }

        public void SetSpriteCacheManager(SpriteCacheManager _spriteCacheManager)
        {
            nftHeroImg.SetSpriteCacheManager(_spriteCacheManager);
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
            SetTextNextReset();

            if(_onNeedRefreshDataCallback != null && 
               (_nextResetData - DateTime.UtcNow).TotalSeconds <= 0)
            {
                _onNeedRefreshDataCallback?.Invoke();

                // For the next 10 second, do not invoke refresh again to avoid spam
                _countTime = 10;
            }
        }

        private void OnClickSelect()
        {
            _onSelect?.Invoke();
        }

        public void SetData(HeroNftItem data, bool isSelected, Action onSelect, Action onNeedRefreshDataCallback = null)
        {
            _onSelect = onSelect;
            _onNeedRefreshDataCallback = onNeedRefreshDataCallback;
            _data = data;
            
            nftHeroImg.ShowUI(data.ingameInfo);
            
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
            
            if(_txtGrindSpeed) // Convert THG/s -> THG/h
                _txtGrindSpeed.text = ZString.Format("{0}/h", (data.grindInfo.currentGrindSpeed * 3600).FormatUnitCurrency()); 

            if (_txtGrindAbility)
                _txtGrindAbility.text = ZString.Format("{0}%", (data.grindInfo.grindAbility * 100).FormatUnitPercent());

            if (data.grindInfo.IsMaxLifeTime())
            {
                _contentMaxGrindTime.SetActive(false);
                _contentMaxLifeTime.SetActive(true);
                if(_imgBgBottom)
                    _imgBgBottom.color = _colorImgBottomMaxTime;
            }
            else if (data.grindInfo.grindTime >= data.grindInfo.maxGrindTime &&
                data.grindInfo.maxGrindTime != 0)
            {
                _contentMaxGrindTime.SetActive(true);
                _contentMaxLifeTime.SetActive(false);

                if(_imgBgBottom)
                    _imgBgBottom.color = _colorImgBottomMaxTime;

                _nextResetData = data.grindInfo.nextReset;

                _countTime = 1;
                
                SetTextNextReset();
            }
            else
            {
                if(_imgBgBottom)
                    _imgBgBottom.color = _colorImgBottomNormal;
                _contentMaxGrindTime.SetActive(false);
                _contentMaxLifeTime.SetActive(false);
            }
            
            _imgSelected.enabled = isSelected;
        }

        private void SetTextNextReset()
        {
            _txtResetIn.text = ZString.Format("Reset in: <color=#53EDFE>{0}</color>",
                ThetanSDKUtilities.ToStringTimeShort(_nextResetData - DateTime.UtcNow));
        }

        public void ChangeIsSelected(bool isSelected)
        {
            _imgSelected.enabled = isSelected;
        }
    }
}