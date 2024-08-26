using System;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService.UserStatisticService;
using ThetanSDK.SDKServices.Profile;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public abstract class ScreenMainUI : Screen
    {
        [SerializeField] protected UIHelperContainer _uiHelperContainer;
        [SerializeField] private UserProfileAvatar _userProfileAvatar;
        [SerializeField] private TextMeshProUGUI _txtUserName;
        [SerializeField] private CommonHeroNFTCardInfo _selectedNFTUI;
        [SerializeField] private GameObject _contentNotSelectedNFT;
        [SerializeField] private Button _btnChangeNft;
        [SerializeField] private Button _btnShowAccountInfo;
        [SerializeField] private PopupAccountInfo _prefabPopupAccountInfo;
        [SerializeField] private CheckDoThetanGateTutorial _checkDoThetanGateTutorial;
        [SerializeField] private Button _btnDetailNFT;

        [Header("Today's Reward")]
        [SerializeField] private UICardInfoStatistic _cardGrind;
        [SerializeField] private UICardInfoStatistic _cardVictoryBonus;
        [SerializeField] private UICardInfoStatistic _cardAirDrop;

        [Header("Leaderboard")]
        [SerializeField] private CardGameLeaderboard _mineLeagueLeaderboard;
        [SerializeField] private CardWorldLeaderboard _worldLeaderboard;

        [Header("Guest Account")]
        [SerializeField] private PopupInviteLinkGuestAccount _prefabPopupInviteLinkGuestAccount;
        [SerializeField] private Button _btnInviteLinkAccount;

        [Header("Tournament")]
        [SerializeField] private Button _btnTournament;
        [SerializeField] private Button _btnTooltipTournament;

        private SpriteCacheManager _spriteCacheManager;

        private const string MSG_TOOLTIP_GRIND_REWARD =
            "Rewards earned through playtime. The more you play, the more you earn.";

        private const string MSG_TOOLTIP_VICTORY_REWARD =
            "Rewards earned through battle result. Winning battles grants you the BIG bonus. Even in defeat, you will still earn a consolation reward.";

        private const string MSG_TOOLTIP_DAILY_AIRDROP = "Airdrop accumulates from claimed grind and victory rewards.";

        private const string MSG_TOOLTIP_GAME_LEADERBOARD =
            "Play matches, join the League, earn points, and win weekly rewards.";

        private const string MSG_TOOLTIP_WORLD_LEADERBOARD =
            "Compete with all users across games on the World leaderboard for bigger monthly prizes.";

        private const string MSG_TOOLTIP_TOURNAMENT =
            "The ultimate tournament to crown Thetan World's top champion across all games.";

        public abstract void ShowScreenListNFT();

        private void Awake()
        {
            _btnTournament.onClick.AddListener(ShowComingSoon);
            _spriteCacheManager = new SpriteCacheManager();
            _userProfileAvatar.SetSpriteCacheManager(_spriteCacheManager);
            _btnChangeNft.onClick.AddListener(ShowScreenListNFT);
            _btnShowAccountInfo.onClick.AddListener(ShowAccountInfo);
            _btnInviteLinkAccount.onClick.AddListener(ShowPopupInviteLinkAccount);

            _mineLeagueLeaderboard.SetInitializeInfo(_uiHelperContainer, MSG_TOOLTIP_GAME_LEADERBOARD);
            _worldLeaderboard.SetInitializeInfo(_uiHelperContainer, MSG_TOOLTIP_WORLD_LEADERBOARD);

            _cardGrind.Initialize(_uiHelperContainer, MSG_TOOLTIP_GRIND_REWARD);
            _cardVictoryBonus.Initialize(_uiHelperContainer, MSG_TOOLTIP_VICTORY_REWARD);
            _cardAirDrop.Initialize(_uiHelperContainer, MSG_TOOLTIP_DAILY_AIRDROP);
            _btnTooltipTournament.onClick.AddListener(ShowTooltipTournament);
            _btnDetailNFT.onClick.AddListener(ShowDetailNFT);
        }

        private void OnFreeNFTChangedData(FreeNFTInfo freeNFT)
        {
            if (string.IsNullOrEmpty(freeNFT.nftId))
                _checkDoThetanGateTutorial.CheckDoTutorial(() => _screenContainer.NotifyOnClickCloseScreen());
        }

        private async void ShowDetailNFT()
        {
            var nftService = ThetanSDKManager.Instance.NftItemService;
            if (nftService.IsSelectedAnyHeroNFT())
            {
                var nftData = await ThetanSDKManager.Instance.GetHeroNftItemInfo(nftService.SelectedHeroNftId);
                ShowDetailNFT(nftData);
            }
        }

        protected abstract void ShowDetailNFT(HeroNftItem nftData);

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();

            var profileService = ThetanSDKManager.Instance.ProfileService;
            _userProfileAvatar.SetUI(profileService.AvatarId, profileService.AvatarFrameId);

            if (_txtUserName)
                _txtUserName.SetText(profileService.Username);

            _btnInviteLinkAccount.gameObject.SetActive(string.IsNullOrEmpty(profileService.Email));
            profileService.OnChangeProfileDataCallback += HandleOnChangeProfileData;

            var statisticService = ThetanSDKManager.Instance.UserStatisticService;
            if (statisticService.UserStatisticData != null)
            {
                SetDataStatisticData(statisticService.UserStatisticData.Value);
                statisticService.FetchUserStatisticData(null, null);
            }

            if (statisticService.LeaderboardData != null)
            {
                SetDataLeaderboard(statisticService.LeaderboardData.Value);
                statisticService.FetchUserLeaderboardData(null, null);
            }

            statisticService.OnChangeUserStatisticDataCallback -= HandleChangeStatisticData;
            statisticService.OnChangeUserStatisticDataCallback += HandleChangeStatisticData;

            statisticService.OnChangeLeaderboardDataCallback -= HandleChangeLeaderboardData;
            statisticService.OnChangeLeaderboardDataCallback += HandleChangeLeaderboardData;


            var nftService = ThetanSDKManager.Instance.NftItemService;
            ShowUIForSelectedNFT();
            nftService.RegisterOnChangeNftItemData(HandleOnChangeNFTItemData);
            nftService.RegisterOnChangeSelectedNftHeroCallback(HandleOnChangeNFT);
            nftService._onListNFTFetchSuccessCallback += HandleRefetchListNFT;
            nftService._onRefreshFreeNFTInfo -= OnFreeNFTChangedData;
            nftService._onRefreshFreeNFTInfo += OnFreeNFTChangedData;
        }

        public override async void OnAfterPushScreen()
        {
            base.OnAfterPushScreen();

            _checkDoThetanGateTutorial.CheckDoTutorial(() => _screenContainer.NotifyOnClickCloseScreen());
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();

            _uiHelperContainer.CloseAllPopup();

            var statisticService = ThetanSDKManager.Instance.UserStatisticService;
            statisticService.OnChangeUserStatisticDataCallback -= HandleChangeStatisticData;
            statisticService.OnChangeLeaderboardDataCallback -= HandleChangeLeaderboardData;

            var nftService = ThetanSDKManager.Instance.NftItemService;
            nftService.UnRegisterOnChangeNftItemData(HandleOnChangeNFTItemData);
            nftService.UnRegisterOnChangeSelectedNftHeroCallback(HandleOnChangeNFT);
            nftService._onListNFTFetchSuccessCallback -= HandleRefetchListNFT;
            nftService._onRefreshFreeNFTInfo -= OnFreeNFTChangedData;
            
            var profileService = ThetanSDKManager.Instance.ProfileService;
            profileService.OnChangeProfileDataCallback -= HandleOnChangeProfileData;

            ThetanSDKManager.Instance.NftItemService._onRefreshFreeNFTInfo -= OnFreeNFTChangedData;
        }

        private void HandleOnChangeProfileData(SDKUserProfileModel data)
        {
            _userProfileAvatar.SetUI(data.avatar, data.avatarFrame);

            if (_txtUserName)
                _txtUserName.SetText(data.nickname);

            _btnInviteLinkAccount.gameObject.SetActive(string.IsNullOrEmpty(data.email));
        }

        private async void ShowAccountInfo()
        {
            var popupAccountInfo = await _uiHelperContainer.PushPopup(_prefabPopupAccountInfo, new PopupOption()
            {
                IsAllowBackdrop = true
            }) as PopupAccountInfo;

            popupAccountInfo.Initialize(_uiHelperContainer, HandleUserLogOut);
        }

        private async void ShowPopupInviteLinkAccount()
        {
            var popupInviteLinkAccount = await _uiHelperContainer.PushPopup(_prefabPopupInviteLinkGuestAccount, new PopupOption()
            {
                IsAllowBackdrop = true
            }) as PopupInviteLinkGuestAccount;

            popupInviteLinkAccount.Initialize(() =>
            {
                _uiHelperContainer.ShowToast("Link account success");
            }, () =>
            {
                _uiHelperContainer.ShowToast("Login account success");
            });
        }

        private void HandleUserLogOut()
        {
            _screenContainer.NotifyOnClickCloseScreen();
        }

        private void HandleOnChangeNFT(string selectedNFTId)
        {
            ShowUIForSelectedNFT();
        }

        private void HandleOnChangeNFTItemData(HeroNftItem data)
        {
            ShowUIForSelectedNFT();
        }

        private void HandleRefetchListNFT()
        {
            ShowUIForSelectedNFT();
        }

        private async void ShowUIForSelectedNFT()
        {
            var nftService = ThetanSDKManager.Instance.NftItemService;
            if (nftService.IsSelectedAnyHeroNFT())
            {

                var selectedNFTHero = await ThetanSDKManager.Instance.GetHeroNftItemInfo(nftService.SelectedHeroNftId);

                if (!selectedNFTHero.IsEmpty())
                {
                    _selectedNFTUI.gameObject.SetActive(true);
                    _contentNotSelectedNFT.SetActive(false);
                    _selectedNFTUI.SetData(selectedNFTHero);
                }
                else
                {
                    nftService.GetInfoDataHeroNftOnServer(nftService.SelectedHeroNftId, item =>
                    {
                        _selectedNFTUI.gameObject.SetActive(true);
                        _contentNotSelectedNFT.SetActive(false);
                        _selectedNFTUI.SetData(item);
                    }, error =>
                    {
                        ShowUINotSelectedAnyNft();
                    });
                }
            }
            else
            {
                ShowUINotSelectedAnyNft();
            }
        }

        private void ShowUINotSelectedAnyNft()
        {
            _selectedNFTUI.gameObject.SetActive(false);
            _contentNotSelectedNFT.SetActive(true);
        }

        private void HandleChangeStatisticData(UserStatisticData statisticData)
        {
            SetDataStatisticData(statisticData);
        }

        private void SetDataStatisticData(UserStatisticData statisticData)
        {
            float grindReward = GetTHGReward(statisticData.grindReward);
            float victoryReward = GetTHGReward(statisticData.victoryReward);
            float airDropReward = GetTHGReward(statisticData.airdropReward);
            float unclaimedGrindReward = GetTHGReward(statisticData.unclaimGrindReward);
            float unclaimedVictoryReward = GetTHGReward(statisticData.unclaimVictoryReward);
            float unclaimedAirDropReward = GetTHGReward(statisticData.unclaimAirdropReward);
            
            _cardGrind.SetData(grindReward, unclaimedGrindReward);
            _cardVictoryBonus.SetData(victoryReward, unclaimedVictoryReward);
            if (statisticData.unclaimAirdropReward != null && 
                statisticData.unclaimAirdropReward.Length > 1)
            {
                _cardAirDrop.SetData(airDropReward, unclaimedAirDropReward, "+{0:N2} + <sprite=0>");
            }
            else
            {
                _cardAirDrop.SetData(airDropReward, unclaimedAirDropReward, "+{0:N2}");
            }
        }

        private float GetTHGReward(DecimalItemNumber[] arrayReward)
        {
            if (arrayReward != null)
            {
                foreach (var reward in arrayReward)
                {
                    if (reward.ItemType == ItemRewardType.gTHG)
                    {
                        return (float)reward.GetRealValue();
                    }
                }
            }

            return 0;
        }

        private void HandleChangeLeaderboardData(LeaderboardData leaderboardData)
        {
            SetDataLeaderboard(leaderboardData);
        }

        private void SetDataLeaderboard(LeaderboardData leaderboardData)
        {
            _mineLeagueLeaderboard.SetData(leaderboardData.gameLeaderboard);
            _worldLeaderboard.SetData(leaderboardData.worldLeaderboard);
        }

        private void ShowComingSoon()
        {
            _uiHelperContainer.ShowToast("Coming Soon");
        }

        private void ShowTooltipTournament()
        {
            _uiHelperContainer.ShowTextTooltip(MSG_TOOLTIP_TOURNAMENT, _btnTooltipTournament.transform as RectTransform,
                TooltipAlignment.TopMiddle);
        }
    }
}