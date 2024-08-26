using System;
using ThetanSDK.SDKService.UserStatisticService;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class CardGameLeaderboard : MonoBehaviour
    {
        [SerializeField] private Button _btnTooltipInfo;
        [SerializeField] private TextMeshProUGUI _txtEndIn;
        [SerializeField] private Image _imgLeague;
        [SerializeField] private TextMeshProUGUI _txtLeagueName;
        [SerializeField] private LeaderboardLeagueConfigAsset _leagueConfigAsset;
        [SerializeField] private Button _btnViewYourLeaderboard;
        [SerializeField] private TextMeshProUGUI _txtBtnLeaderboard;

        [SerializeField] private GameObject _contentInfo;
        [SerializeField] private TextMeshProUGUI _txtRank;
        [SerializeField] private TextMeshProUGUI _txtLbPoint;

        [SerializeField] private GameObject _contentEmpty;

        private GameLeaderboardInfo _leaderboardInfo;

        private float _timeEndIn;
        private UIHelperContainer _uiHelperContainer;
        private string _msgTooltip;

        private const string END_IN_FORMAT = "End in: <color=#D4700D>{0}";
        private const string UP_RANK_FORMAT = "{0}<color=#8C90A8>/{1}</color> <sprite=1> <color=#009C60>{2}</color>";
        private const string DOWN_RANK_FORMAT = "{0}<color=#8C90A8>/{1}</color> <sprite=0> <color=#EC466E>{2}</color>";
        private const string STAY_RANK_FORMAT = "{0}<color=#8C90A8>/{1}</color>";

        private void Awake()
        {
            _btnViewYourLeaderboard.onClick.AddListener(OnClickViewReward);
            _btnTooltipInfo.onClick.AddListener(OnClickTooltip);
        }

        private void Update()
        {
            if (_timeEndIn < 0)
                return;

            int prevTimeEndIn = (int)_timeEndIn;
            _timeEndIn -= Time.deltaTime;
            if ((int)_timeEndIn != prevTimeEndIn)
            {
                SetTextEndIn();
            }
        }

        public void SetInitializeInfo(UIHelperContainer uiHelperContainer, string msgTooltip)
        {
            _uiHelperContainer = uiHelperContainer;
            _msgTooltip = msgTooltip;
        }

        public void SetData(GameLeaderboardInfo leaderboardInfo)
        {
            _leaderboardInfo = leaderboardInfo;

            _timeEndIn = (float)(leaderboardInfo.timeEnd - DateTime.UtcNow).TotalSeconds;

            SetTextEndIn();
            var leagueConfig = _leagueConfigAsset.GetConfig(leaderboardInfo.league);

            if(_imgLeague)
                _imgLeague.sprite = leagueConfig.icon;
            if(_txtLeagueName)
                _txtLeagueName.text = $"{leagueConfig.name} League";

            if (leaderboardInfo.lbPoint > 0)
            {
                _contentEmpty.SetActive(false);
                _contentInfo.SetActive(true);
                if (leaderboardInfo.rank > leaderboardInfo.prevRank && leaderboardInfo.prevRank != 0)
                {
                    _txtRank.text = string.Format(DOWN_RANK_FORMAT, leaderboardInfo.rank, leaderboardInfo.maxRank,
                        leaderboardInfo.rank - leaderboardInfo.prevRank);
                }
                else  if (leaderboardInfo.rank < leaderboardInfo.prevRank && leaderboardInfo.prevRank != 0)
                {
                    _txtRank.text = string.Format(UP_RANK_FORMAT, leaderboardInfo.rank, leaderboardInfo.maxRank,
                        leaderboardInfo.prevRank - leaderboardInfo.rank);
                }
                else
                {
                    _txtRank.text = string.Format(STAY_RANK_FORMAT, leaderboardInfo.rank, leaderboardInfo.maxRank);
                }

                _txtLbPoint.text = leaderboardInfo.lbPoint.ToString("N0");
                _txtBtnLeaderboard.text = "See Your Rewards";
            }
            else
            {
                _contentEmpty.SetActive(true);
                _contentInfo.SetActive(false);
                _txtBtnLeaderboard.text = "More Details";
            }
        }

        private void SetTextEndIn()
        {
            _txtEndIn.text = string.Format(END_IN_FORMAT, _timeEndIn.ToStringTime());
        }
        
        private void OnClickViewReward()
        {
            _uiHelperContainer.ShowToast("Info available only on Thetan World Marketplace");
        }

        private void OnClickTooltip()
        {
            if(_uiHelperContainer != null)
                _uiHelperContainer.ShowTextTooltip(_msgTooltip, _btnTooltipInfo.transform as RectTransform,
                    TooltipAlignment.TopMiddle);
        }
    }
}