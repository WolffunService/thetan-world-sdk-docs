using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wolffun.RestAPI.ThetanWorld
{
    public static class ThetanWorldAPI
    {
        public static void FetchListNFT(List<NFTSortType> listSort, int limit, int page, Action<ListHeroNftItems> result,
            Action<WolffunResponseError> error)
        {
            string queryStr = string.Empty;

            if (listSort != null && listSort.Count > 0)
            {
                var strBuilder = Utils.StringBuilderPool.Get();
                for (int i = 0; i < listSort.Count; i++)
                {
                    strBuilder.Append((int)listSort[i]);

                    if (i != listSort.Count - 1)
                    {
                        strBuilder.Append(",");
                    }
                }

                queryStr = strBuilder.ToString();
                Utils.StringBuilderPool.Release(strBuilder);
            }
            
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/user/nfts")
                .GetQuery(Utils.GetProperties(new RequestNFTListRequestModel()
                {
                    limit = limit,
                    page = page,
                    sorts = queryStr,
                    mountStatus = true,
                }));

            WolffunUnityHttp.Instance.MakeAPI(reqCommon, result, error, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        public static void GetSelectedHeroNFT(Action<SelectedNftResponseModel> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/user/selected-nft")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, onSuccessCallback, onErrorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }
        
        public static void SelectHeroNFT(string nftId, Action<object> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/nfts/{nftId}/select")
                .Post(WolffunRequestBody.From(new
                {
                }));
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, successCallback, errorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }
        
        public static void DeSelectHeroNFT(string nftId, Action<object> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/user/nft-unselect")
                .Post(WolffunRequestBody.From(new
                {
                }));
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, successCallback, errorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        public static void GetHeroNftInfo(string nftId, Action<HeroNftItem> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/nfts/{nftId}")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, successCallback, errorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }
        
        public static void GetHeroDetailGrindInfo(string nftId, Action<DetailHeroGrindInfo> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/nfts/{nftId}/grind")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, successCallback, errorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        public static void StartGrindingHeroNftItem(string nftId, int timeOutGrind, Action<string> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/nfts/{nftId}/grind/prepare")
                .Post(WolffunRequestBody.From(new
                {
                    timeOutGrind = timeOutGrind,
                }));
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, successCallback, errorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        public static void PingGrindingServer(string nftId, int grindTimeIncrease, Action<object> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/nfts/{nftId}/grind/sync")
                .Post(WolffunRequestBody.From(new
                {
                    grindTimeInc = grindTimeIncrease
                }));
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, successCallback, errorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        public static void StopGrindingHeroNftItem(string nftId, Action<object> successCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/nfts/{nftId}/grind/end")
                .Post(WolffunRequestBody.From(new
                {
                }));
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, successCallback, errorCallback, AuthType.TOKEN_AND_CLIENT_SECRET);
        }

        public static void GetDailySummaryNFTItem(Action<NFTItemDailySummaryDataReponse> resultCallback,
            Action<WolffunResponseError> errorCallback)
        {
            // Empty
        }
        
        public static void GetDailySummaryGame(Action<GameDailySummaryDataReponse> resultCallback,
            Action<WolffunResponseError> errorCallback)
        {
            // Empty
        }

        public static void FetchGrindStatisticData(Action<GrindNFTStatisticData> resultCallback,
            Action<WolffunResponseError> errorCallback)
        {
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + $"/user/statistic")
                .Get();
            
            WolffunUnityHttp.Instance.MakeAPI(reqCommon, resultCallback, errorCallback, AuthType.TOKEN);
        }
    }
}

