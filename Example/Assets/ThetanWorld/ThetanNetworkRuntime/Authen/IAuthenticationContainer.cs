
using Cysharp.Threading.Tasks;

namespace Wolffun.RestAPI.ThetanAuth
{
    public interface IAuthenticationContainer
    {
        public UniTask LoadCachedAccessToken();
        public void SaveAccessTokenToCache(string accessToken, string refreshToken);
        public string GetRefreshToken();
        public string GetAccessToken();
        public string GetAdminAccessToken();
        public string GetSecretAPIKey();
        public string GetAppClientId();
        public string GetAppClientSecret();
        public void SetAppClientIdAndSecret(string appClientID, string appClientSecret);

        public string GetAppCheckToken();
        public void SetAppCheckToken(string appCheckToken);

    }
}
