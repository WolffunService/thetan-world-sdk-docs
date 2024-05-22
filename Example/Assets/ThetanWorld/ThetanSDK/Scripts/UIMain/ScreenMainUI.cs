using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ThetanSDK.UI
{
    public abstract class ScreenMainUI : Screen
    {
        private const string KEY_CHECK_FIRST_TIME_OPEN_SDK = "OpenSDKFirstTime";
        
        [SerializeField] protected UIHelperContainer _uiHelperContainer;
        [SerializeField] private PopupWelcomeToSDK _prefabPopupWelcomeToSDK;
        
        public abstract void ShowTabLuckySpin();
        public abstract void ShowTabNFT();
        public abstract void ShowTabTournament();
        
        public override async void OnAfterPushScreen()
        {
            base.OnAfterPushScreen();

            if (!PlayerPrefs.HasKey(KEY_CHECK_FIRST_TIME_OPEN_SDK) &&
                PlayerPrefs.GetInt(KEY_CHECK_FIRST_TIME_OPEN_SDK) != 1)
            {
                await UniTask.Delay(350);
                
                ShowPopupWelcomeToSDK();
                PlayerPrefs.SetInt(KEY_CHECK_FIRST_TIME_OPEN_SDK, 1);
            }
        }
        
        private async void ShowPopupWelcomeToSDK()
        {
            var popup = await _uiHelperContainer.PushPopup(_prefabPopupWelcomeToSDK, new PopupOption()
            {
                IsAllowBackdrop = true,
                IsBackdropCanClosePopup = false
            }) as PopupWelcomeToSDK;

            if (popup == null)
            {
                return;
            }
            
            popup.SetCallback(ShowTabNFT);
        }
    }
}