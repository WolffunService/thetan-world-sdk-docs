using ThetanSDK;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Button btnPlayGame;

    private void Start()
    {
        SetupEvent();
        SetupUI();
        FirebaseManager.Instance.GetAppCheck();
    }

    private void SetupUI() => ThetanSDKManager.Instance.ShowButtonMainAction();

    private void SetupEvent() => btnPlayGame.onClick.AddListener(OnClickPlayGame);

    private void OnClickPlayGame()
    {
        if (ThetanSDKManager.Instance.IsSelectedAnyHeroNftItem)
        {
            ThetanSDKManager.Instance.PrepareMatchForSelectedNFT(() =>
            {
                Debug.Log("Start Grind");
                ThetanSDKManager.Instance.StartGrindingHeroItem();
                LoadingToIngame();
            }, error =>
            {
                Debug.LogError(error.Message);
            });
        }
        else
        {
            ThetanSDKManager.Instance.HideButtonMainAction();
            LoadingToIngame();
        }
    }

    private void LoadingToIngame() => SceneManager.LoadSceneAsync("Ingame");
}
