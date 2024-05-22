using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class ScreenOverview : ScreenContentMainUI
    {
        [SerializeField] private TextMeshProUGUI _txtNFTCount;
        [SerializeField] private TextMeshProUGUI _txtGrindTime;
        [SerializeField] private TextMeshProUGUI _txtGrindReward;
        [SerializeField] private TextMeshProUGUI _txtNewSpinEarn;
        [SerializeField] private TextMeshProUGUI _txtTotalSpinAvailable;

        [SerializeField] private Button _btnSelectNFT;
        [SerializeField] private Button _btnGrindDetail;
        [SerializeField] private Button _btnGoToLuckySpin;
        [SerializeField] private Button _btnRefresh;

        [SerializeField] private ScreenDailyGrindSummary _prefabScreenDailyGrindSummary;

        private Action _onClickSelectNFTCallback;

        private bool _isDisable;

        private void Awake()
        {
            _btnSelectNFT.onClick.AddListener(OnClickSelectNFT);
            _btnGrindDetail.onClick.AddListener(OnClickGrindDetail);
            _btnGoToLuckySpin.onClick.AddListener(OnClickGoToLuckySpin);
            _btnRefresh.onClick.AddListener(RefreshData);
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();

            _onClickSelectNFTCallback = null;
        }

        private void OnDisable()
        {
            _isDisable = true;
        }

        private void OnEnable()
        {
            if (_isDisable)
            {
                // In case when this screen is disable, some 
                SetData();
            }

            _isDisable = false;
        }
        
        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            SetData();
        }

        private void SetData()
        {
            var nftService = ThetanSDKManager.Instance.NftItemService;
            if (nftService.CountTotalNFT == -1)
            {
                FetchTotalNFTCount();
            }
            else
            {
                _txtNFTCount.SetText(nftService.CountTotalNFT);
            }
            
            // Todo: wait backend for api get data
            _txtGrindTime.SetText(0);
            _txtGrindReward.SetText(0);
            _txtNewSpinEarn.SetText("+0");
            _txtTotalSpinAvailable.SetText(0);
        }
        
        private void RefreshData()
        {
            FetchTotalNFTCount();
            
            // Todo: wait backend for api get data
            _txtGrindTime.SetText(0);
            _txtGrindReward.SetText(0);
            _txtNewSpinEarn.SetText("+0");
            _txtTotalSpinAvailable.SetText(0);
        }

        private void FetchTotalNFTCount()
        {
            _txtNFTCount.text = "";

            var nftService = ThetanSDKManager.Instance.NftItemService;
            
            nftService.FetchTotalNFTCount(totalNFT =>
            {
                _txtNFTCount.SetText(totalNFT);
            }, error =>
            {
                _txtNFTCount.SetText("Error");
            });
        }


        private void OnClickGoToLuckySpin()
        {
            throw new NotImplementedException();
        }

        private void OnClickGrindDetail()
        {
            _screenContainer.PushScreen(_prefabScreenDailyGrindSummary);
        }

        private void OnClickSelectNFT()
        {
            _onClickSelectNFTCallback?.Invoke();
        }

        public void SetCallback(Action onClickSelectNFTCallback)
        {
            _onClickSelectNFTCallback = onClickSelectNFTCallback;
        }
    }
}
