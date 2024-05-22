using Cysharp.Threading.Tasks;
using System;
using BestHTTP.Authentication;
using UnityEngine;

namespace Wolffun.RestAPI
{
    public static class WolffunHttp
    {
        public static async void SendAPI(WolffunRequestCommon request, WolffunHttpLog log, Action<WolffunResponseCommon> success,
            Action<WolffunResponseError> error)
        {
            try
            {
                using var httpRequest = new BestHTTP.HTTPRequest(new Uri(request.Url()), request.Method());
                httpRequest.Timeout = request.Timeout();
                var body = request.Body();
                if (body != null)
                {
                    httpRequest.AddHeader("Content-Type", body.ContentType());
                    httpRequest.RawData = body.Body();
                }

                var headers = request.Headers();

                log.Log(LogLevel.Info, $"Send API Request: {request.Url()}");

                if (headers != null)
                    foreach (var header in headers)
                    {
                        httpRequest.SetHeader(header.Key, header.Value);
                    }

                await httpRequest.Send();

                if (httpRequest.State == BestHTTP.HTTPRequestStates.Finished)
                {
                    using var response = WolffunResponseCommon.From(httpRequest);
                    success(response);
                }
                else
                {
                    string errorMsg = httpRequest.Response != null ? httpRequest.Response.Message : string.Empty;
                    var response =
                    new WolffunResponseError((int)WSErrorCode.UnityHttpRequestNetworkError, errorMsg);
                    error(response);
                }
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, $"Send API Request {request.Url()} with exception {ex.GetBaseException()} \n {ex.StackTrace} \n {ex.Message}");
                error?.Invoke(new WolffunResponseError((int)WSErrorCode.Error, ex.Message));
            }
            finally
            {
                request.Dispose();
            }
        }

        public static async UniTask<APIResponse> SendAPI<T>(WolffunRequestCommon request, WolffunHttpLog log)
        {
            APIResponse response = default;
            try
            {
                using var httpRequest = new BestHTTP.HTTPRequest(new Uri(request.Url()), request.Method());
                httpRequest.Timeout = request.Timeout();
                var body = request.Body();
                if (body != null)
                {
                    httpRequest.AddHeader("Content-Type", body.ContentType());
                    httpRequest.RawData = body.Body();
                }

                var headers = request.Headers();
                if (headers != null)
                    foreach (var header in headers)
                        httpRequest.SetHeader(header.Key, header.Value);

                await httpRequest.Send();

                if (httpRequest.State == BestHTTP.HTTPRequestStates.Finished)
                {
                    using var responseData = WolffunResponseCommon.From(httpRequest);
                    response.SetUp(responseData, default, true);
                }
                else
                {
                    var responseData = new WolffunResponseError((int)WSErrorCode.UnityHttpRequestNetworkError,
                        httpRequest.Response.Message);
                    response.SetUp(default, responseData, false);
                }
            }
            catch (UnityWebRequestException ex)
            {
                log.Log(LogLevel.Error, $"Send API Request {request.Url()} with exception {ex.GetBaseException()} \n {ex.StackTrace} \n {ex.Message}");
                response.SetUp(default, new WolffunResponseError((int)WSErrorCode.Error, ex.Message), false);
            }
            finally
            {
                request.Dispose();
            }

            return response;
        }
    }
}