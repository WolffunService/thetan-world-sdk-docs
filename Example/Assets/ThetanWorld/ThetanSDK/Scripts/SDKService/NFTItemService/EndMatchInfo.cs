using System;
using Newtonsoft.Json;

namespace ThetanSDK.SDKServices.NFTItem
{
    public enum MatchResult
    {
        Lose = -1,
        Draw = 0,
        Win = 1,
    }
    
    [Serializable]
    public struct EndMatchInfo
    {
        [JsonProperty("battleResult")]
        public MatchResult matchResult;
        
        /// <summary>
        /// Current level for this match. ex: level 1, 2, 4, ...
        /// Used for level-based game like Candy crush's level, puzzle game's level, ...
        /// </summary>
        public uint gameLevel;
    }
}