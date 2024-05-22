using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.Foundation.Pooling.UnityPools;

namespace ThetanSDK.Utilities.Pooling
{
    public sealed class GameObjectItemPool : GameObjectPool
    {
        internal event Action<GameObjectItemPool> OnPoolEmpty;
        internal event Action<GameObject> OnReturnAction;
        private readonly List<int > _poolItems = new();

        internal int ID { get; }

        public GameObjectItemPool(GameObjectPrefab prefab) : base(prefab) => ID = Prefab.Source.GetInstanceID();

        protected override async UniTask RentPostprocess(GameObject instance, CancellationToken cancelToken)
        {
            await base.RentPostprocess(instance, cancelToken);
            
            if (!instance.TryGetComponent<GameObjectPoolItem>(out var poolItem))
            {
                poolItem = instance.AddComponent<GameObjectPoolItem>();
                poolItem.SetUp(this);
                poolItem.OnItemDestroy += OnItemDestroy;
            }
            
            this._poolItems.Add(instance.GetInstanceID());
        }

        protected override void ReturnPreprocess(GameObject instance)
        {
            base.ReturnPreprocess(instance);
            OnReturnAction?.Invoke(instance);
        }

        private void OnItemDestroy(GameObject instance)
        {
            this._poolItems.Remove(instance.GetInstanceID());
            if(this._poolItems.Count == 0)
                OnPoolEmpty?.Invoke(this);
        }
    }
}