using System;
using System.Collections.Generic;
using ThetanSDK.UI.LuckySpin;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.LuckySpin
{
    [Serializable]
    public struct LuckySpinData : ICustomDefaultable<LuckySpinData>
    {
        public int spinChance;
        public int currentGrindSecs;
        public int grindSecsPerSpin;
        
        public LuckySpinData SetDefault()
        {
            spinChance = 0;
            currentGrindSecs = 0;
            grindSecsPerSpin = 0;
            return this;
        }

        public bool IsEmpty()
        {
            return grindSecsPerSpin == 0 && currentGrindSecs == 0;
        }
    }

    [Serializable]
    public struct LuckySpinConfig : ICustomDefaultable<LuckySpinConfig>
    {
        public List<LuckySpinItemConfig> rewardUIs;
        
        public LuckySpinConfig SetDefault()
        {
            rewardUIs = null;
            return this;
        }

        public bool IsEmpty()
        {
            return rewardUIs == null;
        }
    }

    [Serializable]
    public struct LuckySpinItemConfig
    {
        public bool isGoodLuck;
        public EquipmentItemType equipmentType;
        public int kind;
        public int type;

        public bool IsEquipment() => kind == 6;

        public bool IsTicket() => kind == 1;
    }
}