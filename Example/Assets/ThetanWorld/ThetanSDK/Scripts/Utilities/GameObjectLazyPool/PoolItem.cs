using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.Foundation.Pooling.UnityPools;

namespace ThetanSDK.Utilities.Pooling
{
    public class PoolItem<TPrefab> : MonoBehaviour where TPrefab : IPrefab<GameObject>
    {
        private float _lifeTime;
        private UnityPool<GameObject, TPrefab> _pool;

        public void SetUp(UnityPool<GameObject, TPrefab> pool) => _pool = pool;

        protected virtual void OnDestroy() {}
    }
}