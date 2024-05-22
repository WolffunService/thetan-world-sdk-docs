using System;
using Cysharp.Threading.Tasks;

namespace Wolffun.RestAPI.ThetanAuth
{
	public static class ThetaAuthAPI
	{
		
		/// <summary>
		/// SendCode wolffunId
		/// </summary>
		public static void SendCode(SendCodeRequestModel request, Action<SendCodeResponse> result, Action<WolffunResponseError> error)
		{
			WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/send-code")
				.Post(WolffunRequestBody.From(request));
			WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error);
		}

		/// <summary>
		/// Request login WolffunId
		/// </summary>
		public static void LoginAccount(LoginAccountRequestModel request, Action<TokenResponseModel> result, Action<WolffunResponseError> error)
		{
			WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/login-account")
				.Post(WolffunRequestBody.From(request));
			WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
		}

		/// <summary>
		/// Request create new WolffunId
		/// </summary>
		public static void CreateAccount(CreateAccountRequestModel request, Action<TokenResponseModel> result, Action<WolffunResponseError> error)
		{
			WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/create-account")
				.Post(WolffunRequestBody.From(request));
			WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
		}

		///// <summary>
		///// Request link account wolffunId :
		///// - verify code wolffunId
		///// - update Username password
		///// <para>Must call WolffunIdAPI.Regis first for verify this wolffun ID not exist in system</para>
		///// </summary>
		//public static void LinkWolffunId(LinkWolffunIdRequestModel request, Action<TokenResponseModel> result, Action<WolffunResponseError> error)
		//{
		//	WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/linkWolffunId")
		//		.Post(WolffunRequestBody.From(request));
		//	WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error);
		//}

		/// <summary>
		/// Request register new account WolffunID
		/// Need author
		/// </summary>
		public static void LoginByToken(Action<TokenResponseModel> result, Action<WolffunResponseError> error)
		{
			WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/login-token");
			WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
		}
		//Action<TokenResponseModel> result,
		// public static void LoginByToken(Action<TokenResponseModel> result, Action<WolffunResponseError> error)
		// {
		// 	var reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/loginToken");
		// 	WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN);
		// 	// var res = await WolffunUnityHttp.Instance.MakeAPITask<TokenResponseModel>(reqCommon, AuthType.TOKEN);
		// 	// return res;
		// }

		/// <summary>
		/// Request verify refresh and generate new token
		/// </summary>
		public static void RefreshToken(RefreshTokenRequestModel request, Action<TokenResponseModel> result, Action<WolffunResponseError> error)
		{
			WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/refresh-token")
				.Post(WolffunRequestBody.From(request));
			WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
		}

        public static void LoginGuessAccount(LoginGuessAccountRequestModel request, Action<TokenResponseModel> result, Action<WolffunResponseError> error)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/login-as-guest")
                .GetQuery(Utils.GetProperties(request));
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        public static void LinkAccount(CreateAccountRequestModel request, Action<LinkAccountDataResponse> result, Action<WolffunResponseError> error)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/link-account-guest")
                .Post(WolffunRequestBody.From(request));
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

		public static void LoginAccountByEmail(string email, Action<object> result, Action<WolffunResponseError> error)
        {
#if THETA_ADMIN_MODE
			WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/loginByEmail")
				.Post(WolffunRequestBody.From(new { email = email }));
			WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
#endif
		}

#if BETA_TEST
        public static void CheckRegionAccepted(Action<CheckPlayableResponseModel> response, Action<WolffunResponseError> error)
        {
            CheckPlayableRequestModel request = new CheckPlayableRequestModel()
            {
                gameID = 1
            };  

            WolffunRequestCommon reqCommon = WolffunRequestCommon.Create(WolffunUnityHttp.Settings.AuthServiceURL + "/checkPlayable")
                .GetQuery(Utils.GetProperties(request));

            WolffunUnityHttp.Instance.MakeAPI(reqCommon, response, error, AuthType.TOKEN_AND_CLIENT_SECRET);
        }
#endif
    }
}