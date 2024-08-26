using System;
using System.Collections;
using System.Collections.Generic;

namespace Wolffun.Tweening
{
    public abstract class TweenInstanceCache
    {
        public abstract TweenerCore Rent();
        public abstract void Return(TweenerCore instance);
    }
    
    public class TweenInstanceCache<T> : TweenInstanceCache
        where T : TweenerCore, new()
    {
        private Stack<T> _cachedPool;
        public Type SupportedTweenType { get; }

        public TweenInstanceCache()
        {
            SupportedTweenType = typeof(T);

            _cachedPool = new Stack<T>();
        }
        
        

        private T InternalRent()
        {
            if (_cachedPool.Count > 0)
            {
                var t = _cachedPool.Pop();
                t.ResetValue();
                return t;
            }

            var newInstance = new T();
            newInstance.ResetValue();

            return newInstance;
        }

        private void InternalReturn(T instance)
        {
            if (_cachedPool.Contains(instance))
                return;
            
            instance.ResetValue();
            _cachedPool.Push(instance);
        }

        public override TweenerCore Rent()
        {
            return InternalRent();
        }

        public override void Return(TweenerCore instance)
        {
            if (instance.GetType() != SupportedTweenType)
                return;
            
            InternalReturn(instance as T);
        }
    }
}