using System;
using Cysharp.Threading.Tasks;
using ThetanSDK;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShowSplash : MonoBehaviour
{
    public float logoFullScreenScale = 0.8f;
    public float unityLogoHeightScale = 0.21f;
    [SerializeField]
    private float 
        timeMin = 3, 
        timeMax = 5;
    public Texture texture;

    private async void Start()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(timeMin));
        SceneManager.LoadSceneAsync($"Loading", LoadSceneMode.Additive);
        await UniTask.WaitUntil(() => ThetanSDKManager.IsAlive);
        SceneManager.UnloadSceneAsync("Logo");
    }

    void OnGUI()
    {
        GL.Clear(true,true, Color.black);
        var windowSz = new Rect(0, 0, Screen.width, Screen.height);
        float minSideLength = Mathf.Min(Screen.width, Screen.height);
        var forgroundRect = new Rect(
            windowSz.x + (windowSz.width - minSideLength) * 0.25f,
            windowSz.y + (windowSz.height - minSideLength) * 0.25f,
            (windowSz.width + minSideLength) * 0.5f,
            (windowSz.height + minSideLength) * 0.5f);
            var center = forgroundRect.center;
            forgroundRect.size *= logoFullScreenScale;
            forgroundRect.center = center;
        if (texture)
            GUI.DrawTexture(forgroundRect, texture, ScaleMode.ScaleToFit);
    }
}