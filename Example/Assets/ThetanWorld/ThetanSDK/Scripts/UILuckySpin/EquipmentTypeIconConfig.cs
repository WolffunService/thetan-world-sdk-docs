using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThetanSDK.UI.LuckySpin
{
    [CreateAssetMenu(fileName = "EquipmentTypeIconConfig", menuName = "Wolffun/Configs/EquipmentTypeIconConfig")]
    internal class EquipmentTypeIconConfig : ScriptableObject
    {
        [Serializable]
        private struct Config
        {
            public EquipmentItemType equipmentType;
            public Sprite sprIconEquipment;
        }

        [SerializeField] private List<Config> configs;

        public Sprite GetSpriteIconEquipment(EquipmentItemType equipmentType)
        {
            if (configs == null || configs.Count == 0)
                return null;

            foreach (var config in configs)
            {
                if (config.equipmentType == equipmentType)
                    return config.sprIconEquipment;
            }

            return null;
        }
    }
}