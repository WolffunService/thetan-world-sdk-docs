using System;
using UnityEngine;
using ZBase.Foundation.Pooling.UnityPools;
namespace ThetanSDK.Utilities.Pooling
{
    public class GameObjectPoolItem : PoolItem<GameObjectPrefab>
    {
        public event Action<GameObject> OnItemDestroy;
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.OnItemDestroy?.Invoke(this.gameObject);
        }
    }
}