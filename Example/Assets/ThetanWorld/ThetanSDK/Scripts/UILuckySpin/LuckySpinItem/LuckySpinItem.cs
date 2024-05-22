using TMPro;
using UnityEngine;

namespace ThetanSDK.UI.LuckySpin
{
    public enum EquipmentItemType
    {
        Hat = 1,
        Shirt = 2,
        Glove = 3,
        Pant = 4,
        Shoe = 5,
        Weapon = 6,
    }

    internal enum LuckySpinItemType
    {
        Empty = 0,
        Equipment = 1,
        Ticket = 2,
    }
    
    internal struct LuckySpinItemUIData
    {
        public LuckySpinItemType itemType;
        
        /// <summary>
        /// This data only available for LuckySpinItemType.Equipment
        /// </summary>
        public EquipmentItemType equipmentType;
    }

    internal abstract class LuckySpinItem : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _txtItemName;

        public abstract void ShowUI(LuckySpinItemUIData data);
    }
}