using UnityEngine;

namespace Wolffun.Tweening
{
    public sealed class LocalScaleTweener : Vector3Tweener
    {
        private Transform _targetTransform;

        public LocalScaleTweener SetUp(Transform transform, Vector3 endValue, float duration)
        {
            _targetTransform = transform;
            SetUp(endValue, duration);
            return this;
        }

        protected override Vector3 GetCurrentValue()
        {
            if (!_targetTransform)
                return default;

            return _targetTransform.localScale;
        }

        protected override void SetValue(Vector3 currentValue)
        {
            if (!_targetTransform)
                return;

            _targetTransform.localScale = currentValue;
        }

        public override void ResetValue()
        {
            base.ResetValue();

            _targetTransform = null;
        }
    }
}