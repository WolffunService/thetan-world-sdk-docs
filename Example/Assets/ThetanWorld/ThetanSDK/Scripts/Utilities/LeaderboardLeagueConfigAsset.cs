using ThetanSDK.SDKService.UserStatisticService;
using UnityEngine;

namespace ThetanSDK.Utilities
{
    [System.Serializable]
    public struct LeaderboardLocalConfig
    {
        public LeagueLeaderboard league;
        public Sprite icon;
        public string name;
    }
    
    [CreateAssetMenu(fileName = "LeaderboardLeagueConfigAsset", menuName = "Wolffun/Configs/LeaderboardLeagueConfigAsset")]
    public class LeaderboardLeagueConfigAsset : ScriptableObject
    {
        [SerializeField] private LeaderboardLocalConfig[] _listConfig;

        public LeaderboardLocalConfig GetConfig(LeagueLeaderboard league)
        {
            foreach (var config in _listConfig)
            {
                if (config.league == league)
                    return config;
            }

            if (_listConfig.Length > 0)
                return _listConfig[0];
            else
                return default;
        }
    }
}