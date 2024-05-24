using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wolffun.RestAPI.ThetanAuth
{
    public enum AuthResultType
    {
        Failed              = -1,
        None                = 0,
        LoginByEmail        = 1,
        RegisterByEmail     = 2,
        LoginAsGuest        = 3,
        LinkAccount         = 4,
    }

    /// <summary>
    /// Struct contain data after authen success
    /// Usually used for analytic purpose
    /// </summary>
    public struct AuthenResultData
    {
        private AuthResultType _authType;
        private string _email;

        public AuthResultType AuthType => _authType;
        public string Email => _email;

        public static AuthenResultData CreateLoginByEmailResult(string email)
        {
            AuthenResultData result = new AuthenResultData();
            result._authType = AuthResultType.LoginByEmail;
            result._email = email;

            return result;
        }

        public static AuthenResultData CreateRegistersByEmailResult(string email)
        {
            AuthenResultData result = new AuthenResultData();
            result._authType = AuthResultType.RegisterByEmail;
            result._email = email;

            return result;
        }

        public static AuthenResultData CreateLoginAsGuestResult()
        {
            AuthenResultData result = new AuthenResultData();
            result._authType = AuthResultType.LoginAsGuest;

            return result;
        }

        public static AuthenResultData CreateLinkAccountResult(string email)
        {
            AuthenResultData result = new AuthenResultData();
            result._authType = AuthResultType.LinkAccount;
            result._email = email;

            return result;
        }

    }
}