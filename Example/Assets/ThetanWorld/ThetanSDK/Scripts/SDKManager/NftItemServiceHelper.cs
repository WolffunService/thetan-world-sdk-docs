using System;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKServices.Analytic;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK
{
    /// <summary>
    /// Class extend behavior of nft item service to support UI Thetan World
    /// </summary>
    public class NftItemServiceHelper
    {
        private UIHelperContainer _uiHelperContainer;
        private NftItemService _nftItemService;
        private SDKAnalyticService _analyticService;
        
        private int _matchMaxDuration;

        internal NftItemServiceHelper(
            UIHelperContainer uiHelperContainer, 
            NftItemService nftItemService,
            SDKAnalyticService analyticService)
        {
            _uiHelperContainer = uiHelperContainer;
            _nftItemService = nftItemService;
            _analyticService = analyticService;
        }

        public void SetMatchMaxDuration(int value)
        {
            _matchMaxDuration = value;
        }
        
        public async UniTask<HeroNftItem> GetHeroNftItemInfo(string id)
        {
            if (_nftItemService.ListHeroNftItems != null)
            {
                foreach (var item in _nftItemService.ListHeroNftItems)
                {
                    if (item.id == id)
                        return item;
                }
            }
            
            if (!ThetanSDKManager.Instance.IsVersionSupported)
            {
                return new HeroNftItem().SetDefault();
            }

            UniTaskCompletionSource<HeroNftItem> loadItemCompletionSource = new UniTaskCompletionSource<HeroNftItem>();
            
            _nftItemService.GetInfoDataHeroNftOnServer(id, data =>
            {
                loadItemCompletionSource.TrySetResult(data);
            }, error => loadItemCompletionSource.TrySetResult(new HeroNftItem()));

            return await loadItemCompletionSource.Task;
        }
        
        /// <summary>
        /// Lock NFT and prepare NFT for grinding session. Also, this function will lock interaction for UI Thetan World,
        /// UI can only be unlocked after you call UnlockButtonMain at the end of grinding session.
        /// onErrorCallback will be call when cannot prepare match for NFT, error callback contain 1 of these error codes, can be access via WolffunResponseError.Code
        /// </summary>
        /// <param name="onSuccessCallback">callback when success prepare match with selected NFT</param>
        /// <param name="onErrorCallback">callback contain error info when prepare match is not success.</param>
        public void PrepareMatchForSelectedNFT(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (!ThetanSDKManager.Instance.IsVersionSupported)
            {
                _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                    "This Thetan World version is out dated. Please update your application to newest version.", 
                    AuthenErrorMsg.Confirm);
                onErrorCallback?.Invoke(new WolffunResponseError((int)NftItemServiceErrorCode.SDK_VERSION_NOT_SUPPORTED,
                    "This Thetan World version is out dated. Please update your application to newest version."));
                return;
            }
            
            _nftItemService.StartMatch(_matchMaxDuration, _ => onSuccessCallback?.Invoke(), error =>
            {
                onErrorCallback?.Invoke(error);
                HandleAnalyticErrorPrepareBattle(error, false);
            });
        }
        
        /// <summary>
        /// Lock NFT and prepare NFT for grinding session.
        /// This function is similar to PrepareMatchForSelectedNFT but it auto show error message and ask user if user want to continue playing without grinding when there is an error.
        /// </summary>
        /// <param name="onSuccessCallback">callback when call prepare match for NFT success or there is error but user confirm start match without grind NFT.</param>
        /// <param name="onErrorCallback">callback when call prepare grind has error and user confirm to go back.</param>
        public void PrepareMatchForSelectedNFTAutoHandleError(Action<NftItemServiceErrorCode> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (!ThetanSDKManager.Instance.IsVersionSupported)
            {
                _uiHelperContainer.ShowPopUpMsg("Version out dated", 
                    "This Thetan World version is out dated. Please update your application to newest version.", 
                    AuthenErrorMsg.Confirm);
                onErrorCallback?.Invoke(new WolffunResponseError((int)NftItemServiceErrorCode.SDK_VERSION_NOT_SUPPORTED,
                    "This Thetan World version is out dated. Please update your application to newest version."));
                return;
            }
            
            _nftItemService.StartMatch(_matchMaxDuration, _ => onSuccessCallback?.Invoke(NftItemServiceErrorCode.SUCCESS),
                error =>
                {
                    if (error.Code == (int)WSErrorCode.ServerMaintenance)
                    {
                        _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_SERVER_MAINTAIN,
                            "Continue", "Go back",
                            () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                            () =>
                            {
                                onErrorCallback?.Invoke(error);
                            });
                        return;
                    }
                    
                    switch ((NftItemServiceErrorCode)error.Code)
                    {
                        case NftItemServiceErrorCode.NFT_IS_GRINDING_IN_ANOTHER_GAME:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_GRINDING_IN_ANOTHER_GAME,
                                "Continue", "Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    ThetanSDKManager.Instance.OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.ANOTHER_NFT_IS_GRINDING:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_OTHER_NFT_IS_GRINDING,
                                "Continue","Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    ThetanSDKManager.Instance.OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.HERO_MAX_GRIND_STAGE:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_MAX_GRIND_STAGE,
                                "Continue","Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    ThetanSDKManager.Instance.OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.NFT_DAILY_LIMIT_REACH:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_DAILY_LIMIT,
                                "Continue","Select another nft",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    ThetanSDKManager.Instance.OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.NOT_SELECTED_NFT_HERO:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NOT_SELECT_NFT,
                                "Continue", "Select NFT",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    ThetanSDKManager.Instance.OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.NFT_NOT_MINT:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_NOT_MINT,
                                "Continue", "Select NFT",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    ThetanSDKManager.Instance.OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        case NftItemServiceErrorCode.USER_NOT_OWN_NFT:
                            _uiHelperContainer.ShowPopUpMsg("Error", NFTServiceErrorMsg.ERROR_PREPARE_MATCH_NFT_NOT_OWNED,
                                "Continue", "Select other NFT",
                                () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code),
                                () =>
                                {
                                    ThetanSDKManager.Instance.OpenScreenSelectNFT();
                                    onErrorCallback?.Invoke(error);
                                });
                            break;
                        default:
                            _uiHelperContainer.ShowPopUpMsg("Error", 
                                string.Format(NFTServiceErrorMsg.ERROR_PREPARE_MATCH_UNKNOWN_ERROR, error.Code, error.Message),
                                "Go back", "Continue",
                                () =>
                                {
                                    onErrorCallback?.Invoke(error);
                                }, () => onSuccessCallback?.Invoke((NftItemServiceErrorCode)error.Code));
                            break;
                    }

                    HandleAnalyticErrorPrepareBattle(error, true);
                });
        }

        
        private void HandleAnalyticErrorPrepareBattle(WolffunResponseError error, bool isShowPopup)
        {
            var errorType = (NftItemServiceErrorCode)error.Code;

            if (errorType == NftItemServiceErrorCode.NFT_DAILY_LIMIT_REACH)
                return;
            
            _analyticService.LogErrorOccured("Prepare Battle", "Prepare Battle", isShowPopup, 
                errorType == NftItemServiceErrorCode.UNKNOWN ? error.DevDebugMessage : error.Message);
        }
    }
}