using System;
using System.Collections.Generic;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKServices.Equipment
{
    /// <summary>
    /// Contain string define status of equipment
    /// </summary>
    [Serializable]
    internal class EquipmentStatus
    {
        public static string EquipmentStatusDefault = string.Empty;
        public static string EquipmentStatusUsed = "used";
        public static string EquipmentStatusDeleted = "deleted";
    }

    /// <summary>
    /// A custom list that implement ICustomDefaultable
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// User equipment data
    /// </summary>
    [Serializable]
    internal struct EquipmentItemData : ICustomDefaultable<EquipmentItemData>
    {
        /// <summary>
        /// Id of equipment
        /// </summary>
        public string id;
        
        /// <summary>
        /// Owner id of this equipment
        /// </summary>
        public string userId;
        
        /// <summary>
        /// ItemKindID of equipment
        /// </summary>
        public int kind;
        
        /// <summary>
        /// Equipment Type Id
        /// </summary>
        public int type;
        
        /// <summary>
        /// Total amount of this equipment user has
        /// </summary>
        public int amount;
        
        /// <summary>
        /// Other properties
        /// </summary>
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

    /// <summary>
    /// Equipment properties
    /// </summary>
    [Serializable]
    internal struct EquipmentProps
    {
        /// <summary>
        /// Number of star of this equipment
        /// </summary>
        public int stars;
        
        /// <summary>
        /// Status of this equipment, define by class EquipmentStatus
        /// </summary>
        public string status;
    }
}