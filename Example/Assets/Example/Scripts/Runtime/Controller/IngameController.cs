using ThetanSDK;
using ThetanSDK.SDKServices.NFTItem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameController : MonoBehaviour
{
    [SerializeField] private Button btnEndGame;

    private void Start() => SetupEvent();

    private void SetupEvent() => btnEndGame.onClick.AddListener(OnClickEndGame);

    private void OnClickEndGame()
    {
        StopGrind();
        SceneManager.LoadSceneAsync("Menu");
    }

    private void StopGrind()
    {
        if (ThetanSDKManager.Instance.IsGrindingAnyHeroNftItem)
        {
            Debug.Log("Stop Grind");
            ThetanSDKManager.Instance.StopGrindingHeroItem(new EndMatchInfo()
            {
                matchResult = MatchResult.Win,
                gameLevel = 1
            });
        }
    }
}
