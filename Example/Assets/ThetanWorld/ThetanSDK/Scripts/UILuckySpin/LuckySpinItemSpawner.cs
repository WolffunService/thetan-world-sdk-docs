using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThetanSDK.UI.LuckySpin
{
    internal class LuckySpinItemSpawner : MonoBehaviour
    {
        [Serializable]
        private struct PrefabConfig
        {
            public LuckySpinItemType itemType;
            public LuckySpinItem prefabItem;
        }

        [SerializeField] private List<PrefabConfig> _listPrefabConfigs = new();
        [SerializeField] private RectTransform _parentTransform;

        private List<LuckySpinItem> _listInstanceItem = new List<LuckySpinItem>();

        public void SpawnListItems(List<LuckySpinItemUIData> listItemData)
        {
            DespawnAllItems();

            foreach (var itemData in listItemData)
            {
                var prefab = GetPrefabLuckySpinItem(itemData.itemType);

                if (prefab == null)
                    continue;

                var instance = Instantiate(prefab, _parentTransform);
                
                instance.transform.SetAsLastSibling();
                instance.ShowUI(itemData);
                
                _listInstanceItem.Add(instance);
            }
        }

        private LuckySpinItem GetPrefabLuckySpinItem(LuckySpinItemType itemType)
        {
            foreach (var config in _listPrefabConfigs)
            {
                if (config.itemType == itemType)
                    return config.prefabItem;
            }

            return null;
        }

        public void DespawnAllItems()
        {
            if (_listInstanceItem != null)
            {
                foreach (var instance in _listInstanceItem)
                {
                    Destroy(instance.gameObject);
                }
                _listInstanceItem.Clear();
            }
        }
    }
}