using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class ScreenListNFTPortrait : ScreenListNFT
    {
        [SerializeField] private ScreenDetailHeroNFT prefabScreenDetailHeroNft;

        protected async override void OnSelectItem(HeroNftItem data)
        {
            var instance = await _screenContainer.PushScreen(prefabScreenDetailHeroNft) as ScreenDetailHeroNFT;
            
            instance.SetData(data, _uiHelperContainer, selectedHeroData =>
            {
                if(!selectedHeroData.IsEmpty())
                    _screenContainer.NotifyOnClickCloseScreen();
            });
        }
    }
}