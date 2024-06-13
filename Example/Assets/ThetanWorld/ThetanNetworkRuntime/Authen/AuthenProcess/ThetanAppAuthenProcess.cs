using System;
using BestHTTP;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.WebSocket;
using BestHTTP.WebSocket.Extensions;
using BestHTTP.WebSocket.Frames;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Wolffun.Log;

namespace Wolffun.RestAPI.ThetanAuth
{
    
    internal struct ThetanAppAuthenCmdRequest
    {
        public int cmd;
        public ThetanAppAuthenData data;
    }

    internal struct ThetanAppAuthenData
    {
        public string appName;
    }
    
    internal struct ThetanAppAuthenCmdResponse
    {
        public int cmd;
        public object data;
    }

    internal struct ThetanAppLoginCodeData
    {
        public DateTime expiredAt;
        public string link;
        public string loginCode;

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(loginCode) ||
                string.IsNullOrEmpty(link))
            {
                return false;
            }

            if (expiredAt < DateTime.UtcNow)
            {
                Debug.LogError("GEAGSA");
                return false;
            }

            return true;
        }
    }
    
    /// <summary>
    /// Authen process used for login with thetan app, login by qr code on thetan app
    /// </summary>
    public class ThetanAppAuthenProcess : MonoBehaviorAuthenProcess
    {
        private const string URL_LOGIN_FORMAT = "thetanmarket://logincode?code={0}&source={1}&name={2}";

        private WebSocket _websocket;
        private NetworkClient _networkClient;
        private Action _onSuccessCallback;
        private Action<string> _onSuccessCallbackWithUrlQR;
        private Action<string> _onRefreshCallbackWithUrl;
        private Action<WolffunResponseError> _onErrorCallback;

        private UniTaskCompletionSource<ThetanAppLoginCodeData> waitLoginCodeDataSource;

        private ThetanAppLoginCodeData _currentLoginCodeData;
        private bool _isGettingLoginCode;
        
        private int countErrorTotal = 0;
        private float _countTime;
        
        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearCache()
        {
            countErrorTotal = 0;
            _networkClient = null;
            _onSuccessCallback = null;
            _onErrorCallback = null;
            waitLoginCodeDataSource = null;
            _currentLoginCodeData = new ThetanAppLoginCodeData();
            _isGettingLoginCode = false;
            if(_websocket != null)
            {
                _websocket.OnClosed = null;
                _websocket.OnError = null;
                _websocket.OnMessage = null;
                _websocket.Close();
                _websocket = null;
            }
            
            CommonLog.Log("ThetanAppAuthenProcess clear cache success");
        }

        public void FixedUpdate()
        {
            if (string.IsNullOrEmpty(_currentLoginCodeData.loginCode))
                return;

            if (_isGettingLoginCode)
                return;

            _countTime += Time.fixedDeltaTime;

            if (_countTime >= 5)
            {
                if (DateTime.UtcNow > _currentLoginCodeData.expiredAt)
                {
                    ConnectWebsocketAndGetLoginCodeData(loginCodeData =>
                    {
                        _onSuccessCallbackWithUrlQR?.Invoke(GenerateQRLoginURL(loginCodeData));
                    }, _onErrorCallback);
                }
                _countTime = 0;
            }
        }

        /// <param name="onSuccessCallback">Callback when ready for login with qr, callback with url to convert to qr code</param>
        /// <param name="onRefreshCallback">Callback when new qr code is generated, callback with url to convert to qr code</param>
        public async void PrepareForLoginWithQRCode(NetworkClient networkClient, Action<string> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback, Action<string> onRefreshCallback, Action onAuthenSuccessCallback)
        {
            _networkClient = networkClient;
            _onSuccessCallbackWithUrlQR = onSuccessCallback;
            _onErrorCallback = onErrorCallback;
            _onRefreshCallbackWithUrl = onRefreshCallback;
            _onSuccessCallback = onAuthenSuccessCallback;

            ConnectWebsocketAndGetLoginCodeData(loginCodeData =>
            {
                onSuccessCallback?.Invoke(GenerateQRLoginURL(loginCodeData));
            }, onErrorCallback);
        }
        
        public async void StartProcessLoginWithThetanApp(NetworkClient networkClient, 
            Action onOpenUrlSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            _networkClient = networkClient;
            if (_currentLoginCodeData.IsValid())
            {
                onOpenUrlSuccessCallback?.Invoke();
                OpenUrlThetanApp(_currentLoginCodeData);
                return;
            }

            if (_isGettingLoginCode)
            {
                var isCanceled = await UniTask.WaitUntil(() => _currentLoginCodeData.IsValid())
                    .TimeoutWithoutException(new TimeSpan(0, 0, 5));

                if (isCanceled)
                {
                    if(!_websocket.IsOpen)
                        onErrorCallback?.Invoke(new WolffunResponseError(-99, "Timeout open connection to server"));
                    else
                        onErrorCallback?.Invoke(new WolffunResponseError(-98, "Timeout waiting login code data"));
                    return;
                }
            }
            else
            {
                ConnectWebsocketAndGetLoginCodeData(loginCodeData =>
                {
                    onOpenUrlSuccessCallback?.Invoke();
                    OpenUrlThetanApp(loginCodeData);
                }, onErrorCallback);
                return;
            }

            if (!_currentLoginCodeData.IsValid())
            {
                onErrorCallback?.Invoke(new WolffunResponseError(-97, "Login code invalid"));
                return;
            }
            
            onOpenUrlSuccessCallback?.Invoke();
            OpenUrlThetanApp(_currentLoginCodeData);
            return;
        }

        private void OnMsgReceived(WebSocket ws, string msg)
        {
            CommonLog.Log($"OnMsgReceived {msg}");
            var cmdResponse = JsonConvert.DeserializeObject<ThetanAppAuthenCmdResponse>(msg);

            if (cmdResponse.cmd == 1) // Open thetan app to login
            {
                ThetanAppLoginCodeData loginCodeData =
                    JsonConvert.DeserializeObject<ThetanAppLoginCodeData>(cmdResponse.data.ToString());

                if (waitLoginCodeDataSource != null)
                    waitLoginCodeDataSource.TrySetResult(loginCodeData);
            }
            else if (cmdResponse.cmd == 3)
            {
                TokenResponseModel token =
                    JsonConvert.DeserializeObject<TokenResponseModel>(cmdResponse.data.ToString());
                
                Debug.Log("Authen Success " + token.accessToken);
                ProcessAuthenSucceed(LoginType.QRCodeOrThetanApp, token.accessToken, token.refreshToken);
                _onSuccessCallback?.Invoke();

                _websocket.OnClosed = null;
                _websocket.OnError = null;
                _websocket.OnMessage = null;
                _websocket.Close();
                _websocket = null;
            }
        }

        private async void ConnectWebsocketAndGetLoginCodeData(Action<ThetanAppLoginCodeData> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if(_websocket != null)
            {
                _websocket.OnClosed = null;
                _websocket.OnError = null;
                _websocket.Close();
                _websocket = null;
            }

            var networkConfig = _networkClient.NetworkConfig;

            _isGettingLoginCode = true;
            _websocket = new WebSocket(new Uri(WolffunUnityHttp.Settings.AuthServiceWsURL), 
                string.Empty, string.Empty,
                new PerMessageCompression(/*compression level: */           BestHTTP.Decompression.Zlib.CompressionLevel.Default,
                                                         /*clientNoContextTakeover: */     false,
                                                         /*serverNoContextTakeover: */     false,
                                                         /*clientMaxWindowBits: */         BestHTTP.Decompression.Zlib.ZlibConstants.WindowBitsMax,
                                                         /*desiredServerMaxWindowBits: */  BestHTTP.Decompression.Zlib.ZlibConstants.WindowBitsMax,
                                                         /*minDatalengthToCompress: */     PerMessageCompression.MinDataLengthToCompressDefault),
                new ThetanSecretKeyPlugins(networkConfig.ApplicationID, networkConfig.ApplicationSecret),
                new UserAgentPlugins());
            _websocket.StartPingThread = true;
            
            _websocket.Open();

            var timeOutOpenWs = Time.time + 5;

            await UniTask.WaitUntil(() => _websocket.IsOpen || Time.time >= timeOutOpenWs);

            if (Time.unscaledTime >= timeOutOpenWs)
            {
                _isGettingLoginCode = false;
                onErrorCallback?.Invoke(new WolffunResponseError(-99, "Timeout open ws"));
                return;
            }
            
            _websocket.OnMessage = OnMsgReceived;
            _websocket.OnClosed = OnWebSocketClosedUnExpectedly;
            _websocket.OnError = OnWSError;
            
            waitLoginCodeDataSource = new UniTaskCompletionSource<ThetanAppLoginCodeData>();
            
            _websocket.Send(JsonConvert.SerializeObject(new ThetanAppAuthenCmdRequest()
            {
                cmd = 1,
                data = new ThetanAppAuthenData()
                {
                    appName = networkConfig.ApplicationID
                }
            }));
            
            ThetanAppLoginCodeData loginCodeData = default;
            try
            {
                loginCodeData = await waitLoginCodeDataSource.Task
                    .Timeout(new TimeSpan(0, 0, 5));
            }
            catch (OperationCanceledException canceledException)
            {
                _isGettingLoginCode = false;
                onErrorCallback?.Invoke(new WolffunResponseError(-98, "Timeout wait login code data"));
                return;
            }
            catch (TimeoutException ex)
            {
                _isGettingLoginCode = false;
                onErrorCallback?.Invoke(new WolffunResponseError(-98, "Timeout wait login code data"));
                return;
            }

            _isGettingLoginCode = false;
            countErrorTotal = 0;
            _currentLoginCodeData = loginCodeData;
            onSuccessCallback?.Invoke(loginCodeData);
        }

        private void OnWSError(WebSocket websocket, string reason)
        {
            CommonLog.LogError($"OnWSError close, regenerate new login url {reason}");
            websocket.OnClosed = null;
            websocket.OnMessage = null;
            websocket.OnError = null;
            websocket.Close();
            
            countErrorTotal++;
            if(countErrorTotal < 3)
            {
                _currentLoginCodeData = new ThetanAppLoginCodeData();
                ConnectWebsocketAndGetLoginCodeData(loginCodeData =>
                {
                    _onSuccessCallbackWithUrlQR?.Invoke(GenerateQRLoginURL(loginCodeData));
                }, _onErrorCallback);
            }
            else
            {
                countErrorTotal = 0;
                _currentLoginCodeData = new ThetanAppLoginCodeData();
                _onErrorCallback?.Invoke(new WolffunResponseError(-99, reason));
            }
        }

        private void OnWebSocketClosedUnExpectedly(WebSocket webSocket, UInt16 code, string message)
        {
            CommonLog.Log("OnWebSocketClosedUnExpectedly, regenerate new login url");
            _currentLoginCodeData = new ThetanAppLoginCodeData();
            
            ConnectWebsocketAndGetLoginCodeData(loginCodeData =>
            {
                _onSuccessCallbackWithUrlQR?.Invoke(GenerateQRLoginURL(loginCodeData));
            }, _onErrorCallback);
        }

        private string GenerateQRLoginURL(ThetanAppLoginCodeData loginCodeData)
        {
            return string.Format(URL_LOGIN_FORMAT,
                loginCodeData.loginCode,
                string.Empty,
                UnityWebRequest.EscapeURL(Application.productName));
        }

        private void OpenUrlThetanApp(ThetanAppLoginCodeData loginCodeData)
        {
            var url = string.Format(URL_LOGIN_FORMAT,
                loginCodeData.loginCode,
                _networkClient.NetworkConfig.DeepLinkUrl,
                UnityWebRequest.EscapeURL(Application.productName));
            Debug.Log($"OpenUrlThetanApp {url}");
            Application.OpenURL(url);
        }
    }
}