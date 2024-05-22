using System;
using Newtonsoft.Json.Utilities;
using UnityEngine;
using Wolffun.MultiPlayer;

namespace Wolffun.RestAPI.ThetanAuth
{
    [Serializable]
    public class ThetaAuthModels
    {
    }

    #region Request

    [Serializable]
    public class SendCodeRequestModel
    {
        public string email;
        public bool createAccount;
        public bool fromUnity;
    }

    [Serializable]
    public class LoginAccountRequestModel
    {
        public string email;
        public int code;
        public string deviceId;
        public bool fromUnity;
        public int typeGame;
    }

    [Serializable]
    public class CreateAccountRequestModel
    {
        public string email;
        public int code;
        public string deviceId;
        public int typeGame;
    }

    [Serializable]
    public class RefreshTokenRequestModel
    {
        public string refreshToken;
    }


    [Serializable]
    public class LoginGuessAccountRequestModel
    {
        public string deviceId { get; set; }
        public int typeGame { get; set; }
    }

    [Serializable]
    public struct CheckPlayableRequestModel
    {
        public int gameID { get; set; }
    }
    
    #endregion

    #region Response

    [Serializable]
    public class SendCodeResponse
    {
    }

    [Serializable]
    public struct TokenResponseModel : ICustomDefaultable<TokenResponseModel>
    {
        public string accessToken;
        public string refreshToken;

        public bool IsEmpty() => string.IsNullOrEmpty(accessToken);

        public TokenResponseModel SetDefault()
        {
            accessToken = refreshToken = string.Empty;

            return this;
        }
    }

    [Serializable]
    public class LinkAccountDataResponse
    {
    }

    [Serializable]
    public struct CheckPlayableResponseModel
    {
        public bool playable;
    }
    [Serializable]
    public struct ChangeNameResponseModel
    {
        public ChangeNameFee fee;
    }
    [Serializable]
    public struct ChangeNameFee
    {
        public int type;
        public int value;
        public int decimals;

        public override string ToString()
        {
            return $"{type} {value} {decimals}";
        }
    }

    [Serializable]
    public struct ChangeAvatarResponseModel
    {
        public bool success;
        public int code;
    }

    [Serializable]
    public struct ChangeAvatarFrameResponseModel
    {
        public bool success;
        public int code;
    }

    #endregion
}