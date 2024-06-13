using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;
using UnityEngine.UI;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using Com.TheFallenGames.OSA.Util.PullToRefresh;
using Cysharp.Threading.Tasks;
using frame8.Logic.Misc.Other.Extensions;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.Utilities;
using TMPro;
using UnityEditor;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
	public class ListNftAdapter : GridAdapter<NFTCardViewsCellGroupParams, NFTCardViewsHolder>
	{
		[SerializeField] private int _numberItemPerPage;
		[SerializeField] private TextMeshProUGUI _txtErrorMsg;
		[SerializeField] private PullToRefreshBehaviour _pullToRefreshBehaviour;
		[SerializeField] private TextMeshProUGUI _txtListEmpty;
		[SerializeField] private Button _btnBuyNFT;
		[SerializeField] private TMP_Dropdown _dropdownSelectSort;
		[SerializeField] private PullToRefreshGizmo _RefreshGizmo;
		[SerializeField] private RectTransform _prefabHeaderText;

		private bool _isDoneInitializeData;
		private bool _isFetching;
		private bool _isNoMoreData;

		private int _countFetchDataError = 0;

		private SpriteCacheManager _spriteCacheManager;
		
		public SimpleDataHelper<HeroNftCardViewDataModel> Data { get; private set; }
		private Dictionary<string, int> _dicHeroNftIdToListIndex;

		private Action<HeroNftItem> _onSelectItem;

		public void RegisterOnSelectItem(Action<HeroNftItem> callback)
		{
			_onSelectItem += callback;
		}
		
		public void UnregisterOnSelectItem(Action<HeroNftItem> callback)
		{
			_onSelectItem -= callback;
		}

		public void SetSpriteCacheManager(SpriteCacheManager spriteCacheManager)
		{
			_spriteCacheManager = spriteCacheManager;
		}

		protected override void Awake()
		{
			base.Awake();
			
			_btnBuyNFT.onClick.AddListener(OnClickBuyNFT);
		}

		private void OnClickBuyNFT()
		{
			Application.OpenURL(ThetanSDKManager.Instance.RemoteConfigService.RemoteConfig.mkpUrlConfig.urlTabBuyNFT);
		}

		#region GridAdapter implementation
		protected override async void Start()
		{
			Data = new SimpleDataHelper<HeroNftCardViewDataModel>(this);
			_dicHeroNftIdToListIndex = new Dictionary<string, int>();

			_dropdownSelectSort.options.Clear();
			var nftItemService = ThetanSDKManager.Instance.NftItemService;
			foreach (var sortType in nftItemService.ListSortType)
			{
				_dropdownSelectSort.options.Add(new TMP_Dropdown.OptionData()
				{
					text = sortType.Item1
				});
			}
			_dropdownSelectSort.value = nftItemService.CurrentSortTypeIndex;

			_dropdownSelectSort.onValueChanged.AddListener(HandleOnChangeSortType);
			
			base.Start();
		}
		
		protected override void Update()
		{
			base.Update();

			if (Input.GetKeyDown(KeyCode.M))
			{
				for (int i = 0; i < 20; i++)
				{
					Data.InsertOneAtEnd(new HeroNftCardViewDataModel()
					{
						IsHeader = false,
					});
				}
				Data.NotifyListChangedExternally();
				return;
			}
			
			if (!IsInitialized)
				return;

			if (!_isDoneInitializeData)
				return;

			if (_isFetching)
				return;

			if (_isNoMoreData)
				return;

			if (VisibleItemsCount == 0)
				return;
			
			int lastVisibleItemitemIndex = -1;
			if (VisibleItemsCount > 0)
			{
				var groupViewsHolder = GetItemViewsHolder(VisibleItemsCount - 1);
				lastVisibleItemitemIndex = groupViewsHolder.ContainingCellViewsHolders[groupViewsHolder.NumActiveCells - 1]
					.ItemIndex;
			}
			int numberOfItemsBelowLastVisible = Data.Count - (lastVisibleItemitemIndex + 1);
			
			if (numberOfItemsBelowLastVisible < _numberItemPerPage / 2)
			{
				_isFetching = true;
				ThetanSDKManager.Instance.NftItemService.FetchNextPageListHeroNft((newData) =>
				{
					_isNoMoreData = newData.isNoMoreItem;
					
					var insertIndex = Data.Count - 1;
					if (insertIndex < 0)
						insertIndex = 0;

					if (newData.listNftHeroItems != null)
					{
						for (int i = 0; i < newData.listNftHeroItems.Count; i++)
						{
							var nftItem = newData.listNftHeroItems[i];
							if (_dicHeroNftIdToListIndex.TryGetValue(nftItem.id, out var dataIndex))
							{
								var item = Data.List[dataIndex];
								
								item.HeroNftItem = nftItem;
								Data.List[dataIndex] = item;
							}
							else
							{
								_dicHeroNftIdToListIndex[nftItem.id] = Data.Count;
								Data.List.Add(new HeroNftCardViewDataModel()
								{
									IsHeader = false,
									HeroNftItem = nftItem,
									HasPendingSizeChange = true,
								});
							}
						}
					}
					
					Data.NotifyListChangedExternally();
					
					_isFetching = false;

					_countFetchDataError = 0;
				}, error =>
				{
					_isFetching = false;
					HandleFetchDataError(error);
				});
				// FetchData(1, (newData) =>
				// {
				// 	var insertIndex = Data.Count - 1;
				// 	if (insertIndex < 0)
				// 		insertIndex = 0;
				// 	Data.InsertItems(insertIndex, newData);
				// });
			}
		}

		protected override void UpdateCellViewsHolder(NFTCardViewsHolder newOrRecycled)
		{
			var data = Data[newOrRecycled.ItemIndex];

			if (data.IsHeader)
			{
				newOrRecycled.UpdateHeader();
				return;
			}

			var nftItemService = ThetanSDKManager.Instance.NftItemService;
			newOrRecycled.UpdateData(data.HeroNftItem, data.HeroNftItem.id == nftItemService.SelectedHeroNftId,
				_spriteCacheManager,
				(selectedItem) =>
			{
				_onSelectItem?.Invoke(selectedItem);
			});
		}

		protected override CellGroupViewsHolder<NFTCardViewsHolder> CreateViewsHolder(int itemIndex)
		{
			var instance = new NFTCardViewsCellGroupViewsHolder();

			instance.Init(_Params.GetGroupPrefab(itemIndex).gameObject, _Params.Content, itemIndex, 
				_Params.Grid.CellPrefab, _Params.CurrentUsedNumCellsPerGroup);

			for (int i = 0; i < instance.ContainingCellViewsHolders.Length; i++)
			{
				var cellVH = instance.ContainingCellViewsHolders[i];
				OnCellViewsHolderCreated(cellVH, instance);
			}

			return instance;
		}

		protected override float UpdateItemSizeOnTwinPass(CellGroupViewsHolder<NFTCardViewsHolder> viewsHolder)
		{
			if (viewsHolder.NumActiveCells > 0)
			{
				var firstCellVH = viewsHolder.ContainingCellViewsHolders[0];
				int modelIndex = firstCellVH.ItemIndex;
				var model = Data[modelIndex];
				viewsHolder.root.SetSizeFromParentEdgeWithCurrentAnchors(_Params.Content,
					RectTransform.Edge.Top,
					_Params.Grid.CellPrefab.sizeDelta.y);
			}

			return base.UpdateItemSizeOnTwinPass(viewsHolder);
		}
		#endregion
		
		
		private void HandleOnChangeSortType(int sortIndex)
		{
			_pullToRefreshBehaviour.ShowGizmoIfNeeded();
			
			_RefreshGizmo.OnPull(1d);
			_RefreshGizmo.OnRefreshed(false);

			ThetanSDKManager.Instance.NftItemService.CurrentSortTypeIndex = sortIndex;
			
			PullToRefresh();
		}

		public async void PullToRefresh()
		{
			_Params.ScrollEnabled = false;
			_Params.DragEnabled = false;
			await UniTask.Delay(300, ignoreTimeScale: true);
			
			_Params.ScrollEnabled = true;
			_Params.DragEnabled = true;
			// refresh selected hero nft
			ThetanSDKManager.Instance.NftItemService.GetSelectedHeroNft(null, null);
			ThetanSDKManager.Instance.EquipmentService.FetchListItem(null, null);
			RefetchNewData();
		}
		
		public void ChangeHeroNftData(HeroNftItem newData)
		{
			for (int i = 0; i < Data.Count; i++)
			{
				if (!Data[i].IsHeader &&
					Data[i].HeroNftItem.id == newData.id)
				{
					var item = Data.List[i];
					item.HeroNftItem = newData;
					Data.List[i] = item;
					break;
				}
			}
			
			Data.NotifyListChangedExternally();
		}
		
		public void UpdateSelectedItemId()
		{
			var nftItemService = ThetanSDKManager.Instance.NftItemService;
			if (VisibleItemsCount > 0)
			{
				for (int i = 0; i < VisibleItemsCount; i++)
				{
					var groupViewsHolder = GetItemViewsHolder(i);

					for (int cellIdx = 0; cellIdx < groupViewsHolder.NumActiveCells; cellIdx++)
					{
						var itemIndex = groupViewsHolder.ContainingCellViewsHolders[cellIdx].ItemIndex;

						var cellData = Data[itemIndex];

						if(cellData.IsHeader)
							continue;

						groupViewsHolder.ContainingCellViewsHolders[cellIdx].UpdateData(cellData.HeroNftItem, 
							cellData.HeroNftItem.id == nftItemService.SelectedHeroNftId,
							_spriteCacheManager,
							(selectedItem) =>
							{
								_onSelectItem?.Invoke(selectedItem);
							});
					}
				}
			}
		}

		public void ClearData()
		{
			if (!IsInitialized)
				return;
			
			Data.ResetItems(new List<HeroNftCardViewDataModel>());

			if (_dicHeroNftIdToListIndex == null)
				_dicHeroNftIdToListIndex = new Dictionary<string, int>();
			else
				_dicHeroNftIdToListIndex.Clear();
			
			_isFetching = false;
			_isNoMoreData = false;
		}

		public async void InitializeWithCacheData()
		{
			if(_txtErrorMsg)
			{
				_txtErrorMsg.enabled = false;
				_txtErrorMsg.text = string.Empty;
			}
			
			if (!IsInitialized)
				await UniTask.WaitUntil(() => IsInitialized);
			
			ClearData();

			var nftService = ThetanSDKManager.Instance.NftItemService;
			
			if(nftService.ListHeroNftItems != null)
			{
				// Generate empty items to preserve space for header
				// for (int i = 0; i < _Params.CurrentUsedNumCellsPerGroup; i++)
				// {
				// 	Data.List.Add(new HeroNftCardViewDataModel()
				// 	{
				// 		IsHeader = true,
				// 		HasPendingSizeChange = true,
				// 	});
				// }
					
				for (int i = 0; i < nftService.ListHeroNftItems.Count; i++)
				{
					var nftItem = nftService.ListHeroNftItems[i];
					if (_dicHeroNftIdToListIndex.TryGetValue(nftItem.id, out var dataIndex))
					{
						var item = Data.List[dataIndex];
						item.HeroNftItem = nftItem;
						Data.List[dataIndex] = item;
					}
					else
					{
						_dicHeroNftIdToListIndex[nftItem.id] = Data.Count;
						Data.List.Add(new HeroNftCardViewDataModel()
						{
							IsHeader = false,
							HeroNftItem = nftItem,
							HasPendingSizeChange = true,
						});
					}
				}
			}
			
			_txtListEmpty.enabled = Data.Count == 0;
			bool isEnableBtnBuyNFT =
				ThetanSDKManager.Instance.RemoteConfigService.RemoteConfig.mkpUrlConfig.enableBtnBuyNFT;
			_btnBuyNFT.gameObject.SetActive(isEnableBtnBuyNFT && Data.Count == 0);
			
			Data.NotifyListChangedExternally();
		}

		public async void RefetchNewData()
		{
			if (!IsInitialized)
				await UniTask.WaitUntil(() => IsInitialized);

			_countFetchDataError = 0;
			
			if(_txtErrorMsg)
			{
				_txtErrorMsg.enabled = false;
				_txtErrorMsg.text = string.Empty;
			}

			_txtListEmpty.enabled = false;
			_btnBuyNFT.gameObject.SetActive(false);
			
			_pullToRefreshBehaviour.ShowGizmoIfNeeded();
			_RefreshGizmo.OnPull(1d);
			_RefreshGizmo.OnRefreshed(false);
			
			ClearData();

			_isFetching = true;
			ThetanSDKManager.Instance.NftItemService.RefetchListHeroNFT((newData) =>
			{
				_pullToRefreshBehaviour.HideGizmo();
				
				_isDoneInitializeData = true;

				_isNoMoreData = newData.isNoMoreItem;
				
				if(newData.listNftHeroItems != null)
				{
					// Generate empty items to preserve space for header
					// for (int i = 0; i < _Params.CurrentUsedNumCellsPerGroup; i++)
					// {
					// 	Data.List.Add(new HeroNftCardViewDataModel()
					// 	{
					// 		IsHeader = true,
					// 		HasPendingSizeChange = true,
					// 	});
					// }
					
					for (int i = 0; i < newData.listNftHeroItems.Count; i++)
					{
						var nftItem = newData.listNftHeroItems[i];
						if (_dicHeroNftIdToListIndex.TryGetValue(nftItem.id, out var dataIndex))
						{
							var item = Data.List[dataIndex];
							item.HeroNftItem = nftItem;
							Data.List[dataIndex] = item;
						}
						else
						{
							_dicHeroNftIdToListIndex[nftItem.id] = Data.Count;
							Data.List.Add(new HeroNftCardViewDataModel()
							{
								IsHeader = false,
								HeroNftItem = nftItem,
								HasPendingSizeChange = true,
							});
						}
					}
				}
				
				Data.NotifyListChangedExternally();

				_txtListEmpty.enabled = Data.Count == 0;
				bool isEnableBtnBuyNFT =
					ThetanSDKManager.Instance.RemoteConfigService.RemoteConfig.mkpUrlConfig.enableBtnBuyNFT;
				_btnBuyNFT.gameObject.SetActive(isEnableBtnBuyNFT && Data.Count == 0);
				
				_isFetching = false;
				_countFetchDataError = 0;
			}, error =>
			{
				_pullToRefreshBehaviour.HideGizmo();
				_isFetching = false;
				ShowMessageErrorFetchData(error);
			});
			
			// FetchData(1, (newData) =>
			// {
			// 	_isDoneInitializeData = true;
			// 	
			// 	Data.InsertItems(0, newData);
			// });
		}

		private void HandleFetchDataError(WolffunResponseError error)
		{
			_countFetchDataError++;

			if (_countFetchDataError >= 3)
			{
				_isNoMoreData = true;

				ShowMessageErrorFetchData(error);
			}
		}

		private void ShowMessageErrorFetchData(WolffunResponseError error)
		{
			if (error.Code == (int)WSErrorCode.ServerMaintenance)
			{
				ThetanSDKManager.Instance.HandleMaintenance();
				return;
			}
			
			if(_txtErrorMsg)
			{
				_txtErrorMsg.enabled = true;

				if (error.Code == (int)WSErrorCode.UserBanned)
				{
					_txtErrorMsg.text = AuthenErrorMsg.AccountBannedContactSupport;
					return;
				}

				switch ((NftItemServiceErrorCode)error.Code)
				{
					case NftItemServiceErrorCode.NETWORK_ERROR:
						_txtErrorMsg.text = UINftErrorMsg.ERROR_NO_CONNECTION;
						break;
					default:
						_txtErrorMsg.text = UINftErrorMsg.ERROR_FETCH_LIST_NFT_HERO;
						break;
				}
				
			}
		}
	}

	public class HeroNftCardViewDataModel
	{
		public bool IsHeader;
		public HeroNftItem HeroNftItem;
		public bool HasPendingSizeChange;
	}
	
	public class NFTCardViewsHolder : CellViewsHolder
	{
		private UIHeroNftCard _uiHeroNftCard;
		private LayoutElement _layoutElement;
		private HeroNftItem _heroNftItem;
		
		public override void CollectViews()
		{
			base.CollectViews();

			_uiHeroNftCard = root.GetComponent<UIHeroNftCard>();
			_layoutElement = root.GetComponent<LayoutElement>();
		}

		public void UpdateHeader()
		{
			if (_layoutElement)
				_layoutElement.ignoreLayout = true;
			
			views.gameObject.SetActive(false);
		}
		
		public void UpdateData(HeroNftItem data, bool isSelected, SpriteCacheManager spriteCacheManager, Action<HeroNftItem> onSelectCallback)
		{
			_heroNftItem = data;

			if (_layoutElement)
				_layoutElement.ignoreLayout = false;
			
			views.gameObject.SetActive(true);
			
			if(_uiHeroNftCard)
			{
				_uiHeroNftCard.SetSpriteCacheManager(spriteCacheManager);
				_uiHeroNftCard.SetData(data, isSelected, () => onSelectCallback?.Invoke(_heroNftItem),
					() => { ThetanSDKManager.Instance.NftItemService.RefreshDataHeroNft(data, null, null); });
			}
		}

		public void UpdateIsSelected(bool isSelected)
		{
			if(_uiHeroNftCard)
				_uiHeroNftCard.ChangeIsSelected(isSelected);
		}
	}

	[Serializable]
	public class NFTCardViewsCellGroupParams : GridParams
	{
		[SerializeField] private GameObject _cellGroupPrefab;
		
		protected override GameObject CreateCellGroupPrefabGameObject()
		{
			// Already created
			return _cellGroupPrefab;
		}
	}
	
	public class NFTCardViewsCellGroupViewsHolder : CellGroupViewsHolder<NFTCardViewsHolder>
	{
		private NFTGroupUI _headerText;

		public override void CollectViews()
		{
			base.CollectViews();

			_headerText = root.GetComponent<NFTGroupUI>();
		}
	}
}
