using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThetanSDK.UI
{
    public static class AuthenErrorMsg
    {
        public static string Error = "Error";
        public static string AccountBanned = "Account Banned";

        public static string AccountBannedContactSupport =
            "Your account has been banned. Please contact support for further assistance.";

        public static string ConnectedAccountContext =
            "Hey there! It seems like your device has connected to {0}. Could you please log in with this email instead? Thanks!";

        public static string Okay = "Okay";
        public static string Confirm = "Confirm";

        public static string TooManyAccountContext =
            "You have reached the maximum login limit. Only 2 accounts are allowed on the same device. Please try again by using other accounts that have been successfully logged in before.";

        public static string LostConnectionContext =
            "You have lost connection with the server. Please check your network and try again!";

        public static string UnknownErrorContext = "An error has occurred, please try again later.";

        public static string UnknownErrorContextWithDetail =
            "An error has occurred, please try again later.\nErrorCode: {0}.\nMessage: {1}";

        public static string InvalidCode = "Invalid code";
        public static string InvalidCodeContext = "Please make sure to use the correct code sent to you.";

        public static string InvalidEmail = "Invalid Email";
        public static string EmailNotExistContext = "This email is not exist, please register";
        public static string EmailExistedContext = "This email is existed, please login";

        public static string TIME_OUT_OPEN_WS = "Cannot open connect to server in allowed time";

        public static string TIME_OUT_WAIT_LOGIN_CODE_DATA = "Cannot receive login code from server in allowed time";

        public static string DO_NOT_HAVE_PERMISSION =
            "You do not have permission to access our services. Please make sure your IP is allowed to access our services or contact our technical support for further support.";
    }
}

