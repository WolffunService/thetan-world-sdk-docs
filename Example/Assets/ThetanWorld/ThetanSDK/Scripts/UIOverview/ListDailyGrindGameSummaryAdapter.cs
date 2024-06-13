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
	public class ListDailyGrindGameSummaryAdapter : OSA<BaseParamsWithPrefab, DailySummaryGameViewsHolder>
	{
		[SerializeField] private PullToRefreshBehaviour _pullToRefreshBehaviour;
		
		// Helper that stores data and notifies the adapter when items count changes
		// Can be iterated and can also have its elements accessed by the [] operator
		public SimpleDataHelper<GameDailySummaryData> Data { get; private set; }

		private bool _isFetchDataBefore;


		#region OSA implementation
		protected override void Start()
		{
			Data = new SimpleDataHelper<GameDailySummaryData>(this);

			base.Start();
		}

		protected override DailySummaryGameViewsHolder CreateViewsHolder(int itemIndex)
		{
			var instance = new DailySummaryGameViewsHolder();

			instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

			return instance;
		}
		
		protected override void UpdateViewsHolder(DailySummaryGameViewsHolder newOrRecycled)
		{
			newOrRecycled.SetData(Data[newOrRecycled.ItemIndex]);
		}
		#endregion

		public async void RefreshData()
		{
			await UniTask.Delay(500, ignoreTimeScale: true);
			
			ThetanSDKManager.Instance.NftItemService.FetchDailyGameSummary(newData =>
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
			
			if (ThetanSDKManager.Instance.NftItemService.ListGameDailySummaryCached != null)
			{
				Data.ResetItems(ThetanSDKManager.Instance.NftItemService.ListGameDailySummaryCached);
				
				if(Data.Count > 0)
					ScrollTo(0);
			}

			if (!_isFetchDataBefore)
			{
				ThetanSDKManager.Instance.NftItemService.FetchDailyGameSummary(newData =>
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
	public class DailySummaryGameViewsHolder : BaseItemViewsHolder
	{
		private GameSummaryInfoUI _nftItemSummaryInfoUI;

		// Retrieving the views from the item's root GameObject
		public override void CollectViews()
		{
			base.CollectViews();

			_nftItemSummaryInfoUI = root.GetComponent<GameSummaryInfoUI>();
		}

		public void SetData(GameDailySummaryData data)
		{
			if(_nftItemSummaryInfoUI)
				_nftItemSummaryInfoUI.SetData(data);
		}
	}
}
