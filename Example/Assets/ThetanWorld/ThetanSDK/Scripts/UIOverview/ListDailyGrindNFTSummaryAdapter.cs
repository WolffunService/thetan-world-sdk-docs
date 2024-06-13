using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using Com.TheFallenGames.OSA.Util.PullToRefresh;
using Cysharp.Threading.Tasks;
using Wolffun.RestAPI.ThetanWorld;

// You should modify the namespace to your own or - if you're sure there won't ever be conflicts - remove it altogether
namespace ThetanSDK.UI
{
	// There are 2 important callbacks you need to implement, apart from Start(): CreateViewsHolder() and UpdateViewsHolder()
	// See explanations below
	public class ListDailyGrindNFTSummaryAdapter : OSA<BaseParamsWithPrefab, DailySummaryNftItemViewsHolder>
	{
		[SerializeField] private PullToRefreshBehaviour _pullToRefreshBehaviour;
		
		// Helper that stores data and notifies the adapter when items count changes
		// Can be iterated and can also have its elements accessed by the [] operator
		public SimpleDataHelper<NFTItemDailySummaryData> Data { get; private set; }

		private bool _isFetchDataBefore = false;
		
		
		#region OSA implementation
		protected override void Start()
		{
			Data = new SimpleDataHelper<NFTItemDailySummaryData>(this);

			base.Start();
		}

		protected override DailySummaryNftItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			var instance = new DailySummaryNftItemViewsHolder();

			instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

			return instance;
		}
		
		protected override void UpdateViewsHolder(DailySummaryNftItemViewsHolder newOrRecycled)
		{
			newOrRecycled.SetData(Data[newOrRecycled.ItemIndex]);
		}
		#endregion

		public async void RefreshData()
		{
			await UniTask.Delay(500, ignoreTimeScale: true);
			
			ThetanSDKManager.Instance.NftItemService.FetchDailyNFTItemSummary(newData =>
			{
				Data.ResetItems(newData.data);
				_pullToRefreshBehaviour.HideGizmo();
			}, error =>
			{
				_pullToRefreshBehaviour.HideGizmo();
			});
		}

		public async void ShowUI()
		{
			if (!IsInitialized)
				await UniTask.WaitUntil(() => IsInitialized);
			
			if (ThetanSDKManager.Instance.NftItemService.ListItemNFTSummaryCached != null)
			{
				Data.ResetItems(ThetanSDKManager.Instance.NftItemService.ListItemNFTSummaryCached);
				
				if(Data.Count > 0)
					ScrollTo(0);
			}

			if (!_isFetchDataBefore)
			{
				ThetanSDKManager.Instance.NftItemService.FetchDailyNFTItemSummary(newData =>
				{
					Data.ResetItems(newData.data);
				}, error =>
				{
				
				});

				_isFetchDataBefore = true;
			}
		}

		public void ClearData()
		{
			_isFetchDataBefore = false;
		}
	}
	
	// This class keeps references to an item's views.
	// Your views holder should extend BaseItemViewsHolder for ListViews and CellViewsHolder for GridViews
	public class DailySummaryNftItemViewsHolder : BaseItemViewsHolder
	{
		private NFTItemSummaryInfoUI _nftItemSummaryInfoUI;

		// Retrieving the views from the item's root GameObject
		public override void CollectViews()
		{
			base.CollectViews();

			_nftItemSummaryInfoUI = root.GetComponent<NFTItemSummaryInfoUI>();
		}

		public void SetData(NFTItemDailySummaryData data)
		{
			if(_nftItemSummaryInfoUI)
				_nftItemSummaryInfoUI.SetData(data);
		}
	}
}
