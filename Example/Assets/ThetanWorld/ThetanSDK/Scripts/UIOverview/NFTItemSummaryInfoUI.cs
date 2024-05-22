using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using ThetanSDK.UI;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class NFTItemSummaryInfoUI : MonoBehaviour
    {
        [SerializeField] private NFTHeroAvatar _nftHeroAvatar;
        [SerializeField] private TextMeshProUGUI _txtNFTName;
        [SerializeField] private TextMeshProUGUI _txtGrindTime;
        [SerializeField] private TextMeshProUGUI _txtGrindReward;

        public void SetData(NFTItemDailySummaryData data)
        {
            _txtNFTName.text = data.metaData.name;
            _nftHeroAvatar.ShowUI(data.ingameInfo);
            _txtGrindTime.text = data.grindInfo.grindTime.ToStringTime();
            _txtGrindReward.SetText(data.grindInfo.grindPoint);
        }
    }
}
