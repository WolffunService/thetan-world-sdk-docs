using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.Foundation.Pooling;
using ZBase.Foundation.Pooling.UnityPools;

namespace ThetanSDK.Utilities.Pooling
{
    public class MultipleGameObjectPool : IPool, IShareable
    {
        private readonly Dictionary<int, GameObjectItemPool> _pools = new();
        private readonly Dictionary<int, GameObjectItemPool> _dicTrackingInstancePools = new();
        private readonly Dictionary<int, GameObjectPrefab> _poolKeyCache = new();

        private Transform _parent;

        public MultipleGameObjectPool(Transform parent)
        {
            _parent = parent;
        }

        public async UniTask<GameObject> Rent(GameObject gameObjectReference)
        {
            var hash = gameObjectReference.GetInstanceID();
            if (!_poolKeyCache.TryGetValue(hash, out var key))
                this._poolKeyCache.Add(hash, key = new GameObjectPrefab { Source = gameObjectReference, Parent = _parent});
            return await Rent(key);
        }
        
        public async UniTask<GameObject> Rent(GameObjectPrefab gameObjectReference)
        {
            var instanceID = gameObjectReference.Source.GetInstanceID();
            if (!_pools.TryGetValue(instanceID, out var pool))
            {
                if (gameObjectReference.Source.scene.IsValid())
                    throw new Exception($"Non Prefab not supported {gameObjectReference.Source.name}");
                pool = new GameObjectItemPool(gameObjectReference);
                pool.OnReturnAction += RemoveTrackingItem;
                pool.OnPoolEmpty += OnPoolEmpty;
                this._pools.Add(instanceID, pool);
            }
            GameObject item = await pool.Rent();
            this._dicTrackingInstancePools.Add(item.GetInstanceID(), pool);
            return item;
        }


        public void Return(GameObject gameObject)
        {
            if (!gameObject)
                return;
            if (this._dicTrackingInstancePools.TryGetValue(gameObject.GetInstanceID(), out var pool))
                pool.Return(gameObject);
            else
                Debug.LogWarning(
                    $"GameObject {gameObject.name} is not registered in the pool or was already returned.");
        }

        public void Return(GameObjectPrefab gameObjectReference, GameObject gameObject)
        {
            if (_pools.TryGetValue(gameObjectReference.Source.GetInstanceID(), out var pool))
                pool.Return(gameObject);
        }

        public void ReleaseInstances(int keep, Action<GameObject> onReleased = null)
        {
            foreach (var pool in _pools.Values)
                pool.ReleaseInstances(keep, onReleased);
        }

        private void OnPoolEmpty(GameObjectItemPool pool)
        {
            pool.Dispose();
            this._pools.Remove(pool.ID);
            RemoveReference(pool.ID);
            RemoveCacheKey(pool.ID);
        }
        
        private void RemoveReference(int poolID)
        {
            foreach (var keyPair in this._dicTrackingInstancePools)
            {
                if (keyPair.Value.ID != poolID)
                    continue;
                this._dicTrackingInstancePools.Remove(keyPair.Key);
                Debug.LogWarning($"Pool {poolID} is empty and removed from tracking");
                break;
            }
        }
   
        private void RemoveCacheKey(int poolID)
        {
            foreach (var keyPair in  _poolKeyCache)
            {
                if (keyPair.Value.Source.GetInstanceID() != poolID)
                    continue;
                this._poolKeyCache.Remove(keyPair.Key);
                break;
            }
        }
        
        private void RemoveTrackingItem(GameObject gameObject) =>
            this._dicTrackingInstancePools.Remove(gameObject.GetInstanceID());

        public void Dispose()
        {
            foreach (var pool in _pools.Values)
                pool.Dispose();
            _pools.Clear();
            _dicTrackingInstancePools.Clear();
            _poolKeyCache.Clear();
        }
    }
}