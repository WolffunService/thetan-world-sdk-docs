using UnityEngine;

namespace Wolffun.Tweening
{
    public class WorldMovePositionTweener : Vector3Tweener
    {
        private Transform _transform;

        public void SetUp(Transform transform, Vector3 endPosition, float duration)
        {
            _transform = transform;

            SetUp(endPosition, duration);
        }

        protected override Vector3 GetCurrentValue()
        {
            if (!_transform)
                return default;
            
            return _transform.position;
        }

        protected override void SetValue(Vector3 currentValue)
        {
            if (!_transform)
                return;

            _transform.position = currentValue;
        }

        public override void ResetValue()
        {
            base.ResetValue();

            _transform = null;
        }
    }
}