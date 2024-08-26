using UnityEngine.UI;

namespace Wolffun.Tweening
{
    public sealed class FadeGraphicTweener : FloatTweener
    {
        private Graphic _graphic;

        public void SetUp(Graphic graphic, float endValue, float duration)
        {
            _graphic = graphic;
            SetUp(endValue, duration);
        }

        protected override float GetCurrentValue()
        {
            if (!_graphic)
                return default;

            return _graphic.color.a;
        }

        protected override void SetValue(float valueChanged)
        {
            if (!_graphic)
                return;

            var color = _graphic.color;
            color.a = valueChanged;
            _graphic.color = color;
        }

        public override void ResetValue()
        {
            base.ResetValue();

            _graphic = null;
        }
    }
}