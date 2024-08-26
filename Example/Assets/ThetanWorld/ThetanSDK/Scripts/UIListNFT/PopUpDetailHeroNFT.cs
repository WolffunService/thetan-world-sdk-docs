using System;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class PopUpDetailHeroNFT : Popup
    {
        [SerializeField] private DetailHeroNFTUI _detailHeroNFTUi;
        [SerializeField] private Button _btnClose;

        private HeroNftItem _heroData;
        private Action<HeroNftItem> _onSelectCallback;
        private UIHelperContainer _uiHelperContainer;

        private void Awake()
        {
            _btnClose.onClick.AddListener(ClosePopup);
        }

        private void ClosePopup()
        {
            if(_popupContainer)
                _popupContainer.Pop();
        }

        public override void OnAfterPopPopup()
        {
            base.OnAfterPopPopup();
            
            _heroData = new HeroNftItem();
            _onSelectCallback = null;
            _uiHelperContainer = null;
            _detailHeroNFTUi.ClearCache();
        }

        public void SetData(HeroNftItem data, UIHelperContainer uiHelperContainer, Action<HeroNftItem> onSelectCallback)
        {
            _onSelectCallback = onSelectCallback;
            _uiHelperContainer = uiHelperContainer;
            _heroData = data;

            _detailHeroNFTUi.SetData(data, uiHelperContainer, OnSelectHeroNFTCallback);
        }

        private void OnSelectHeroNFTCallback(HeroNftItem selectedNFT)
        {
            _onSelectCallback?.Invoke(selectedNFT);
            ClosePopup();
        }
    }
}