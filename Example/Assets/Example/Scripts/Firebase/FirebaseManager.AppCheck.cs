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
        Debug.Log("Start GetAppCheckTokenAsync");
        try
        {
            var firebaseApp = FirebaseAppCheck.DefaultInstance;
            if (firebaseApp == null) return;
            var appCheckToken = await firebaseApp.GetAppCheckTokenAsync(false);
            ThetanSDKManager.Instance.SetAppCheckToken(appCheckToken.Token);
        }
        catch (AggregateException e)
        {
            Debug.LogError($"GetAppCheckTokenAsync default instance failed with exception: {e.Message}");
        }
#endif
    }
}
