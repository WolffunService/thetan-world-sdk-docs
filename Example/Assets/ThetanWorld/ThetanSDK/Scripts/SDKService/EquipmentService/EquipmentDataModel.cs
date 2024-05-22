using System;
using System.Collections.Generic;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKServices.Equipment
{
    [Serializable]
    internal class EquipmentStatus
    {
        public static string EquipmentStatusDefault = string.Empty;
        public static string EquipmentStatusUsed = "used";
        public static string EquipmentStatusDeleted = "deleted";
    }

    [Serializable]
    internal class CustomList<T> : List<T>, ICustomDefaultable<CustomList<T>>
    {
        public CustomList<T> SetDefault()
        {
            this.Clear();
            return this;
        }

        public bool IsEmpty()
        {
            return this.Count == 0;
        }
    }

    [Serializable]
    internal struct EquipmentItemData : ICustomDefaultable<EquipmentItemData>
    {
        public string id;
        public string userId;
        public int kind;
        public int type;
        public int amount;
        public EquipmentProps props;
        
        public EquipmentItemData SetDefault()
        {
            id = string.Empty;
            userId = string.Empty;
            return this;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId);
        }
    }

    [Serializable]
    internal struct EquipmentProps
    {
        public int stars;
        public string status;
    }
}