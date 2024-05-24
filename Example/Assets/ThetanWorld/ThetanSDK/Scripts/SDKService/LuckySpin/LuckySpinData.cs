using System;
using System.Collections.Generic;
using ThetanSDK.UI.LuckySpin;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.LuckySpin
{
    /// <summary>
    /// Contain lucky spin data of user
    /// </summary>
    [Serializable]
    public struct LuckySpinData : ICustomDefaultable<LuckySpinData>
    {
        /// <summary>
        /// Total lucky spin numer user has
        /// </summary>
        public int spinChance;
        
        /// <summary>
        /// How many grind second user has.
        /// When currentGrindSecs reach grindSecsPerSpin, user receive 1 spinChance and currentGrindSecs is reset
        /// </summary>
        public int currentGrindSecs;
        
        /// <summary>
        /// How many second user has to grind NFT to receive 1 spinChance
        /// </summary>
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

    /// <summary>
    /// Lucky spin config data
    /// </summary>
    [Serializable]
    public struct LuckySpinConfig : ICustomDefaultable<LuckySpinConfig>
    {
        /// <summary>
        /// All reward in lucky spin wheel
        /// </summary>
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

    /// <summary>
    /// Item that in lucky spin wheel
    /// </summary>
    [Serializable]
    public struct LuckySpinItemConfig
    {
        /// <summary>
        /// This is empty item, when this bool is true, all other field is set to detault
        /// </summary>
        public bool isGoodLuck;
        
        /// <summary>
        /// Equipment type, only available when kind == 6
        /// </summary>
        public EquipmentItemType equipmentType;
        
        /// <summary>
        /// Contain item kind when item is not isGoodLuck.
        /// Equipment: 6
        /// Ticket: 1
        /// </summary>
        public int kind;
        
        /// <summary>
        /// Contain detail item type id
        /// </summary>
        public int type;

        /// <summary>
        /// Is this item an equipment
        /// </summary>
        public bool IsEquipment() => kind == 6;

        /// <summary>
        /// Is this item a ticket
        /// </summary>
        public bool IsTicket() => kind == 1;
    }
}