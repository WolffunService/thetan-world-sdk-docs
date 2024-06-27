using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class GameSummaryInfoUI : MonoBehaviour
    {
        [SerializeField] private GameIconImg _gameIcon;
        [SerializeField] private TextMeshProUGUI _txtGameName;
        [SerializeField] private TextMeshProUGUI _txtGrindTime;
        [SerializeField] private TextMeshProUGUI _txtGrindReward;

        public void SetData(GameDailySummaryData data)
        {
            _gameIcon.ShowUI(data.GameWorldType);
            _txtGameName.text = ThetanSDKUtilities.GetWorldName(data.GameWorldType);
            _txtGrindTime.text = data.grindInfo.grindTime.ToStringTime();
            _txtGrindReward.SetText("{0}", data.grindInfo.grindPoint);
        }
    }
}