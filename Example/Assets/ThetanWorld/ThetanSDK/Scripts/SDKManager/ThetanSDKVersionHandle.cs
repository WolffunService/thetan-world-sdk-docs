using Cysharp.Threading.Tasks;
using UnityEngine;
using Wolffun.RestAPI;

namespace ThetanSDK.VersionCheckService
{
    internal enum VersionSupportedStatus
    {
        Unknown = 0, // Cannot determine if current sdk version is supported or not
        Supported = 1, // Current sdk version is supported
        Unsupported = 2 // Current sdk version is not supported
    }
    
    // This class will determine if current sdk version is supported.
    // If it fail to determine supported status, it will retry until it can determine supported status
    internal class ThetanSDKVersionHandle : MonoBehaviour
    {
        private const int RECHECK_SDK_VERSION_INTERVAL_SECOND = 5;
        
        private VersionSupportedStatus _versionSupportedStatus;
        public VersionSupportedStatus VersionSupportedStatus => _versionSupportedStatus;

        private bool _isInitialized = false;
        private NetworkClient _networkClient;
        private float _countTimeUpdateRecheckStatus;
        
        public async UniTask<VersionSupportedStatus> InitializeVersionHandle(NetworkClient networkClient)
        {
            _versionSupportedStatus = await CheckVersion();
            _isInitialized = true;
            _networkClient = networkClient;

            if (_versionSupportedStatus != VersionSupportedStatus.Unknown)
            {
                enabled = false;
            }
            else
            {
                _countTimeUpdateRecheckStatus = RECHECK_SDK_VERSION_INTERVAL_SECOND;
            }
            
            return _versionSupportedStatus;
        }


        private void Update()
        {
            if (!_isInitialized)
                return;

            if (_versionSupportedStatus != VersionSupportedStatus.Unknown)
            {
                enabled = false;
                return;
            }

            _countTimeUpdateRecheckStatus -= Time.deltaTime;
            if (_countTimeUpdateRecheckStatus > 0)
                return;

            _countTimeUpdateRecheckStatus = RECHECK_SDK_VERSION_INTERVAL_SECOND;
            if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork ||
                _networkClient.NetworkClientState == ThetanNetworkClientState.NotLoggedInNoNetwork)
                return;

            RecheckVersion().Forget();
        }


        private async UniTaskVoid RecheckVersion()
        {
            _versionSupportedStatus = await CheckVersion();

            if (_versionSupportedStatus != VersionSupportedStatus.Unknown)
            {
                enabled = false;
            }
        }

        private UniTask<VersionSupportedStatus> CheckVersion()
        {
            UniTaskCompletionSource<VersionSupportedStatus> checkVersionCompletionSource = new UniTaskCompletionSource<VersionSupportedStatus>();
            
            WolffunRequestCommon req = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/partner/app/config")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI<VersionDataModel>(req, versionDataModel =>
            {
                if (versionDataModel.supportedVersions == null ||
                    versionDataModel.supportedVersions.Length == 0)
                {
                    checkVersionCompletionSource.TrySetResult(VersionSupportedStatus.Unsupported);
                    return;
                }

                var version = ThetanSDKManager.Instance.Version;
                
#if STAGING
                version = version.Replace("_S", string.Empty);
#endif

                foreach (var supportedVersion in versionDataModel.supportedVersions)
                {
                    if (version == supportedVersion)
                    {
                        checkVersionCompletionSource?.TrySetResult(VersionSupportedStatus.Supported);
                        return;
                    }
                }
                
                checkVersionCompletionSource?.TrySetResult(VersionSupportedStatus.Unsupported);

            }, error =>
            {
                if ((WSErrorCode)error.Code == WSErrorCode.DoNotHavePermission)
                    checkVersionCompletionSource.TrySetResult(VersionSupportedStatus.Unknown);
                else if ((WSErrorCode)error.Code == WSErrorCode.UnityHttpRequestNetworkError)
                    checkVersionCompletionSource.TrySetResult(VersionSupportedStatus.Unknown);
                else 
                    checkVersionCompletionSource.TrySetResult(VersionSupportedStatus.Unsupported);
            }, AuthType.TOKEN);

            return checkVersionCompletionSource.Task;
        }
    }
}