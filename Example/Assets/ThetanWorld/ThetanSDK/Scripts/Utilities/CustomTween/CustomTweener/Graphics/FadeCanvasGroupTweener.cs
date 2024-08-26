using UnityEngine;

namespace Wolffun.Tweening
{
    public sealed class FadeCanvasGroupTweener : FloatTweener
    {
        private CanvasGroup _canvasGroup;

        public void SetUp(CanvasGroup canvasGroup, float endValue, float duration)
        {
            _canvasGroup = canvasGroup;
            SetUp(endValue, duration);
        }

        protected override float GetCurrentValue()
        {
            if (!_canvasGroup)
                return default;

            return _canvasGroup.alpha;
        }

        protected override void SetValue(float valueChanged)
        {
            if (!_canvasGroup)
                return;
            
            _canvasGroup.alpha = valueChanged;
        }

        public override void ResetValue()
        {
            base.ResetValue();

            _canvasGroup = null;
        }
    }
}