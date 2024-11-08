﻿using System;
using ThetanSDK.SDKServices.NFTItem;

namespace ThetanSDK.SDKServices.Analytic
{
    internal partial class SDKAnalyticService
    {
        internal enum BattleFlowStep
        {
            PrepareBattle,
            StartGrind,
            PauseGrind,
            EndBattle
        }
        
        /// <summary>
        /// Log analytic grinding flow for SDK
        /// </summary>
        /// <param name="timeBattle">only available when step is EndBattle</param>
        public async void LogBattleFlow(string grindingHeroNftId, string grindingSessionId, BattleFlowStep step, int timeBattle = -1,
            MatchResult matchResult = MatchResult.Draw)
        {
            try
            {
                _dataLog.Clear();

                if (string.IsNullOrEmpty(grindingHeroNftId))
                    return;

                var profileService = ThetanSDKManager.Instance.ProfileService;

                _dataLog["grind_id"] = grindingSessionId;
                _dataLog["user_id"] = profileService.UserId;
                _dataLog["client_id"] = _networkConfig.ApplicationID;

                (var onChainInfo, var nftIngameInfo) = await GetAndCacheNFTIngameInfo(grindingHeroNftId);

                _dataLog["hero_id"] = grindingHeroNftId;
                _dataLog["hero_token_id"] = onChainInfo.tokenId;
                _dataLog["hero_rarity"] = nftIngameInfo.rarity.ToString();
                _dataLog["hero_skin_id"] = nftIngameInfo.type.ToString();
                _dataLog["game_version"] = ThetanSDKManager.Instance.Version;
                _dataLog["time_battle"] = timeBattle.ToString();
                _dataLog["trigger_at"] = step.ToString();
                _dataLog["battle_result"] = ((int)matchResult).ToString();

                LogEvent("tw_sdk_battle_flow", _dataLog);
            }
            catch (Exception ex)
            {

            }

        }
    }
}