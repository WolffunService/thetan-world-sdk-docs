using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using ThetanSDK;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI;

public class PopupClaimFreeNft : Popup
{
    [SerializeField] private NFTHeroAvatar _avatarImg;
    [SerializeField] private TextMeshProUGUI _nameTxt;
    [SerializeField] private TextMeshProUGUI _rewardTxt;
    [SerializeField] private Button _claimBtn;
    private Action _onClaimFreeNFTCallback;
    private NftItemService nftItemService;
    private UIHelperContainer _uiHelperContainer;

    private void Awake()
    {
        _claimBtn.onClick.AddListener(ClaimFreeNFT);
    }

    public override void OnBeforePushPopup()
    {
        base.OnBeforePushPopup();

        SetUpView();
    }

    private void SetUpView()
    {
        nftItemService = ThetanSDKManager.Instance.NftItemService;
        var freeNFTConfig = nftItemService.FreeNFTConfig;
        _avatarImg.ShowUI(freeNFTConfig.freeHero);
        _nameTxt.SetText(freeNFTConfig.freeHero.name);
        _rewardTxt.SetText($"{(int)freeNFTConfig.avgFreeEarnHourlyMin} - {(int)freeNFTConfig.avgFreeEarnHourlyMax}/h");
    }

    private void ClaimFreeNFT()
    {
        if (!nftItemService) nftItemService = ThetanSDKManager.Instance.NftItemService;
        nftItemService.ClaimFreeNFT((res) =>
        {
            _uiHelperContainer.ShowToast("Claim Success");
            _onClaimFreeNFTCallback?.Invoke();
        }, HandleClaimFreeNFTError);
        _popupContainer.Pop();
    }

    private void HandleClaimFreeNFTError(WolffunResponseError error)
    {
        switch ((NftItemServiceErrorCode)error.Code)
        {
            case NftItemServiceErrorCode.FREE_NFT_CLAIMED:
                _uiHelperContainer.ShowToast("Claim Success");
                _onClaimFreeNFTCallback?.Invoke();
                break;
            default:
                _uiHelperContainer.ShowPopUpMsg("Error",
                    "Unknown error occured when claim free NFT. Please try again later or contact technical support.",
                    "Confirm", () =>
                    {
                        _onClaimFreeNFTCallback?.Invoke();
                    });
                break;
        }
    }

    public void Initialize(UIHelperContainer uiHelperContainer, Action onClaimFreeNFTCallback)
    {
        _uiHelperContainer = uiHelperContainer;
        _onClaimFreeNFTCallback = onClaimFreeNFTCallback;
    }
}
