using System;
using Cysharp.Threading.Tasks;
using ThetanSDK;
using ThetanSDK.SDKServices.Profile;
using ThetanSDK.UI;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI;

public class CheckDoThetanGateTutorial : MonoBehaviour
{
    private const string TUTORIAL_KEY = "ThetanGateUITutorial";
    [SerializeField] protected UIHelperContainer _uiHelperContainer;
    [SerializeField] private PopupTutorialStep _popupTutorialStep;
    [SerializeField] private Image imgBlockRaycast;
    private Canvas canvasComp;
    private GraphicRaycaster raycastComp;

    [Header("Claim NFT Hero")]
    [SerializeField] private PopupClaimFreeNft _prefabPopupClaimFreeNft;

    [Header("Intro NFT Hero")]
    [SerializeField] private GameObject _introNFTHeroFocusGameObject;
    [SerializeField] private RectTransform _introNFTHeroFocusTooltipTransform;
    [SerializeField] private TooltipAlignment _introNFTHeroFocusTooltipAlignment;

    [Header("Intro Task")]
    [SerializeField] private GameObject _introTaskFocusGameObject;
    [SerializeField] private RectTransform _introTaskFocusTooltipTransform;
    [SerializeField] private TooltipAlignment _introTaskFocusTooltipAlignment;

    [Header("Intro Play")]
    [SerializeField] private PopupStartPlay _prefabPopupStartPlay;

    private Action _requestCloseScreenCallback;

    private bool _isDoingTutorial;

    private void Awake()
    {
        _isDoingTutorial = false;
    }

    public void CheckDoTutorial(Action requestCloseScreenCallback)
    {
        if (_isDoingTutorial)
            return;

        _isDoingTutorial = true;
        _requestCloseScreenCallback = requestCloseScreenCallback;
        var nftItemService = ThetanSDKManager.Instance.NftItemService;

        if (string.IsNullOrEmpty(nftItemService.FreeNftInfo.nftId))
        {
            StartOnboarding();
        }
        else if (!PlayerPrefs.HasKey(TUTORIAL_KEY) && PlayerPrefs.GetInt(TUTORIAL_KEY) != 1)
        {
            IntroNFTHeroFocus();
        }
        else 
            _isDoingTutorial = false;
    }

    private void FocusOnGameObject(GameObject focusGameObject)
    {
        imgBlockRaycast.gameObject.SetActive(true);
        canvasComp = focusGameObject.AddComponent<Canvas>();
        //raycastComp = focusGameObject.AddComponent<GraphicRaycaster>();

        canvasComp.overrideSorting = true;
        canvasComp.sortingOrder = 10000;
        canvasComp.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
    }

    private UniTask FinishFocus()
    {
        imgBlockRaycast.gameObject.SetActive(false);

        //Destroy(raycastComp);
        Destroy(canvasComp);

        return UniTask.CompletedTask;
    }

    private async void StartOnboarding()
    {
        var popupAccountInfo = await _uiHelperContainer.PushPopup(_prefabPopupClaimFreeNft,
            new PopupOption() { IsAllowBackdrop = true }) as PopupClaimFreeNft;

        popupAccountInfo.Initialize(_uiHelperContainer, () =>
        {
            if (PlayerPrefs.HasKey(TUTORIAL_KEY) && PlayerPrefs.GetInt(TUTORIAL_KEY) == 1) // Already do tutorial, do not do it again
            {
                return;
            }

            IntroNFTHeroFocus();
        });
    }

    private async void IntroNFTHeroFocus()
    {
        PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
        FocusOnGameObject(_introNFTHeroFocusGameObject);
        var popup = await _uiHelperContainer.PushPopup(_popupTutorialStep, new PopupOption() { IsAllowBackdrop = false }) as PopupTutorialStep;
        popup.Initialize("Selected NFT is ready to earn", () =>
        {
            FinishFocus();
            IntroTaskFocus();
        },
        _introNFTHeroFocusTooltipTransform, _introNFTHeroFocusTooltipAlignment);
    }

    private async void IntroTaskFocus()
    {
        FocusOnGameObject(_introTaskFocusGameObject);
        var popupTutorialStep = await _uiHelperContainer.PushPopup(_popupTutorialStep, new PopupOption() { IsAllowBackdrop = false }) as PopupTutorialStep;
        popupTutorialStep.Initialize("Complete tasks to earn your rewards!", EndIntroTaskFocus,
        _introTaskFocusTooltipTransform, _introTaskFocusTooltipAlignment);
    }

    private async void EndIntroTaskFocus()
    {
        await FinishFocus();
        var popup = await _uiHelperContainer.PushPopup(_prefabPopupStartPlay, new PopupOption() { IsAllowBackdrop = true }) as PopupStartPlay;
        popup.Initialize(() => { IntroNFTHeroSwitchFocus(); });
    }

    private async void IntroNFTHeroSwitchFocus()
    {
        var nftItemService = ThetanSDKManager.Instance.NftItemService;
        var profileService = ThetanSDKManager.Instance.ProfileService;
        if (nftItemService.CountTotalNFT < 2)
        {
            if (string.IsNullOrEmpty(profileService.Email) &&
                nftItemService.IsSelectedAnyHeroNFT())
            {
                _requestCloseScreenCallback?.Invoke();
                _isDoingTutorial = false;
                return;
            }
            else
            {
                _isDoingTutorial = false;
                return;
            }
        }

        FocusOnGameObject(_introNFTHeroFocusGameObject);
        var popup = await _uiHelperContainer.PushPopup(_popupTutorialStep, new PopupOption() { IsAllowBackdrop = false }) as PopupTutorialStep;
        popup.Initialize("Switch NFTs to earn more rewards",
            () =>
            {
                FinishFocus();
                _isDoingTutorial = false;
            }, _introNFTHeroFocusTooltipTransform, TooltipAlignment.RightMiddle);
    }
}
