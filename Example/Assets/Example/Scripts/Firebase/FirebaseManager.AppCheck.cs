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
#if (UNITY_ANDROID || UNITY_IOS)
        CommonLog.Log("Start GetAppCheckTokenAsync");
        try
        {
            var appCheck = FirebaseAppCheck.DefaultInstance;
            if (appCheck != null)
            {
                var appCheckToken = await appCheck.GetAppCheckTokenAsync(false);
                ThetanSDKManager.Instance.SetAppCheckToken(appCheckToken.Token);
            }
        }
        catch (AggregateException e)
        {
            CommonLog.LogError($"GetAppCheckTokenAsync default instance failed with exception: {e.Message}");
        }
#endif
        
    }
}
