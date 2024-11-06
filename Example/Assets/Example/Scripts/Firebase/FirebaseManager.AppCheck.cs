using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.AppCheck;
using ThetanSDK;
using UnityEngine;
using Wolffun.Log;

public partial class FirebaseManager
{
    public async void GetAppCheck()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        CommonLog.Log("Start GetAppCheckTokenAsync");
        string log = "GetAppCheckTokenAsync";
        try
        {
            var appCheck = FirebaseAppCheck.DefaultInstance;
            if (appCheck != null)
            {
                CommonLog.Log("AppCheck default instance not null");
                var appCheckToken = await appCheck.GetAppCheckTokenAsync(false);
                log = "Start GetAppCheckTokenAsync success for default instance " +
                      appCheckToken.Token;
                ThetanSDKManager.Instance.SetAppCheckToken(appCheckToken.Token);
            }
            else
            {
                log = "AppCheck instance null";
            }
        }
        catch (AggregateException e)
        {
            CommonLog.LogError($"GetAppCheckTokenAsync default instance failed with exception: {e.Message}");
        }
#endif
        
    }
}
