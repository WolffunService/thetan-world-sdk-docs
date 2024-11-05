using Cysharp.Threading.Tasks;
using System;
using System.Text;
using BestHTTP;
using Cysharp.Threading.Tasks.Triggers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using Wolffun.MultiPlayer;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI
{
    public class WolffunUnityHttp : MonoSingleton<WolffunUnityHttp>
    {
        public static string EnvironmentName
        {
            get
            {
#if STAGING
                return "STAGING";
#elif UAT
                return "UAT";
#else
                return "PRODUCTION";
#endif
                
            }
        }
        
        private IWolffunHandleSpecialError _handleSpecialError;

        private IAuthenticationContainer _authenticationContainer;
        private WolffunHttpLog _httpLog;
        
        private bool _isShowingSpecialDialog = false;

        private bool _isInitialized = false;

        private IWolffunEndpointSetting _endpointSetting;

        private ITokenErrorAPIHandle _tokenErrorAPIHandle;

        private string _generatedUserAgentString = string.Empty;

        public string GeneratedUserAgentStringString => _generatedUserAgentString;
        
        public static IWolffunEndpointSetting Settings => Instance._endpointSetting;

        /// <summary>
        /// Use for specific case when you need handle special wolffun error code.
        /// You do not need this for general usage. 
        /// DO NOT use this if you do not know what you are doing.
        /// </summary>
        public static IWolffunHandleSpecialError HandleErrorSo => Instance._handleSpecialError;

        public void Initialize(string version, IAuthenticationContainer authenticationContainer, IWolffunEndpointSetting endpointSetting, LogLevel logLevel = LogLevel.Error, 
            IWolffunHandleSpecialError handleSpecialError = null, ITokenErrorAPIHandle tokenErrorAPIHandle = null)
        {
            _authenticationContainer = authenticationContainer;
            _endpointSetting = endpointSetting;
            _httpLog = new WolffunHttpLog(logLevel);
            _handleSpecialError = handleSpecialError;
            _tokenErrorAPIHandle = tokenErrorAPIHandle;

            _generatedUserAgentString = GenerateUserAgentString(version);
            
            BestHTTP.HTTPManager.UserAgent = _generatedUserAgentString;
            
            _isInitialized = true;
        }

        private string GenerateUserAgentString(string version)
        {
            var appClientId = _authenticationContainer.GetAppClientId();
            return $"Unity/{Application.unityVersion} ({Application.platform}; {SystemInfo.deviceModel}; {SystemInfo.operatingSystem}; {appClientId}; {version})";
        }

        public static JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };
        
        /// <summary>
        /// If you are using method make api with JsonConverter,
        /// create JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        ///  {
        ///     Converters = { jsonConverter },
        ///     NullValueHandling = NullValueHandling.Ignore,
        ///   }; 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <param name="authType"></param>
        /// <param name="pageData"></param>
        /// <param name="jsonSerializerSettings"></param>
        /// <typeparam name="T"></typeparam>
        public async void MakeAPI<T>(WolffunRequestCommon request, 
            Action<T> result, 
            Action<WolffunResponseError> error,
            AuthType authType = AuthType.NONE, 
            Action<Page> pageData = null, 
            JsonSerializerSettings jsonSerializerSettings = null)
        {
            try
            {
                if (!_isInitialized)
                    await UniTask.WaitUntil(() => _isInitialized);
                
                if(_authenticationContainer == null)
                {
                    Debug.LogError(
                        "Authentication container is null, please configure authentication container before all api");
                    return;
                }

                AddAuthenticationHeader(request, authType);
        
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept-Encoding", "gzip, identity");

                var appCheckToken = _authenticationContainer.GetAppCheckToken();
                if (request.IsRequireAppCheck() && !string.IsNullOrEmpty(appCheckToken))
                {
                    request.AddHeader("X-Firebase-AppCheck", appCheckToken);
                }
                
                _httpLog.Log(LogLevel.Info, $"MakeAPI {request.Url()}");
                _httpLog.Log(LogLevel.Info, $"MakeAPI {JsonConvert.SerializeObject(request.Headers())}");
                //Send request
                WolffunHttp.SendAPI(request, _httpLog, resultAPI =>
                {
                    if (resultAPI.IsOk())
                    {
                        if (_httpLog.CheckCanShowLog(LogLevel.Info))
                            _httpLog.Log(LogLevel.Info,
                                $"Call Api {request.URL} success with response {resultAPI.ToString()}");
                        
                        var res = resultAPI.To<T>(jsonSerializerSettings != null
                            ? jsonSerializerSettings
                            : DefaultJsonSerializerSettings);
                        
                        if (res.Success)
                        {
                            try
                            {
                                result?.Invoke(res.Data);
                            }
                            catch (Exception e)
                            {
                                _httpLog.Log(LogLevel.Error, $"Handle success callback with exception {e.Message} {e.StackTrace}");
                            }

                            try
                            {
                                pageData?.Invoke(res.Paging);
                            }
                            catch (Exception e)
                            {
                                _httpLog.Log(LogLevel.Error, $"Handle pageData callback with exception {e.Message}");
                            }
                        }
                        else OnError(error, resultAPI, request.Url());
                    }
                    else
                    {
                        _httpLog.Log(LogLevel.Error, $"MakeAPI Error {request.Url()}: {resultAPI.ToString()}");
                        OnError(error, resultAPI, request.Url());
                    }
                }, (errorData) =>
                {
                    if (_httpLog.CheckCanShowLog(LogLevel.Error))
                        _httpLog.Log(LogLevel.Error, 
                            $"MakeAPI Error {request.URL}: {errorData.ToString()}");
                    error?.Invoke(errorData);
                });
            }
            catch (Exception ex)
            {
                _httpLog.Log(LogLevel.Error,
                    $"MakeAPI {request.Url()} throw exception {ex.Message} - stack trace {ex.StackTrace}");
                error?.Invoke(new WolffunResponseError("Exception making request"));
            }
        }
        
        [Obsolete("This method is deprecated. Please use the MakeAPI with JsonSerializerSettings instead.")]
        public async UniTask<APIResponseWrapper<T>> MakeAPITask<T>(WolffunRequestCommon request,
            AuthType authType = AuthType.NONE, Action<WolffunResponseError> error = null, Action<Page> pageData = null, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var result = default(APIResponseWrapper<T>);
            try
            {
                
                if (!_isInitialized)
                    await UniTask.WaitUntil(() => _isInitialized);
                
                if(_authenticationContainer == null)
                {
                    Debug.LogError(
                        "Authentication container is null, please configure authentication container before all api");
                    return new APIResponseWrapper<T>()
                    {
                        Data = default(T),
                        IsSuccess = false,
                        Error = new WolffunResponseError(-99, "Authentication container is null, please configure authentication container before all api")
                    };
                }
                
                AddAuthenticationHeader(request, authType);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept-Encoding", "gzip, identity");

                var response = await WolffunHttp.SendAPI<T>(request, _httpLog);

                _httpLog.Log(LogLevel.Info, $"MakeAPITask {request.URL} response = {response.ResponseCommon}");
                if (response.IsSuccess)
                {
                    var res = response.ResponseCommon.To<T>(jsonSerializerSettings != null? jsonSerializerSettings: DefaultJsonSerializerSettings);
                    if (res.Success)
                    {
                        pageData?.Invoke(res.Paging);
                        return new APIResponseWrapper<T> { Data = res.Data, IsSuccess = true };
                    }
                }

                OnError(error, response.ResponseCommon, request.Url());
                return new APIResponseWrapper<T> { Data = default, IsSuccess = false, Error = response.ResponseError };
            }
            catch (Exception ex)
            {
                _httpLog.Log(LogLevel.Error,
                    $"MakeAPITask {request.URL} throw exception {ex.Message} - stack trace {ex.StackTrace}");
                return new APIResponseWrapper<T> { Data = default, IsSuccess = false, Error = new WolffunResponseError("Exception making request") };
            }
        }

        [Obsolete("This method is deprecated. Please use the MakeAPI with JsonSerializerSettings instead.")]
        public async UniTask<APIResponseWrapper<T>> MakeAPITaskSimple<T>(WolffunRequestCommon request,
            string accessToken)
        {
            var result = default(APIResponseWrapper<T>);
            try
            {
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("Content-Type", "application/json");
                var response = await WolffunHttp.SendAPI<T>(request, _httpLog);
                _httpLog.Log(LogLevel.Info, $"MakeAPITask {request.URL} response = {response.ToString()}");
                if (response.IsSuccess)
                {
                    var res = response.ResponseCommon.To<T>(DefaultJsonSerializerSettings);
                    if (res.Success)
                        return new APIResponseWrapper<T> { Data = res.Data, IsSuccess = true };
                }

                return new APIResponseWrapper<T> { Data = default, IsSuccess = false, Error = response.ResponseError };
            }
            catch (Exception ex)
            {
                _httpLog.Log(LogLevel.Error,
                    $"MakeAPI {request.URL} throw exception {ex.Message} - stack trace {ex.StackTrace}");
                return new APIResponseWrapper<T> { Data = default, IsSuccess = false, Error = new WolffunResponseError("Exception making request") };
            }
        }

        private void AddAuthenticationHeader(WolffunRequestCommon request, AuthType authType)
        {
            var accessToken = _authenticationContainer.GetAccessToken();
            
            switch (authType)
            {
                case AuthType.TOKEN when !string.IsNullOrEmpty(accessToken):
                    request.AddHeader("Authorization", "Bearer " + accessToken);
                    break;
                case AuthType.ADMIN when !string.IsNullOrEmpty(_authenticationContainer.GetAdminAccessToken()):
                    request.AddHeader("Authorization", "Bearer " + _authenticationContainer.GetAdminAccessToken());
                    break;
                case AuthType.SECRET_API_KEY when !string.IsNullOrEmpty(_authenticationContainer.GetSecretAPIKey()):
                    request.AddHeader("X-API-KEY", _authenticationContainer.GetSecretAPIKey());
                    break;
                case AuthType.TOKEN_AND_CLIENT_SECRET:
                {
                    if(!string.IsNullOrEmpty(accessToken)) 
                        request.AddHeader("Authorization", "Bearer " + accessToken);
                    
                    if(!string.IsNullOrEmpty(_authenticationContainer.GetAppClientId()) &&
                       !string.IsNullOrEmpty(_authenticationContainer.GetAppClientSecret()))
                    {
                        var appClientId = _authenticationContainer.GetAppClientId();
                        var appClientSecret = _authenticationContainer.GetAppClientSecret();
                        request.AddHeader("X-ThetanSecretKey", GenerateThetanSecretKey(appClientId, appClientSecret));
                    }
                    
                    break;
                }
            }
        }

        internal static string GenerateThetanSecretKey(string appClientId, string appClientSecret)
        {
            var thetanSecretKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appClientId}:{appClientSecret}"));
            return $"Basic {thetanSecretKey}";
        }
        
        private async void OnError(Action<WolffunResponseError> error, WolffunResponseCommon resultAPI, string endpoint = "")
        {
            try
            {
                _httpLog.Log(LogLevel.Error, $"Call Api {endpoint} has error {resultAPI.ToString()}");
                WolffunResponseError err = new WolffunResponseError(0);

                try
                {
                    err = resultAPI.Error();
                }
                catch (Exception e)
                {
                    _httpLog.Log(LogLevel.Error, $"Error when parse {endpoint} response with exception {e.Message}");
                }
                
                //process when the error is from the server status, not from the code point out b4 hand
                if (!resultAPI.IsOk())
                {
                    //server maintain
                    if (resultAPI.Status() == 503)
                    {
                        if (_handleSpecialError != null)
                        {
                            if (_isShowingSpecialDialog)
                            {
                                error?.Invoke(new WolffunResponseError((int)WSErrorCode.AlreadyResolveFromBase,
                                    resultAPI.Body()));
                                return;
                            }

                            _isShowingSpecialDialog = true;
                            _handleSpecialError.HandleMaintain((wsErrorCode) =>
                            {
                                _isShowingSpecialDialog = false;
                            });
                            error?.Invoke(new WolffunResponseError((int)WSErrorCode.AlreadyResolveFromBase,
                                resultAPI.Body()));
                            return;
                        }
                        else
                        {
                            error?.Invoke(new WolffunResponseError((int)resultAPI.Status(), resultAPI.Body()));
                            return;
                        }
                        
                    }
                    else if (resultAPI.Status() == 403 && (WSErrorCode)err.Code != WSErrorCode.UserBanned)
                    {
                        error?.Invoke(new WolffunResponseError((int)WSErrorCode.DoNotHavePermission, resultAPI.Body()));
                        return;
                    }
                    else if (resultAPI.Status() >= 500)
                    {
                        error?.Invoke(new WolffunResponseError((int)WSErrorCode.ServerStatus500, resultAPI.Body()));
                        return;
                    }
                }

                switch ((WSErrorCode)err.Code)
                {
                    case WSErrorCode.UserBanned:
                        if (_tokenErrorAPIHandle != null)
                        {
                            try
                            {
                                _tokenErrorAPIHandle.HandleAccountBanned();
                            }
                            catch (Exception ex)
                            {
                                _httpLog.Log(LogLevel.Error, $"TokenErrorAPIHandle handle account banned throw exception {ex.Message}");
                            }
                        }
                        break;
                    case WSErrorCode.TokenInvalid:
                        if(_tokenErrorAPIHandle != null)
                        {
                            try
                            {
                                _tokenErrorAPIHandle.HandleTokenError();
                            }
                            catch (Exception ex)
                            {
                                _httpLog.Log(LogLevel.Error, $"TokenErrorAPIHandle handle token invalid throw exception {ex.Message}");
                            }
                        }
                        break;
                    case WSErrorCode.TokenExpired:
                        if(_tokenErrorAPIHandle != null)
                        {
                            try
                            {
                                _tokenErrorAPIHandle.HandleTokenExpire();
                            }
                            catch (Exception ex)
                            {
                                _httpLog.Log(LogLevel.Error, $"TokenErrorAPIHandle handle token expire throw exception {ex.Message}");
                            }
                        }
                        break;
                    case WSErrorCode.GameMaintainServer:
                        if (_handleSpecialError != null)
                        {
                            _isShowingSpecialDialog = true;
                            _handleSpecialError.HandleMaintain((wsErrorCode) =>
                            {
                                _isShowingSpecialDialog = false;
                            });
                            error?.Invoke(new WolffunResponseError((int)WSErrorCode.AlreadyResolveFromBase,
                                resultAPI.Body()));
                            return;    
                        }
                        break;
                    case WSErrorCode.NewVersionAvailable:
                        if (_handleSpecialError != null)
                        {
                            _isShowingSpecialDialog = true;
                            _handleSpecialError.HandleNewVersion((wsErrorCode) => { _isShowingSpecialDialog = false; });
                            error?.Invoke(new WolffunResponseError((int)WSErrorCode.AlreadyResolveFromBase,
                                resultAPI.Body()));
                            return;
                        }
                        break;
                }

                error?.Invoke(err);
            }
            catch (Exception e)
            {
                _httpLog.Log(LogLevel.Error, $"Handle OnError {endpoint} has exception {e.Message}");
                error?.Invoke(new WolffunResponseError((int)WSErrorCode.CantParseError, resultAPI.Body()));
            }
        }
    }

    public enum AuthType
    {
        NONE,
        TOKEN,
        ADMIN,
        SECRET_API_KEY,
        TOKEN_AND_CLIENT_SECRET,
    }
}

