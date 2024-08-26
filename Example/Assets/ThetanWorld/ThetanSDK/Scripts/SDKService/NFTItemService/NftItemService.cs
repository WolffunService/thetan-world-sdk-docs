using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ThetanSDK.SDKService;
using ThetanSDK.SDKServices.Analytic;
using UnityEngine;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;
using Random = System.Random;

namespace ThetanSDK.SDKServices.NFTItem
{
    /// <summary>
    /// Service use for interact with NFT items, including grinding a NFT item
    /// </summary>
    internal class NftItemService : BaseClassService
    {
        #region Constant
        /// <summary>
        /// How many items service should fetch for each page
        /// </summary>
        private const int NUMBER_ITEM_PER_PAGE = 150;

        /// <summary>
        /// Interval (in second) for service to send grind signal for server
        /// </summary>
        private const int PING_GRINDING_SERVER_INTERVAL = 5;

        /// <summary>
        /// Interval (in second) for service refresh data of grinding item when service is in pausing grind state
        /// </summary>
        private const int REFRESH_DATA_WHEN_PAUSE_GRIND_INTERVAL = 20;

        private const string SAVE_DATA_LOCAL_NFT_SERVICE_NAME = "DataLocalNFTService";
        #endregion

        /// <summary>
        /// All supported sort type when fetch list NFT.
        /// Each sort type contain one or many sort criteria, priority as they appeared in list
        /// </summary>
        private readonly List<(string, List<NFTSortType>)> _listSortType = new List<(string, List<NFTSortType>)>()
        {
            ("NFT Rarity", new List<NFTSortType>()
            {
                NFTSortType.Rarity,
                NFTSortType.EquipmentEffect,
                NFTSortType.GrindReward,
                NFTSortType.GrindTime,
            }),
            ("Grind Ability", new List<NFTSortType>()
            {
                NFTSortType.EquipmentEffect,
                NFTSortType.Rarity,
                NFTSortType.GrindReward,
                NFTSortType.GrindTime,
            }),
            ("Grind Speed", new List<NFTSortType>()
            {
                NFTSortType.GrindReward,
                NFTSortType.Rarity,
                NFTSortType.EquipmentEffect,
                NFTSortType.GrindTime,
            })
        };
        internal List<(string, List<NFTSortType>)> ListSortType => _listSortType;

        /// <summary>
        /// Current selected index of _listSortType
        /// </summary>
        private int _currentSortTypeIndex = 0;
        public int CurrentSortTypeIndex
        {
            get => _currentSortTypeIndex;
            set => _currentSortTypeIndex = value;
        }

        /// <summary>
        /// Network client of ThetanSDK
        /// </summary>
        private NetworkClient _networkClient;

        /// <summary>
        /// List HeroNFTItem that has been fetched and cache locally
        /// </summary>
        private List<HeroNftItem> _listHeroNftItems;
        public List<HeroNftItem> ListHeroNftItems => _listHeroNftItems;

        /// <summary>
        /// Count how many total NFT user has.
        /// This different than _listHeroNftItems.Count because _listHeroNftItems use paging when fetch data
        /// </summary>
        private int _countTotalNFT = -1;
        public int CountTotalNFT => _countTotalNFT;

        /// <summary>
        /// Dictionaty convert HeroNFTId to index in _listHeroNftItems, this use to speed up search
        /// </summary>
        private Dictionary<string, int> _dicHeroNftIdToListIndex;
        public Dictionary<string, int> DicHeroNftIdToListIndex => _dicHeroNftIdToListIndex;

        /// <summary>
        /// Dictionaty convert IngameHeroId to index in _listHeroNftItems, this use to speed up search
        /// </summary>
        private Dictionary<string, int> _dicIngameHeroIdToListIndex;

        /// <summary>
        /// HeroNFTId of selected hero NFT.
        /// Will be null or empty when not select any hero NFT
        /// </summary>
        private string _selectedHeroNftId;
        public string SelectedHeroNftId => _selectedHeroNftId;

        /// <summary>
        /// HeroNFTId of grinding hero NFT.
        /// Will be null or empty when not grinding any hero NFT
        /// </summary>
        private string _grindingHeroNftId;
        public string GrindingHeroNftId => _grindingHeroNftId;

        /// <summary>
        /// SessionId of grinding session
        /// Will be null or empty when not in grinding session
        /// </summary>
        private string _grindSessionId;
        public string GrindSessionId => _grindSessionId;

        /// <summary>
        /// Info about free NFT
        /// </summary>
        private FreeNFTConfig _freeNFTConfig;
        public FreeNFTConfig FreeNFTConfig => _freeNFTConfig;
        
        /// <summary>
        /// Info about free NFT section earned
        /// </summary>
        private FreeNFTInfo _freeNFtInfo;
        public FreeNFTInfo FreeNftInfo => _freeNFtInfo;

        /// <summary>
        /// Detect when player has started new free nft play section
        /// </summary>
        private bool _isInFreeNftPlaySection;

        /// <summary>
        /// Check if is fetching free nft info
        /// </summary>
        private bool _isFetchingFreeNFTInfo;

        /// <summary>
        /// Called when new data of list nft is fetched from server. This can be when refetch list nft or fetch new nft page
        /// </summary>
        internal Action _onListNFTFetchSuccessCallback;

        /// <summary>
        /// Callback when user select new NFT or unselect a NFT.
        /// When unselect NFT, callback will be trigger with null or empty
        /// </summary>
        private Action<string> _onChangeSelectedHeroNftItemCallback;

        /// <summary>
        /// Callback when HeroNFTItem change its data.
        /// It occured when ending grinding session or refresh data of NFT
        /// </summary>
        private Action<HeroNftItem> _onChangeHeroNftDataCallback;

        /// <summary>
        /// Callback when change grinding status from grinding to not grinding and vice versa.
        /// The bool in callback is Grinding or not
        /// </summary>
        public Action<bool> _onChangeGrindingStatus;

        /// <summary>
        /// Callback when user have a victory match
        /// </summary>
        public Action _onReceiveVictoryMatch;

        /// <summary>
        /// Callback when service send grinding signal to server succeed
        /// </summary>
        public Action _onPingGrindSuccess;

        /// <summary>
        /// Called whe free nft is refreshed data
        /// </summary>
        public Action<FreeNFTInfo> _onRefreshFreeNFTInfo;

        /// <summary>
        /// NFT statistic data that has been fetch and cache locally
        /// </summary>
        private GrindNFTStatisticData _cacheNFTStatisticData;
        public GrindNFTStatisticData CacheNftStatisticData => _cacheNFTStatisticData;

        /// <summary>
        /// NFT daily summary data that has been fetch and cache locally
        /// </summary>
        private List<NFTItemDailySummaryData> _listItemNFTSummaryCached;
        public List<NFTItemDailySummaryData> ListItemNFTSummaryCached => _listItemNFTSummaryCached;

        /// <summary>
        /// Summary data of grinding in each game that has been fetch and cache locally
        /// </summary>
        private List<GameDailySummaryData> _listGameDailySummaryCached;
        public List<GameDailySummaryData> ListGameDailySummaryCached => _listGameDailySummaryCached;

        /// <summary>
        /// Current page number of _listHeroNftItems.
        /// Use when need to fetch next page of _listHeroNftItems
        /// </summary>
        private int _currentPageNumber;

        /// <summary>
        /// Is there no more unfetched page of _listHeroNftItems
        /// </summary>
        private bool _isNoMoreHeroItems;

        /// <summary>
        /// Is fetching _listHeroNftItems in progress
        /// </summary>
        private bool _isFetching;

        /// <summary>
        /// Is grinding pausing or not
        /// </summary>
        private bool _isAllowPingGrindingServer;
        internal bool IsAllowPingGrindingServer => _isAllowPingGrindingServer;

        private float _countTimePingGrindingServer;

        private float _countTimeRefreshDataWhenPauseGrind;

        /// <summary>
        /// Count consecutive error when send ping signal to server
        /// </summary>
        private int _countErrorPingGrindingServer;

        /// <summary>
        /// When end match is called but due to some reason (ex: temporally not connected to network)
        /// this field is turned on to try end match at Update function
        /// </summary>
        private bool _isPendingEndMatch;

        /// <summary>
        /// When end match is called but due to some reason (ex: temporally not connected to network)
        /// the end match info is cached for later used at Update function when it try to end pending match
        /// </summary>
        private EndMatchInfo _pendingEndMatchInfo;

        /// <summary>
        /// Count total seconds that has grind success at local machine, use for analytic only
        /// </summary>
        private float _countTimeGrindSuccess;

        /// <summary>
        /// Data local save in machine local files system contain service cached
        /// </summary>
        private DataLocalNftItemService _dataLocalNftItemService;

        private float _countTimeUpdateFreeNftData;

        /// <summary>
        /// Return if user is selecting any hero NFT
        /// </summary>
        public bool IsSelectedAnyHeroNFT() => !string.IsNullOrEmpty(_selectedHeroNftId);

        /// <summary>
        /// Check if user is in grinding session
        /// </summary>
        public bool IsGrinding() => !string.IsNullOrEmpty(_grindSessionId);

        /// <summary>
        /// Check if user is selecting hero NFT that has nftId
        /// </summary>
        public bool IsHeroNftSelected(string nftId)
        {
            if (string.IsNullOrEmpty(nftId) ||
                string.IsNullOrEmpty(_selectedHeroNftId))
                return false;

            return _selectedHeroNftId == nftId;
        }

        private void Awake()
        {
            _listHeroNftItems = new List<HeroNftItem>();
            _dicHeroNftIdToListIndex = new Dictionary<string, int>();
            _dicIngameHeroIdToListIndex = new Dictionary<string, int>();
        }

        /// <summary>
        /// Init service before it can be used
        /// </summary>
        public async UniTask InitService(NetworkClient networkClient)
        {
            _networkClient = networkClient;
            _grindingHeroNftId = string.Empty;

            if (PlayerPrefs.HasKey(SAVE_DATA_LOCAL_NFT_SERVICE_NAME))
            {
                var saveData = PlayerPrefs.GetString(SAVE_DATA_LOCAL_NFT_SERVICE_NAME);

                _dataLocalNftItemService = JsonConvert.DeserializeObject<DataLocalNftItemService>(saveData);
            }
            else
            {
                _dataLocalNftItemService = new DataLocalNftItemService();
            }

            _networkClient.SubcribeOnChangeNetworkClientState(OnChangeNetworkClientState);
            _networkClient.SubcribeOnReAuthenCallback(HandleReAuthen);

            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                _selectedHeroNftId = string.Empty;
                return;
            }

            _countTotalNFT = -1;

            UniTaskCompletionSource getSelectedHeroNftCompleteSource = new UniTaskCompletionSource();

            RefetchListHeroNFT(null, null);
            GetSelectedHeroNft(() =>
            {
                getSelectedHeroNftCompleteSource.TrySetResult();
            }, error =>
            {
                GetSelectedHeroNft(() =>
                {
                    getSelectedHeroNftCompleteSource.TrySetResult();
                }, (error) =>
                {
                    getSelectedHeroNftCompleteSource.TrySetResult();
                });
            });

            await getSelectedHeroNftCompleteSource.Task;

            UniTaskCompletionSource freeNFTInfoCompleteSource = new UniTaskCompletionSource();
            FetchFreeNFTInfo((freeNftInfo) =>
            {
                try
                {
                    if (!_dataLocalNftItemService.hasCheckUnselectNftWhenNotClaimFreeNft &&
                        string.IsNullOrEmpty(freeNftInfo.nftId) &&
                        !string.IsNullOrEmpty(_selectedHeroNftId))
                    {
                        if (_dicHeroNftIdToListIndex.TryGetValue(_selectedHeroNftId, out var index))
                        {
                            if (index >= 0 && index < _listHeroNftItems.Count)
                            {
                                DeselectHeroNft(_listHeroNftItems[index], null, null);
                            }
                        }

                        _dataLocalNftItemService.hasCheckUnselectNftWhenNotClaimFreeNft = true;
                        SaveDataLocal();
                    }
                }
                catch (Exception e)
                {
                    CommonLog.LogError($"Handle after fetch free nft info catch exception {e.Message}");
                }

                freeNFTInfoCompleteSource.TrySetResult();
            }, error =>
            {
                _freeNFtInfo = new FreeNFTInfo();
                freeNFTInfoCompleteSource.TrySetResult();
            });
            FetchFreeNFTConfig(null, null);

            await freeNFTInfoCompleteSource.Task;
        }

        /// <summary>
        /// Handle when network client state changed
        /// </summary>
        private void OnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.LoggedIn)
            {
                GetSelectedHeroNft(null, null);
                RefetchListHeroNFT(null, null);
                FetchFreeNFTInfo(null, null);
                FetchFreeNFTConfig(null, null);
            }
            else if (newState == ThetanNetworkClientState.NotLoggedIn ||
                     newState == ThetanNetworkClientState.NotLoggedInNoNetwork)
            {
                ClearDataService();
            }
        }

        private async void HandleReAuthen()
        {
            ClearDataService();

            RefetchListHeroNFT(null, null);
            GetSelectedHeroNft(() =>
            {
            }, error =>
            {
                GetSelectedHeroNft(() =>
                {
                }, (error) =>
                {
                });
            });
            FetchFreeNFTInfo(null, null);
            FetchFreeNFTConfig(null, null);
        }

        /// <summary>
        /// Clear cached data to when it's initialized
        /// </summary>
        public override void ClearDataService()
        {
            if (_listHeroNftItems == null)
                _listHeroNftItems = new List<HeroNftItem>();
            else
                _listHeroNftItems.Clear();

            if (_dicHeroNftIdToListIndex == null)
                _dicHeroNftIdToListIndex = new Dictionary<string, int>();
            else
                _dicHeroNftIdToListIndex.Clear();

            if (_dicIngameHeroIdToListIndex == null)
                _dicIngameHeroIdToListIndex = new Dictionary<string, int>();
            else
                _dicIngameHeroIdToListIndex.Clear();

            _selectedHeroNftId = string.Empty;
            _cacheNFTStatisticData = new GrindNFTStatisticData();

            if (_listGameDailySummaryCached != null)
                _listGameDailySummaryCached.Clear();

            if (_listItemNFTSummaryCached != null)
                _listHeroNftItems.Clear();

            _freeNFtInfo = new FreeNFTInfo();
        }

        private void Update()
        {
            /* Check is there any pending end match and available to call end match. If there is, call EndMatch */
            UpdatePendingEndMatch();

            UpdatePingGrinding();

            // When end play section of free nft, refresh data
            UpdateFreeNFT();
        }

        private void UpdatePendingEndMatch()
        {
            if (_isPendingEndMatch)
            {
                if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedIn)
                {
                    _isPendingEndMatch = false;
                    EndMatch(_pendingEndMatchInfo, null, null);
                }
                else if (_networkClient.NetworkClientState != ThetanNetworkClientState.LoggedInNoNetwork)
                {
                    _isPendingEndMatch = false;
                }
            }
        }

        private void UpdatePingGrinding()
        {
            if (string.IsNullOrEmpty(_grindingHeroNftId))
            {
                return;
            }

            /* If not connected no network, temporary pause grinding */
            if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
                return;

            /* If pausing grind, count time and refresh data selected NFT Id, grinding session Id, grinding NFT id */
            if (!_isAllowPingGrindingServer)
            {
                _countTimeRefreshDataWhenPauseGrind += Time.unscaledDeltaTime;

                if (_countTimeRefreshDataWhenPauseGrind >= REFRESH_DATA_WHEN_PAUSE_GRIND_INTERVAL)
                {
                    _countTimeRefreshDataWhenPauseGrind = 0;
                    GetSelectedHeroNft(null, null);
                }
                return;
            }

            

            /* Count time and send grinding signal every interval */
            _countTimePingGrindingServer += Time.unscaledDeltaTime;
            if (_countTimePingGrindingServer >= PING_GRINDING_SERVER_INTERVAL)
            {
                _countTimePingGrindingServer = 0;
                
                if (_dicHeroNftIdToListIndex.TryGetValue(_grindingHeroNftId, out var heroIndex) &&
                    heroIndex >= 0 && heroIndex < _listHeroNftItems.Count &&
                    _listHeroNftItems[heroIndex].id == _grindingHeroNftId)
                {
                    var heroInfo = _listHeroNftItems[heroIndex];
                    if (heroInfo.nftType == NFTType.NormalNFT)
                    {
                        if(heroInfo.grindInfo.grindTime >= heroInfo.grindInfo.maxGrindTime)
                        {
                            StopGrind();
                            return;
                        }
                    }
                    else if (heroInfo.nftType == NFTType.FreeNFT)
                    {
                        var curTime = DateTime.UtcNow;

                        if (curTime >= _freeNFtInfo.startSectionRest && curTime < _freeNFtInfo.nextResetGrindEarn)
                        {
                            StopGrind();
                            return;
                        }
                    }
                }
                
                PingServerGrinding();
            }
        }

        private void UpdateFreeNFT()
        {
            if (string.IsNullOrEmpty(_freeNFtInfo.nftId)) // Not claim free nft yet
                return;

            if (_isFetchingFreeNFTInfo)
                return;

            _countTimeUpdateFreeNftData += Time.unscaledDeltaTime;

            if (_countTimeUpdateFreeNftData >= 1)
            {
                _countTimeUpdateFreeNftData = 0;

                var currentTime = DateTime.UtcNow;

                if (_isInFreeNftPlaySection &&
                    currentTime >= _freeNFtInfo.nextResetGrindEarn)
                {
                    FetchFreeNFTInfo(null, null);
                    RefreshDataHeroNft(_freeNFtInfo.nftId, null, null);
                }
            }

        }

        private void SaveDataLocal()
        {
            PlayerPrefs.SetString(SAVE_DATA_LOCAL_NFT_SERVICE_NAME, JsonConvert.SerializeObject(_dataLocalNftItemService));
            PlayerPrefs.Save();
        }

        #region PUBLIC API

        /// <summary>
        /// Get HeroNFTId in _listHeroNftItems
        /// </summary>
        public HeroNftItem GetCachedHeroNftItemByIngameId(string ingameId)
        {
            if (string.IsNullOrEmpty(ingameId))
                return new HeroNftItem().SetDefault();

            if (_dicIngameHeroIdToListIndex.TryGetValue(ingameId, out var itemIndex))
            {
                if (itemIndex >= 0 && itemIndex < _listHeroNftItems.Count &&
                    _listHeroNftItems[itemIndex].ingameInfo.ingameID == ingameId)
                {
                    return _listHeroNftItems[itemIndex];
                }
            }

            return new HeroNftItem().SetDefault();
        }

        /// <summary>
        /// Fetch _selectedHeroNftId, _grindSessionId, _grindingHeroNftId
        /// </summary>
        public void GetSelectedHeroNft(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            ThetanWorldAPI.GetSelectedHeroNFT(selectedNftResponseModel =>
            {
                var prevSelectedHeroNftId = _selectedHeroNftId;
                _selectedHeroNftId = selectedNftResponseModel.selected;

                // if (_grindSessionId != null && _grindSessionId != selectedNftResponseModel.grindId)
                // {
                //     _onChangeGrindingStatus?.Invoke(!string.IsNullOrEmpty(selectedNftResponseModel.grindId));
                // }
                _grindSessionId = selectedNftResponseModel.grindId;
                _grindingHeroNftId = selectedNftResponseModel.nftGrind;

                if (prevSelectedHeroNftId != _selectedHeroNftId)
                    _onChangeSelectedHeroNftItemCallback?.Invoke(_selectedHeroNftId);

                onSuccessCallback?.Invoke();
            }, (error) =>
            {
                _selectedHeroNftId = string.Empty;
                _onChangeSelectedHeroNftItemCallback?.Invoke(_selectedHeroNftId);
                onErrorCallback?.Invoke(error);
            });
        }

        public void FetchFreeNFTInfo(Action<FreeNFTInfo> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            _isFetchingFreeNFTInfo = true;
            ThetanWorldAPI.FetchFreeNFTInfo(data =>
            {
                _freeNFtInfo = data;

                var currentTime = DateTime.UtcNow;

                if (currentTime <= _freeNFtInfo.nextResetGrindEarn)
                {
                    _isInFreeNftPlaySection = true;
                }
                else
                {
                    _isInFreeNftPlaySection = false;
                }

                _onRefreshFreeNFTInfo?.Invoke(_freeNFtInfo);
                onSuccessCallback?.Invoke(_freeNFtInfo);
                _isFetchingFreeNFTInfo = false;
            }, error =>
            {
                _isFetchingFreeNFTInfo = false;
                onErrorCallback?.Invoke(error);
            });
        }

        /// <summary>
        /// Register when selected nft hero changed
        /// </summary>
        public void RegisterOnChangeSelectedNftHeroCallback(Action<string> callback)
        {
            _onChangeSelectedHeroNftItemCallback += callback;
        }

        /// <summary>
        /// UnRegister when selected nft hero changed
        /// </summary>
        public void UnRegisterOnChangeSelectedNftHeroCallback(Action<string> callback)
        {
            _onChangeSelectedHeroNftItemCallback -= callback;
        }

        /// <summary>
        /// Register when hero nft change its data
        /// </summary>
        public void RegisterOnChangeNftItemData(Action<HeroNftItem> callback)
        {
            _onChangeHeroNftDataCallback += callback;
        }

        /// <summary>
        /// UnRegister when hero nft change its data
        /// </summary>
        public void UnRegisterOnChangeNftItemData(Action<HeroNftItem> callback)
        {
            _onChangeHeroNftDataCallback -= callback;
        }

        /// <summary>
        /// Clear _listHeroNftItems and refetch _listHeroNftItems at page 1
        /// </summary>
        public void RefetchListHeroNFT(Action<FetchListNftItemResponse> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (_listHeroNftItems == null)
                _listHeroNftItems = new List<HeroNftItem>();
            else
                _listHeroNftItems.Clear();

            if (_dicHeroNftIdToListIndex == null)
                _dicHeroNftIdToListIndex = new Dictionary<string, int>();
            else
                _dicHeroNftIdToListIndex.Clear();


            if (_dicIngameHeroIdToListIndex == null)
                _dicIngameHeroIdToListIndex = new Dictionary<string, int>();
            else
                _dicIngameHeroIdToListIndex.Clear();

            _currentPageNumber = 0;
            _isNoMoreHeroItems = false;

            FetchListNftData(GetCurrentListNFTSortType(), 1, onSuccessCallback, onErrorCallback);
        }

        /// <summary>
        /// Get user's total NFT count from server
        /// </summary>
        public void FetchTotalNFTCount(Action<int> result, Action<WolffunResponseError> error)
        {
            ThetanWorldAPI.FetchListNFT(null, 1, 0, listNftItems =>
            {
                _countTotalNFT = listNftItems.total;
                result?.Invoke(_countTotalNFT);
            }, error);

        }

        /// <summary>
        /// Fetch next page of _listHeroNftItems.
        /// Return data of next page and if there is any unfetched page
        /// </summary>
        public void FetchNextPageListHeroNft(Action<FetchListNftItemResponse> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (_isNoMoreHeroItems)
            {
                onSuccessCallback?.Invoke(new FetchListNftItemResponse()
                {
                    listNftHeroItems = null,
                    isNoMoreItem = true
                });
                return;
            }

            FetchListNftData(GetCurrentListNFTSortType(), 1, onSuccessCallback, onErrorCallback);
        }

        /// <summary>
        /// Call server to get new data of heroNftItem. Will trigger OnChangeHeroNftDataCallback
        /// </summary>
        public void RefreshDataHeroNft(HeroNftItem heroNftItem, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            RefreshDataHeroNft(heroNftItem.id, onSuccessCallback, onErrorCallback);
        }

        /// <summary>
        /// Call server to get new data of heroNftItem. Will trigger _onChangeHeroNftDataCallback
        /// </summary>
        public void RefreshDataHeroNft(string herNftId, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Client is not in Logged in state",
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN
                });
                return;
            }

            ThetanWorldAPI.GetHeroNftInfo(herNftId, newData =>
            {
                for (int i = 0; i < _listHeroNftItems.Count; i++)
                {
                    if (_listHeroNftItems[i].id == herNftId)
                    {
                        _listHeroNftItems[i] = newData;
                    }
                }

                _onChangeHeroNftDataCallback?.Invoke(newData);
                onSuccessCallback?.Invoke(newData);
            }, onErrorCallback);
        }

        /// <summary>
        /// Call server to get HeroNftItem of heroNftId. Will NOT trigger _onChangeHeroNftDataCallback
        /// </summary>
        public void GetInfoDataHeroNftOnServer(string heroNftId, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Client is not in Logged in state",
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN
                });
                return;
            }

            ThetanWorldAPI.GetHeroNftInfo(heroNftId, onSuccessCallback, onErrorCallback);
        }

        /// <summary>
        /// Call to select an HeroNFTItem. Will trigger OnChangeSelectedHeroNftItemCallback if succeed
        /// </summary>
        public void SelectHeroNft(HeroNftItem heroNftItem, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Client is not connected to network",
                    Code = (int)NftItemServiceErrorCode.NETWORK_ERROR
                });
                return;
            }

            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Client is not in Logged in state",
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN
                });
                return;
            }

            ThetanWorldAPI.SelectHeroNFT(heroNftItem.id, _ =>
            {
                var prevSelectedHeroId = _selectedHeroNftId;

                _selectedHeroNftId = heroNftItem.id;
                _onChangeSelectedHeroNftItemCallback?.Invoke(_selectedHeroNftId);
                onSuccessCallback?.Invoke(heroNftItem);
            }, onErrorCallback);
        }

        /// <summary>
        /// Call to unselect an HeroNFTItem. Will trigger OnChangeSelectedHeroNftItemCallback if succeed
        /// </summary>
        public void DeselectHeroNft(HeroNftItem heroNft, Action<HeroNftItem> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Client is not connected to network",
                    Code = (int)NftItemServiceErrorCode.NETWORK_ERROR
                });
                return;
            }

            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Client is not in Logged in state",
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN
                });
                return;
            }

            if (string.IsNullOrEmpty(_selectedHeroNftId))
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Selected hero doesn't have the same id as input parameter",
                    Code = (int)NftItemServiceErrorCode.SELECTED_HERO_ID_NOT_THE_SAME
                });
                return;
            }

            ThetanWorldAPI.DeSelectHeroNFT(heroNft.id, _ =>
            {
                _selectedHeroNftId = string.Empty;
                _onChangeSelectedHeroNftItemCallback?.Invoke(string.Empty);

                onSuccessCallback?.Invoke(heroNft);
            }, onErrorCallback);
        }

        /// <summary>
        /// Get detail grind info of heroNftItem. Will trigger OnChangeHeroNftDataCallback if heroNftItem is in cached data
        /// </summary>
        public void GetDetailGrindInfoHeroNft(HeroNftItem heroNftItem, Action<DetailHeroGrindInfo> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Message = "Client is not connected to network",
                    Code = (int)NftItemServiceErrorCode.NETWORK_ERROR
                });
                return;
            }

            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN,
                    Message = "NetworkClient is not in logged in state",
                });
                return;
            }

            ThetanWorldAPI.GetHeroDetailGrindInfo(heroNftItem.id, detailHeroGrindInfo =>
            {
                for (int i = 0; i < _listHeroNftItems.Count; i++)
                {
                    var heroNftInList = _listHeroNftItems[i];

                    if (heroNftInList.id == heroNftItem.id)
                    {
                        heroNftInList.grindInfo.status = detailHeroGrindInfo.status;
                        heroNftInList.grindInfo.grindPoint = detailHeroGrindInfo.grindPoint;
                        heroNftInList.grindInfo.maxGrindSpeed = detailHeroGrindInfo.maxGrindSpeed;
                        _listHeroNftItems[i] = heroNftInList;

                        _onChangeHeroNftDataCallback?.Invoke(heroNftInList);
                        break;
                    }
                }

                onSuccessCallback?.Invoke(detailHeroGrindInfo);
            }, onErrorCallback);
        }

        /// <summary>
        /// Call to prepare NFT for grinding
        /// </summary>
        public void StartMatch(int matchMaxDuration, Action<object> successCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NETWORK_ERROR,
                    Message = "Client is not connected to network.",
                });
                return;
            }

            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN,
                    Message = "Client is not in logged in state.",
                });
                return;
            }

            if (string.IsNullOrEmpty(_selectedHeroNftId))
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NOT_SELECTED_NFT_HERO,
                    Message = "Please select any nft hero before call start grind."
                });
                return;
            }

            if (_freeNFtInfo.nftId == _selectedHeroNftId)
            {
                var currentTime = DateTime.UtcNow;
                if(currentTime >= _freeNFtInfo.startSectionEarn &&
                   currentTime >= _freeNFtInfo.startSectionRest &&
                   currentTime <= _freeNFtInfo.nextResetGrindEarn)
                {
                    onErrorCallback?.Invoke(new WolffunResponseError()
                    {
                        Code = (int)NftItemServiceErrorCode.FREE_HERO_IN_REST_SESSION,
                        Message = "Free hero is in rest section, please select other hero."
                    });
                    return;
                }
            }

            ThetanWorldAPI.StartGrindingHeroNftItem(_selectedHeroNftId, matchMaxDuration, grindSessionId =>
            {
                _grindingHeroNftId = _selectedHeroNftId;
                _grindSessionId = grindSessionId;
                _countTimePingGrindingServer = 0;
                _countErrorPingGrindingServer = 0;
                _countTimeRefreshDataWhenPauseGrind = 0;
                _countTimeGrindSuccess = 0;

                ThetanSDKManager.Instance.AnalyticService.LogBattleFlow(_grindingHeroNftId, _grindSessionId,
                    SDKAnalyticService.BattleFlowStep.PrepareBattle);

                if (_selectedHeroNftId == _freeNFtInfo.nftId)
                {
                    FetchFreeNFTInfo(null, null);
                }

                successCallback?.Invoke(grindSessionId);
                _onChangeGrindingStatus?.Invoke(true);
            }, onErrorCallback);
        }

        /// <summary>
        /// Call to start send grind signal after prepare match is succeed
        /// </summary>
        public void StartGrind()
        {
            if (string.IsNullOrEmpty(_grindingHeroNftId))
                return;

            if (!_isAllowPingGrindingServer)
            {
                ThetanSDKManager.Instance.AnalyticService.LogBattleFlow(_grindingHeroNftId, _grindSessionId,
                    SDKAnalyticService.BattleFlowStep.StartGrind);
            }

            _isAllowPingGrindingServer = true;
        }

        /// <summary>
        /// Call to pause grinding
        /// </summary>
        public void StopGrind()
        {
            _countTimeRefreshDataWhenPauseGrind = 0;

            if (_isAllowPingGrindingServer &&
                !string.IsNullOrEmpty(_grindingHeroNftId))
            {
                ThetanSDKManager.Instance.AnalyticService.LogBattleFlow(_grindingHeroNftId, _grindSessionId,
                    SDKAnalyticService.BattleFlowStep.PauseGrind);
            }

            _isAllowPingGrindingServer = false;
        }

        /// <summary>
        /// Call to end grinding session and unlock NFT
        /// </summary>
        public void EndMatch(EndMatchInfo endMatchInfo, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _isAllowPingGrindingServer = false;

            if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onSuccessCallback?.Invoke();
                _isPendingEndMatch = true;
                _pendingEndMatchInfo = endMatchInfo;
                return;
            }

            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN,
                    Message = "Client is not in logged in state",
                });
                return;
            }

            if (string.IsNullOrEmpty(_grindingHeroNftId))
            {
                onSuccessCallback?.Invoke();
                return;
            }

            ThetanWorldAPI.StopGrindingHeroNftItem(_grindingHeroNftId, endMatchInfo, _ =>
            {
                if (endMatchInfo.matchResult == MatchResult.Win)
                {
                    _onReceiveVictoryMatch?.Invoke();    
                }
                
                RefreshDataHeroNft(_grindingHeroNftId, null, null);
                FetchFreeNFTInfo(null, null);
                
                ThetanSDKManager.Instance.AnalyticService.LogBattleFlow(_grindingHeroNftId, _grindSessionId,
                    SDKAnalyticService.BattleFlowStep.EndBattle, (int)_countTimeGrindSuccess, endMatchInfo.matchResult);
                ThetanSDKManager.Instance.UserStatisticService.FetchUserLeaderboardData(null, null);

                _countTimeGrindSuccess = 0;
                _grindingHeroNftId = string.Empty;
                _grindSessionId = string.Empty;

                if (_dicHeroNftIdToListIndex.TryGetValue(_grindingHeroNftId, out var nftIndex))
                {
                    if (nftIndex >= 0 && nftIndex < _listHeroNftItems.Count)
                    {
                        var nftHero = _listHeroNftItems[nftIndex];

                        nftHero.grindInfo.status = null;

                        _listHeroNftItems[nftIndex] = nftHero;

                        _onChangeHeroNftDataCallback?.Invoke(nftHero);
                    }
                }

                onSuccessCallback?.Invoke();
                _onChangeGrindingStatus?.Invoke(false);
            }, error =>
            {
                ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("End Battle", "End Grind NFT",
                    false,
                    (NftItemServiceErrorCode)error.Code == NftItemServiceErrorCode.UNKNOWN ? error.DevDebugMessage : error.Message);
                _grindingHeroNftId = string.Empty;
                _grindSessionId = string.Empty;
                onErrorCallback?.Invoke(error);
                _onChangeGrindingStatus?.Invoke(false);
            });
        }

        /// <summary>
        /// Fetch and cache grind statistic data
        /// </summary>
        public void FetchGrindNFTStatisticData(Action<GrindNFTStatisticData> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            ThetanWorldAPI.FetchGrindStatisticData(data =>
            {
                _cacheNFTStatisticData = data;
                onSuccessCallback?.Invoke(_cacheNFTStatisticData);
            }, onErrorCallback);
        }

        public void ClaimFreeNFT(Action<HeroNftItem> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            ThetanWorldAPI.ClaimFreeNFT(freeNft =>
            {
                RefetchListHeroNFT(_ =>
                {
                    SelectHeroNft(freeNft, null, null);
                }, null);
                
                _freeNFtInfo.nftId = freeNft.id;
                onSuccessCallback?.Invoke(freeNft);
            }, error =>
            {
                if ((NftItemServiceErrorCode)error.Code == NftItemServiceErrorCode.FREE_NFT_CLAIMED)
                {
                    RefetchListHeroNFT(null, null);
                    FetchFreeNFTInfo(null, null);
                }

                onErrorCallback?.Invoke(error);
            });
        }

        public void FetchFreeNFTConfig(Action<FreeNFTConfig> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            ThetanWorldAPI.GetFreeNFTConfig(freeNft =>
                {
                    _freeNFTConfig = freeNft;
                    onSuccessCallback?.Invoke(freeNft);
                },
                error => onErrorCallback?.Invoke(error));
        }
        #endregion

        #region Internal API Functions
        /// <summary>
        /// Fetch and cache daily nft item summary data
        /// </summary>
        internal void FetchDailyNFTItemSummary(Action<NFTItemDailySummaryDataReponse> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            ThetanWorldAPI.GetDailySummaryNFTItem(reponse =>
            {
                if (reponse.data == null)
                    _listItemNFTSummaryCached = new List<NFTItemDailySummaryData>();
                else
                    _listItemNFTSummaryCached = reponse.data;

                onSuccessCallback?.Invoke(reponse);
            }, onErrorCallback);
        }

        /// <summary>
        /// Fetch and cache daily game grinding summary data
        /// </summary>
        internal void FetchDailyGameSummary(Action<GameDailySummaryDataReponse> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            ThetanWorldAPI.GetDailySummaryGame(reponse =>
            {
                if (reponse.data == null)
                    _listGameDailySummaryCached = new List<GameDailySummaryData>();
                else
                    _listGameDailySummaryCached = reponse.data;

                onSuccessCallback?.Invoke(reponse);
            }, onErrorCallback);
        }

        /// <summary>
        /// Send grinding signal to server
        /// </summary>
        private void PingServerGrinding()
        {
            ThetanWorldAPI.PingGrindingServer(_grindingHeroNftId, PING_GRINDING_SERVER_INTERVAL, _ =>
            {
                _countTimeGrindSuccess += PING_GRINDING_SERVER_INTERVAL;

                if (_dicHeroNftIdToListIndex.TryGetValue(_grindingHeroNftId, out var itemIndex) &&
                    itemIndex >= 0 && itemIndex < _listHeroNftItems.Count &&
                    _listHeroNftItems[itemIndex].id == _grindingHeroNftId)
                {
                    // We temporary update data local to serve UI grinding progress.
                    // We will sync data with server when end grind. So this temporary data only affect UI during grind time
                    var nftItem = _listHeroNftItems[itemIndex];
                    nftItem.grindInfo.grindTime += PING_GRINDING_SERVER_INTERVAL;

                    nftItem.grindInfo.grindTime =
                        Mathf.Clamp(nftItem.grindInfo.grindTime, 0, nftItem.grindInfo.maxGrindTime);

                    _listHeroNftItems[itemIndex] = nftItem;
                }
                
                _onPingGrindSuccess?.Invoke();
            }, HandlePingServerGrindingError);
        }

        /// <summary>
        /// Handle when send grinding signal has error
        /// </summary>
        private void HandlePingServerGrindingError(WolffunResponseError error)
        {
            ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("Ingame", "Ping Grind NFT", false,
                (NftItemServiceErrorCode)error.Code == NftItemServiceErrorCode.UNKNOWN ? error.DevDebugMessage : error.Message);

            _countErrorPingGrindingServer++;

            /* Accept 3 consecutive signal error */
            if (_countErrorPingGrindingServer < 3)
            {
                // Do nothing
                return;
            }

            NftItemServiceErrorCode errorCode = (NftItemServiceErrorCode)error.Code;
            switch (errorCode)
            {
                case NftItemServiceErrorCode.HERO_NOT_GRINDING:
                    {
                        /* User is not in grinding session anymore, call fetch _grindSessionId and _grindingHeroNftId again */
                        _isAllowPingGrindingServer = false;
                        GetSelectedHeroNft(() =>
                        {
                            if (!string.IsNullOrEmpty(_grindingHeroNftId))
                            {
                                _isAllowPingGrindingServer = true;
                            }
                        }, null);
                        break;
                    }
                default:
                    {
                        /* When there is 3 consecutive error, temporary pause grinding */
                        _isAllowPingGrindingServer = false;
                        break;
                    }
            }
        }

        /// <summary>
        /// Get list sort criteria
        /// </summary>
        private List<NFTSortType> GetCurrentListNFTSortType()
        {
            if (_currentSortTypeIndex < 0 || _currentSortTypeIndex >= _listSortType.Count)
                return null;

            return _listSortType[_currentSortTypeIndex].Item2;
        }

        /// <summary>
        /// Fetch list HeroNFTItem
        /// </summary>
        private async void FetchListNftData(List<NFTSortType> listSort, int numberPageFetch,
            Action<FetchListNftItemResponse> onDoneCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NETWORK_ERROR,
                    Message = "Client is not connected",
                });
                return;
            }

            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NOT_LOGGED_IN,
                    Message = "NetworkClient is not in logged in state",
                });
                return;
            }

            if (_isFetching)
                await UniTask.WaitUntil(() => !_isFetching);

            _isFetching = true;

            if (_listHeroNftItems == null)
                _listHeroNftItems = new List<HeroNftItem>();

            var nextPage = _currentPageNumber + 1;

            ThetanWorldAPI.FetchListNFT(listSort, NUMBER_ITEM_PER_PAGE * numberPageFetch, nextPage, listNftItems =>
            {
                _currentPageNumber += numberPageFetch;
                if (listNftItems.data != null)
                {
                    //for (int i = listNftItems.data.Count - 1; i >=0; i--)
                    //{
                    //    if(listNftItems.data[i] == null)
                    //        listNftItems.data.RemoveAt(i);
                    //}

                    _isNoMoreHeroItems = listNftItems.data.Count < NUMBER_ITEM_PER_PAGE;
                    _countTotalNFT = listNftItems.total;

                    for (int i = 0; i < listNftItems.data.Count; i++)
                    {
                        var nftItem = listNftItems.data[i];

                        if (_dicHeroNftIdToListIndex.TryGetValue(nftItem.id, out var cachedItemIndex))
                        {
                            _listHeroNftItems[cachedItemIndex] = nftItem;
                        }
                        else
                        {
                            _dicHeroNftIdToListIndex[nftItem.id] = _listHeroNftItems.Count;
                            _dicIngameHeroIdToListIndex[nftItem.ingameInfo.ingameID] = _listHeroNftItems.Count;
                            _listHeroNftItems.Add(nftItem);
                        }
                    }

                    onDoneCallback?.Invoke(new FetchListNftItemResponse()
                    {
                        listNftHeroItems = listNftItems.data,
                        isNoMoreItem = _isNoMoreHeroItems
                    });
                }
                else
                {
                    _countTotalNFT = 0;
                    _isNoMoreHeroItems = true;
                    onDoneCallback?.Invoke(new FetchListNftItemResponse()
                    {
                        listNftHeroItems = null,
                        isNoMoreItem = true
                    });
                }

                _onListNFTFetchSuccessCallback?.Invoke();
                _isFetching = false;
            }, error =>
            {
                _isFetching = false;
                onErrorCallback?.Invoke(error);
            });
        }
        #endregion
    }
}

