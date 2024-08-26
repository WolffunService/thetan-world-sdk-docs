using UnityEngine;

namespace Wolffun.Tweening
{
    public abstract class Vector3Tweener : Tweener
    {
        private Vector3 _startValue;
        protected Vector3 _endValue;
        private Vector3 _changedValue;

        protected void SetUp(Vector3 endValue, float duration)
        {
            _endValue = endValue;
            SetDuration(duration);
        }
        
        protected override void OnStartTween()
        {
            base.OnStartTween();

            _startValue = GetCurrentValue();
            _changedValue = _endValue - _startValue;
        }

        protected override void InternalUpdateValue(float deltaChange)
        {
            var valueChanged = _changedValue * deltaChange;

            SetValue(_startValue + valueChanged);
        }

        protected abstract Vector3 GetCurrentValue();
        
        protected abstract void SetValue(Vector3 currentValue);
    }
}