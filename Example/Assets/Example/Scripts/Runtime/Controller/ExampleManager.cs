using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wolffun.RestAPI;

public class ExampleManager : MonoBehaviour
{
    private async void Start()
    {
        SetupEvent();
        await UniTask.WaitUntil(() => ThetanSDKManager.IsAlive);
        LoginWithSDK();
    }
    
    private void LoginWithSDK()
    {
        ThetanSDKManager.Instance.Initialize(new SDKOption()
        {
            UseFullscreenLogin = true,
            AutoShowPopupWhenLostConnection = true,
        }, state =>
        {
            LoadingToMenu();
        });
    }

    private async void LoadingToMenu()
    {
        await SceneManager.LoadSceneAsync("Menu");
        ThetanSDKManager.Instance.ShowButtonMainAction();
    }

    private void SetupEvent()
    {
        ThetanSDKManager.Instance.OnUserLogOutCallback += OnLogout;
    }

    private void OnLogout()
    {
        // DO Something here
    }
}
