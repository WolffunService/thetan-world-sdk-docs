using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService;
using ThetanSDK.SDKServices.Analytic;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;
using Random = System.Random;

namespace ThetanSDK.SDKServices.NFTItem
{
    internal class NftItemService : BaseClassService
    {
        private const int NUMBER_ITEM_PER_PAGE = 150;
        private const int PING_GRINDING_SERVER_INTERVAL = 5;
        private const int REFRESH_DATA_WHEN_PAUSE_GRIND_INTERVAL = 20;
        
        private List<(string, List<NFTSortType>)> _listSortType = new List<(string, List<NFTSortType>)>()
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

        private int _currentSortTypeIndex = 0;
        
        public int CurrentSortTypeIndex
        {
            get => _currentSortTypeIndex;
            set => _currentSortTypeIndex = value;
        }

        private NetworkClient _networkClient;
        
        private List<HeroNftItem> _listHeroNftItems;
        public List<HeroNftItem> ListHeroNftItems => _listHeroNftItems;

        private int _countTotalNFT = -1;

        public int CountTotalNFT => _countTotalNFT;

        private Dictionary<string, int> _dicHeroNftIdToListIndex;
        private Dictionary<string, int> _dicIngameHeroIdToListIndex;

        private string _selectedHeroNftId;
        public string SelectedHeroNftId => _selectedHeroNftId;

        private string _grindingHeroNftId;
        public string GrindingHeroNftId => _grindingHeroNftId;

        private string _grindSessionId;
        public string GrindSessionId => _grindSessionId;
        
        private Action<string> _onChangeSelectedHeroNftItemCallback;
        private Action<HeroNftItem> _onChangeHeroNftDataCallback;
        public Action<bool> _onChangeGrindingStatus;
        public Action _onPingGrindSuccess;

        private GrindNFTStatisticData _cacheNFTStatisticData;
        public GrindNFTStatisticData CacheNftStatisticData => _cacheNFTStatisticData;

        private List<NFTItemDailySummaryData> _listItemNFTSummaryCached;
        public List<NFTItemDailySummaryData> ListItemNFTSummaryCached => _listItemNFTSummaryCached;

        private List<GameDailySummaryData> _listGameDailySummaryCached;
        public List<GameDailySummaryData> ListGameDailySummaryCached => _listGameDailySummaryCached;

        private int _currentPageNumber;

        private bool _isNoMoreHeroItems;
        
        private bool _isFetching;

        private bool _isAllowPingGrindingServer;
        internal bool IsAllowPingGrindingServer => _isAllowPingGrindingServer;
        
        private float _countTimePingGrindingServer;

        private float _countTimeRefreshDataWhenPauseGrind;

        private int _countErrorPingGrindingServer;

        private bool _isPendingEndMatch;

        private float _countTimeGrindSuccess;

        public bool IsSelectedAnyHeroNFT() => !string.IsNullOrEmpty(_selectedHeroNftId);

        public bool IsGrinding() => !string.IsNullOrEmpty(_grindSessionId);
        
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

        public async UniTask InitService(NetworkClient networkClient)
        {
            _networkClient = networkClient;
            _grindingHeroNftId = string.Empty;
            
            _networkClient.SubcribeOnChangeNetworkClientState(OnChangeNetworkClientState);
            
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
        }

        private void OnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.LoggedIn)
            {
                GetSelectedHeroNft(null, null);
                RefetchListHeroNFT(null, null);
            }
        }

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
            
            if(_listGameDailySummaryCached != null) 
                _listGameDailySummaryCached.Clear();
            
            if(_listItemNFTSummaryCached != null)
                _listHeroNftItems.Clear();
            
            if(_networkClient)
                _networkClient.SubcribeOnChangeNetworkClientState(OnChangeNetworkClientState);
        }

        private void Update()
        {
            if (_isPendingEndMatch)
            {
                if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedIn)
                {
                    _isPendingEndMatch = false;
                    EndMatch(null, null);
                }
                else if (_networkClient.NetworkClientState != ThetanNetworkClientState.LoggedInNoNetwork)
                {
                    _isPendingEndMatch = false;
                }
            }
            
            if (string.IsNullOrEmpty(_grindingHeroNftId))
            {
                return;
            }
            
            if(_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork) // Pause grind
                return;

            if (!_isAllowPingGrindingServer)
            {
                _countTimeRefreshDataWhenPauseGrind += Time.deltaTime;

                if (_countTimeRefreshDataWhenPauseGrind >= REFRESH_DATA_WHEN_PAUSE_GRIND_INTERVAL)
                {
                    _countTimeRefreshDataWhenPauseGrind = 0;
                    GetSelectedHeroNft(null, null);
                }
                return;
            }

            _countTimePingGrindingServer += Time.unscaledDeltaTime;

            if (_countTimePingGrindingServer >= PING_GRINDING_SERVER_INTERVAL)
            {
                _countTimePingGrindingServer = 0;
                PingServerGrinding();
            }
        }

        #region PUBLIC API

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
                                
                if(prevSelectedHeroNftId != _selectedHeroNftId)
                    _onChangeSelectedHeroNftItemCallback?.Invoke(_selectedHeroNftId);

                onSuccessCallback?.Invoke();
            }, (error) =>
            {
                _selectedHeroNftId = string.Empty;
                _onChangeSelectedHeroNftItemCallback?.Invoke(_selectedHeroNftId);
                onErrorCallback?.Invoke(error);
            });
        }
        
        public void RegisterOnChangeSelectedNftHeroCallback(Action<string> callback)
        {
            _onChangeSelectedHeroNftItemCallback += callback;
        }

        public void UnRegisterOnChangeSelectedNftHeroCallback(Action<string> callback)
        {
            _onChangeSelectedHeroNftItemCallback -= callback;
        }
        
        public void RegisterOnChangeNftItemData(Action<HeroNftItem> callback)
        {
            _onChangeHeroNftDataCallback += callback;
        }

        public void UnRegisterOnChangeNftItemData(Action<HeroNftItem> callback)
        {
            _onChangeHeroNftDataCallback -= callback;
        }

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

        public void FetchTotalNFTCount(Action<int> result, Action<WolffunResponseError> error)
        {
            ThetanWorldAPI.FetchListNFT(null, 1, 0, listNftItems =>
            {
                _countTotalNFT = listNftItems.total;
                result?.Invoke(_countTotalNFT);
            }, error);

        }
        
        public void FetchNextPageListHeroNft(Action<FetchListNftItemResponse> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if(_isNoMoreHeroItems)
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

        public void RefreshDataHeroNft(HeroNftItem heroNftItem, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            RefreshDataHeroNft(heroNftItem.id, onSuccessCallback, onErrorCallback);
        }

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

        public void StartMatch(int matchMaxDuration, Action<object> successCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NETWORK_ERROR,
                    Message = "Client is not connected to network",
                });
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
            
            if (string.IsNullOrEmpty(_selectedHeroNftId))
            {
                onErrorCallback?.Invoke(new WolffunResponseError()
                {
                    Code = (int)NftItemServiceErrorCode.NOT_SELECTED_NFT_HERO,
                    Message = "Please select any nft hero before call start grind"
                });
                return;
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
                
                successCallback?.Invoke(grindSessionId);
                _onChangeGrindingStatus?.Invoke(true);
            }, onErrorCallback);
        }

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

        public void EndMatch(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _isAllowPingGrindingServer = false;

            if (_networkClient.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onSuccessCallback?.Invoke();
                _isPendingEndMatch = true;
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
            
            ThetanWorldAPI.StopGrindingHeroNftItem(_grindingHeroNftId, _ =>
            {
                RefreshDataHeroNft(_grindingHeroNftId, null, null);
                
                ThetanSDKManager.Instance.AnalyticService.LogBattleFlow(_grindingHeroNftId, _grindSessionId,
                    SDKAnalyticService.BattleFlowStep.EndBattle, (int)_countTimeGrindSuccess);

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

        public void FetchGrindNFTStatisticData(Action<GrindNFTStatisticData> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            ThetanWorldAPI.FetchGrindStatisticData(data =>
            {
                _cacheNFTStatisticData = data;
                onSuccessCallback?.Invoke(_cacheNFTStatisticData);
            }, onErrorCallback);
        }
        
        #endregion

        #region Internal API Functions
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
            },  onErrorCallback);
        }

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
            },  onErrorCallback);
        }
        
        private void PingServerGrinding()
        {
            ThetanWorldAPI.PingGrindingServer(_grindingHeroNftId, PING_GRINDING_SERVER_INTERVAL, _ =>
            {
                _countTimeGrindSuccess += PING_GRINDING_SERVER_INTERVAL;
                _onPingGrindSuccess?.Invoke();
            }, HandlePingServerGrindingError);
        }

        private void HandlePingServerGrindingError(WolffunResponseError error)
        {
            ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("Ingame", "Ping Grind NFT", false,
                (NftItemServiceErrorCode)error.Code == NftItemServiceErrorCode.UNKNOWN ? error.DevDebugMessage : error.Message);
            
            _countErrorPingGrindingServer++;

            if (_countErrorPingGrindingServer < 3)
            {
                // Ping lỗi vài lần không sao
                return;
            }
            NftItemServiceErrorCode errorCode = (NftItemServiceErrorCode)error.Code;
            switch (errorCode)
            {
                case NftItemServiceErrorCode.HERO_NOT_GRINDING:
                {
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
                    // Todo: Hỏi lại anh Hoàng xem default error handle như thế nào
                    _isAllowPingGrindingServer = false;
                    break;
                }
            }
        }

        private List<NFTSortType> GetCurrentListNFTSortType()
        {
            if (_currentSortTypeIndex < 0 || _currentSortTypeIndex >= _listSortType.Count)
                return null;

            return _listSortType[_currentSortTypeIndex].Item2;
        }
        
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
                        isNoMoreItem =  _isNoMoreHeroItems
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

