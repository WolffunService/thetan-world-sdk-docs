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
            switch (state)
            {
                case ThetanNetworkClientState.LoggedIn:
                    LoadingToMenu();
                    break;
                case ThetanNetworkClientState.NotLoggedIn:
                    LoadingToLogin();
                    break;
                case ThetanNetworkClientState.Banned:
                case ThetanNetworkClientState.NotInitialized:
                case ThetanNetworkClientState.LoggedInNoNetwork:
                case ThetanNetworkClientState.NotLoggedInNoNetwork:
                    break;
                default:
                    break;
            }
        });
    }

    private async void LoadingToLogin() => await SceneManager.LoadSceneAsync("Login");

    private async void LoadingToMenu()
    {
        await SceneManager.LoadSceneAsync("Menu");
        ThetanSDKManager.Instance.ShowButtonMainAction();
    }

    private void SetupEvent()
    {
        ThetanSDKManager.Instance.OnUserLogOutCallback += OnLogout;
    }

    private void OnLogout() => SceneManager.LoadSceneAsync("Login");
}
