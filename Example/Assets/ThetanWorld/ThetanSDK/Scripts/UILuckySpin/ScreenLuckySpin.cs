using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using ThetanSDK.SDKService.LuckySpin;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Random = System.Random;

namespace ThetanSDK.UI.LuckySpin
{
    internal class ScreenLuckySpin : ScreenContentMainUI
    {
        [SerializeField] private TextMeshProUGUI _txtTitle;
        [SerializeField] private LuckySpinItemSpawner _luckySpinItemSpawn;
        [SerializeField] private Slider _sliderProcessTime;
        [SerializeField] private TextMeshProUGUI _txtProcessTime;
        [SerializeField] private TextMeshProUGUI _txtAvailableSpin;
        [SerializeField] private Button _btnSpinOnThetanWorld;
        
        private void Awake()
        {
            _btnSpinOnThetanWorld.onClick.AddListener(OnClickSpinOnThetanWorld);
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            
            bool isEnableBtnSpin =
                ThetanSDKManager.Instance.RemoteConfigService.RemoteConfig.mkpUrlConfig.enableBtnSpin;
            _btnSpinOnThetanWorld.gameObject.SetActive(isEnableBtnSpin);
            
            ShowUI();
        }

        private void ShowUI()
        {
            var luckySpinService = ThetanSDKManager.Instance.LuckySpinService;

            SetData(luckySpinService.CacheData);
            
            luckySpinService.CallGetDataLuckySpin(SetData, null);

            if (!luckySpinService.LuckySpinConfig.IsEmpty())
            {
                ShowListSpinItem(luckySpinService.LuckySpinConfig);
            }
            else
            {
                luckySpinService.CallGetLuckySpinConfig(ShowListSpinItem, null);
            }
            
        }

        private void ShowListSpinItem(LuckySpinConfig configData)
        {
            if (configData.IsEmpty())
                return;
            
            List<LuckySpinItemUIData> _listData = new List<LuckySpinItemUIData>();
            foreach (var itemConfig in configData.rewardUIs)
            {
                if (itemConfig.isGoodLuck)
                {
                    _listData.Add(new LuckySpinItemUIData()
                    {
                        itemType = LuckySpinItemType.Empty,
                    });
                }
                else if (itemConfig.IsTicket())
                {
                    _listData.Add(new LuckySpinItemUIData()
                    {
                        itemType = LuckySpinItemType.Ticket,
                    });
                }
                else if (itemConfig.IsEquipment())
                {
                    _listData.Add(new LuckySpinItemUIData()
                    {
                        itemType = LuckySpinItemType.Equipment,
                        equipmentType = itemConfig.equipmentType
                    });
                }
            }
            ThetanSDKUtilities.Shuffle(_listData);
            _luckySpinItemSpawn.SpawnListItems(_listData);
        }

        private void SetData(LuckySpinData luckySpinData)
        {
            _sliderProcessTime.minValue = 0;
            _sliderProcessTime.maxValue = luckySpinData.grindSecsPerSpin;
            _sliderProcessTime.value = luckySpinData.currentGrindSecs;
            _txtProcessTime.SetTextFormat("{0}m", luckySpinData.currentGrindSecs/60);

            var availableSpin = luckySpinData.spinChance;
            _txtAvailableSpin.SetText(availableSpin);

            if (availableSpin <= 0)
                _txtTitle.text = "Lucky Spin";
            else
                _txtTitle.SetTextFormat("Lucky Spin ({0})", availableSpin);
        }
        
        private void OnClickSpinOnThetanWorld()
        {
            Application.OpenURL(ThetanSDKManager.Instance.RemoteConfigService.RemoteConfig.mkpUrlConfig.urlTabSpin);
        }
    }
}
