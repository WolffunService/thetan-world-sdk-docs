using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK
{
    public class SDKAuthenContainer : MonoBehaviour, IAuthenticationContainer
    {
        private const string filename = "ThetanSDKAuthenCached";
        
        [NonSerialized] private string _accessToken;
        [NonSerialized] private string _refreshToken;
        [NonSerialized] private LocalDataLoadSaver<TokenResponseModel> _localDataLoadSaver;
        [NonSerialized] private string _applicationId;
        [NonSerialized] private string _applicationSecret;
        [NonSerialized] private string _appCheckToken;
        
        public async UniTask LoadCachedAccessToken()
        {
            var cachedData = _localDataLoadSaver.LoadDataLocal(filename);

            _accessToken = cachedData.accessToken;
            _refreshToken = cachedData.refreshToken;
        }

        public void SaveAccessTokenToCache(string accessToken, string refreshToken)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _localDataLoadSaver.SaveDataLocal(new TokenResponseModel()
            {
                accessToken = accessToken,
                refreshToken = refreshToken
            }, filename);
        }

        public string GetRefreshToken() => _refreshToken;

        public string GetAccessToken() => _accessToken;

        public string GetAdminAccessToken() => string.Empty;

        public string GetSecretAPIKey() => string.Empty;

        public string GetAppClientId() => _applicationId;

        public string GetAppClientSecret() => _applicationSecret;

        public void SetAppClientIdAndSecret(string appClientID, string appClientSecret)
        {
            _applicationId = appClientID;
            _applicationSecret = appClientSecret;
        }

        public string GetAppCheckToken() => _appCheckToken;
        public void SetAppCheckToken(string appCheckToken) => _appCheckToken = appCheckToken;
    }
}