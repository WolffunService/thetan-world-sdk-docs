using System;

namespace ThetanSDK.SDKServices.Analytic
{
    internal partial class SDKAnalyticService
    {
        public async void LogErrorOccured(string screen, string source, bool isShowPopup, string description)
        {
            try
            {
                _dataLog.Clear();

                var profileService = ThetanSDKManager.Instance.ProfileService;
                var nftItemService = ThetanSDKManager.Instance.NftItemService;
            
                _dataLog["user_id"] = profileService.UserId;
                _dataLog["client_id"] = _networkConfig.ApplicationID;
                _dataLog["game_version"] = ThetanSDKManager.Instance.Version;

                bool isGrinding = !string.IsNullOrEmpty(nftItemService.GrindingHeroNftId);

                _dataLog["is_grinding"] = isGrinding.ToString();

                if (isGrinding)
                {
                    (var onChainInfo, var nftIngameInfo) = await GetAndCacheNFTIngameInfo(nftItemService.GrindingHeroNftId);
                
                    _dataLog["hero_id"] = nftItemService.GrindingHeroNftId;
                    _dataLog["hero_token_id"] = onChainInfo.tokenId;
                    _dataLog["hero_rarity"] = nftIngameInfo.rarity.ToString();
                    _dataLog["hero_skin_id"] = nftIngameInfo.type.ToString();
                }

                _dataLog["screen"] = screen;
                _dataLog["source"] = source;
                _dataLog["is_show_popup"] = isShowPopup.ToString();
                _dataLog["description"] = description;
            
                LogEvent("tw_sdk_error", _dataLog);
            }
            catch (Exception e)
            {
            }
            
        }
    }
}