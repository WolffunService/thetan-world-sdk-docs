using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.UserStatisticService
{
    public class UserStatisticService : BaseClassService
    {
        private NetworkClient _networkClient;
        
        private Nullable<UserStatisticData> _userStatisticData;
        public Nullable<UserStatisticData> UserStatisticData => _userStatisticData;
        public Action<UserStatisticData> OnChangeUserStatisticDataCallback;

        private Nullable<LeaderboardData> _leaderboardData;
        public Nullable<LeaderboardData> LeaderboardData => _leaderboardData;
        public Action<LeaderboardData> OnChangeLeaderboardDataCallback;

        private bool _isFetchingStatisticData;
        private bool _isFetchingLeaderboard;
        private bool _isInitialized;

        public void InitService(NetworkClient networkClient)
        {
            _networkClient = networkClient;

            _userStatisticData = null;
            
            networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);

            if(networkClient.NetworkClientState == ThetanNetworkClientState.LoggedIn)
            {
                FetchUserStatisticData(null, null);
                FetchUserLeaderboardData(null, null);
            }
            networkClient.SubcribeOnReAuthenCallback(HandleReAuthen);

            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized ||
                _networkClient == null ||
                _networkClient.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;

            UpdateUserStatisticData();
            UpdateLeaderboardData();
        }

        private void UpdateUserStatisticData()
        {
            if (_isFetchingStatisticData)
                return;
            
            if (_userStatisticData == null)
            {
                FetchUserStatisticData(null, null);
            }
        }

        private void UpdateLeaderboardData()
        {
            if (_isFetchingLeaderboard)
                return;
            
            if (_leaderboardData == null)
            {
                FetchUserLeaderboardData(null, null);
                return;
            }
        }

        private void HandleOnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.LoggedIn)
            {
                FetchUserStatisticData(null, null);
            }
            else if(newState == ThetanNetworkClientState.NotLoggedIn ||
                    newState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                ClearDataService();
            }
        }

        private void HandleReAuthen()
        {
            ClearDataService();
            FetchUserStatisticData(null, null);
        }

        public void FetchUserStatisticData(Action<UserStatisticData> successCallback, Action<WolffunResponseError> errorCallback)
        {
            _isFetchingStatisticData = true;
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/user/daily-stat")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI<UserStatisticData>(reqCommon,
                data =>
                {
                    _userStatisticData = data;
                    OnChangeUserStatisticDataCallback?.Invoke(_userStatisticData.Value);
                    successCallback?.Invoke(_userStatisticData.Value);
                    _isFetchingStatisticData = false;
                }, error =>
                {
                    _userStatisticData = new UserStatisticData();
                    errorCallback?.Invoke(error);
                    _isFetchingStatisticData = false;
                }, AuthType.TOKEN);
            
           
            
            
            
        }

        public void FetchUserLeaderboardData(Action<LeaderboardData> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            _isFetchingLeaderboard = true;
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/leaderboards/sdk")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI<LeaderboardData>(reqCommon,
                data =>
                {
                    _isFetchingLeaderboard = false;
                    _leaderboardData = data;
                    successCallback?.Invoke(data);
                    OnChangeLeaderboardDataCallback?.Invoke(data);
                }, error =>
                {
                    _leaderboardData = new LeaderboardData();
                    _isFetchingLeaderboard = false;
                    errorCallback?.Invoke(error);
                }, AuthType.TOKEN);
        }

        public override void ClearDataService()
        {
            _userStatisticData = null;
        }
    }
}