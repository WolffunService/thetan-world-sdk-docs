using UnityEditor;
using UnityEngine;
using Wolffun.RestAPI;

namespace Wolffun.RestAPI
{
    /// <summary>
    /// Network config used by client application to config SDK network behavior
    /// </summary>
    [CreateAssetMenu(fileName = "ThetanSDKNetworkConfig", menuName = "ThetanSDK/ThetanSDKNetworkConfig")]
    public class ThetanNetworkConfig : ScriptableObject
    {
        [Header("Staging")]
        [SerializeField] private string _applicationID_Staging;
        [SerializeField] private string _applicationSecret_Staging;
        
        [Header("Prod")]
        [SerializeField] private string _applicationID_Prod;
        [SerializeField] private string _applicationSecret_Prod;
        
        [Header("LogLevel")]
        [SerializeField] private LogLevel _logLevel = LogLevel.None;

        [Header("Deeplink")]
        [Tooltip("DeepLink to this app, example: testsdk://mygame. https://docs.unity3d.com/Manual/deep-linking-android.html")]
        [HideInInspector, SerializeField] private string _androidDeepLinkUrl;
        [Tooltip("DeepLink to this app, example: testsdk://mygame. https://docs.unity3d.com/Manual/deep-linking-ios.html")]
        [HideInInspector, SerializeField] private string _iosDeepLinkUrl;

        [Header("Developer mode (Optional)")]
        [SerializeField] private bool useCustomEndpoint;
        [SerializeField] private StructWolffunEndpointSetting _customEndpointSetting =
            StructWolffunEndpointSetting.GetDefaultEndpointSetting();
        

        public string ApplicationID
        {
            get
            {
#if STAGING || BPF
                return _applicationID_Staging;
#else
                return NetworkClient.IsUseTemporaryVersion ? _applicationID_Staging : _applicationID_Prod;
#endif
            }
        }
        public string ApplicationSecret 
        {
            get
            {
#if STAGING || BPF
                return _applicationSecret_Staging;
#else
                return NetworkClient.IsUseTemporaryVersion ? _applicationID_Staging : _applicationSecret_Prod;
#endif
            }
        }
        public LogLevel LogLevel => _logLevel;

        public string DeepLinkUrl
        {
            get
            {
#if UNITY_ANDROID
                return _androidDeepLinkUrl;
#elif UNITY_IOS
                return _iosDeepLinkUrl;
#else
                return string.Empty;
#endif
            }
        }
        
        public bool IsUseCustomEndpoint => useCustomEndpoint;
        public StructWolffunEndpointSetting CustomEndpointSetting => _customEndpointSetting;

        [ContextMenu("ResetCustomEndpoint")]
        private void ResetCustomEndpoint()
        {
            _customEndpointSetting = StructWolffunEndpointSetting.GetDefaultEndpointSetting();
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }
}