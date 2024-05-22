using Cysharp.Threading.Tasks;
using System;
using BestHTTP.Authentication;

namespace Wolffun.RestAPI
{
    public struct APIResponse
    {
        public WolffunResponseCommon ResponseCommon;
        public WolffunResponseError ResponseError;
        public bool IsSuccess;

        public void SetUp(WolffunResponseCommon responseCommon, WolffunResponseError responseError, bool isSuccess)
        {
            ResponseCommon = responseCommon;
            ResponseError = responseError;
            IsSuccess = isSuccess;
        }
    }
    public struct APIResponseWrapper<T>
    {
        public T Data;
        public WolffunResponseError Error;
        public bool IsSuccess;
    }
    
    // public async UniTask<APIResponseWrapper<T>> MakeAPITaskSimple<T>(WolffunRequestCommon request,
    //     string accessToken)
    // {
    //     var result = default(APIResponseWrapper<T>);
    //     try
    //     {
    //         request.AddHeader("Authorization", "Bearer " + accessToken);
    //         request.AddHeader("Content-Type", "application/json");
    //         var response = await WolffunHttp.SendAPI<T>(request);
    //
    //         if (response.IsSuccess)
    //         {
    //             var res = response.ResponseCommon.To<T>();
    //             if (res.Success)
    //                 return new APIResponseWrapper<T> { Data = res.Data, IsSuccess = true };
    //         }
    //
    //         return new APIResponseWrapper<T> { Data = default, IsSuccess = false, Error = response.ResponseError };
    //     }
    //     catch (Exception ex)
    //     {
    //         CommonLog.LogError("MakeAPI throw exception " + ex.Message + "_" + ex.StackTrace);
    //     }
    //
    //     return result;
    // }
    
    // public async UniTask<APIResponseWrapper<T>> MakeAPITask<T>(WolffunRequestCommon request,
    //     AuthType authType = AuthType.NONE, Action<WolffunResponseError> error = null, Action<Page> pageData = null)
    // {
    //     var result = default(APIResponseWrapper<T>);
    //     try
    //     {
    //         if (!accessTokenData.IsDoneLoadLocalData())
    //             await UniTask.WaitUntil(() => accessTokenData.IsDoneLoadLocalData());
    //
    //         var accessToken = accessTokenData.GetAccessToken();
    //
    //         switch (authType)
    //         {
    //             case AuthType.TOKEN when !string.IsNullOrEmpty(accessToken):
    //                 request.AddHeader("Authorization", "Bearer " + accessToken);
    //                 break;
    //             case AuthType.ADMIN when !string.IsNullOrEmpty(Settings.AdminAccessToken):
    //                 request.AddHeader("Authorization", "Bearer " + Settings.AdminAccessToken);
    //                 break;
    //         }
    //
    //         request.AddHeader("Content-Type", "application/json");
    //         var response = await WolffunHttp.SendAPI<T>(request);
    //
    //         if (response.IsSuccess)
    //         {
    //             var res = response.ResponseCommon.To<T>();
    //             if (res.Success)
    //             {
    //                 pageData?.Invoke(res.Paging);
    //                 return new APIResponseWrapper<T> { Data = res.Data, IsSuccess = true };
    //             }
    //         }
    //
    //         OnError(error, response.ResponseCommon);
    //         return new APIResponseWrapper<T> { Data = default, IsSuccess = false, Error = response.ResponseError };
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.LogError("MakeAPI throw exception " + ex.Message + "_" + ex.StackTrace);
    //     }
    //
    //     return result;
    // }

}