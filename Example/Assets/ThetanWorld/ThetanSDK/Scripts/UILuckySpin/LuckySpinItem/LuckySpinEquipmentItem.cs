using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI.LuckySpin
{
    internal class LuckySpinEquipmentItem : LuckySpinItem
    {
        [SerializeField] private Image _imgIcon;
        [SerializeField] private EquipmentTypeIconConfig _equipmentTypeIconConfig;
        
        public override void ShowUI(LuckySpinItemUIData data)
        {
            if (data.itemType != LuckySpinItemType.Equipment)
                return;

            _txtItemName.text = data.equipmentType.ToString();
            _imgIcon.sprite = _equipmentTypeIconConfig.GetSpriteIconEquipment(data.equipmentType);
        }
    }
}