using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

public class LoginController : MonoSingleton<LoginController>
{
    [SerializeField] private Button btnLogin;
    private async void Start()
    {
        SetupEvent();
        ShowLogin();
    }

    private void SetupEvent()
    {
        btnLogin.onClick.AddListener(ShowLogin);
    }

    private void ShowLogin() => ThetanSDKManager.Instance.ShowLogin(HandleLoginCallback, () =>
    {
        btnLogin.gameObject.SetActive(true);
    });

    private async void LoadingToMenu()
    {
        await SceneManager.LoadSceneAsync("Menu");
        ThetanSDKManager.Instance.ShowButtonMainAction();
    }
    
    private void HandleLoginCallback(AuthenResultData data)
    {
        switch (data.AuthType)
        {
            case AuthResultType.LoginByEmail:
            case AuthResultType.RegisterByEmail:
            case AuthResultType.LoginAsGuest:
                LoadingToMenu();
                break;
        }

    }
}
