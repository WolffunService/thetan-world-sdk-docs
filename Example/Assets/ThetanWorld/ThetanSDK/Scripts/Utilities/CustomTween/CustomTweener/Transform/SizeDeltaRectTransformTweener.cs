using UnityEngine;

namespace Wolffun.Tweening
{
    public sealed class SizeDeltaRectTransformTweener : Vector2Tweener
    {
        private RectTransform _rectTransform;

        public void SetUp(RectTransform rectTransform, Vector2 endValue, float duration)
        {
            _rectTransform = rectTransform;
            SetUp(endValue, duration);
        }

        protected override Vector2 GetCurrentValue()
        {
            if (!_rectTransform)
                return default;

            return _rectTransform.sizeDelta;
        }

        protected override void SetValue(Vector2 currentValue)
        {
            if (!_rectTransform)
                return;

            _rectTransform.sizeDelta = currentValue;
        }

        public override void ResetValue()
        {
            base.ResetValue();

            _rectTransform = null;
        }
    }
}