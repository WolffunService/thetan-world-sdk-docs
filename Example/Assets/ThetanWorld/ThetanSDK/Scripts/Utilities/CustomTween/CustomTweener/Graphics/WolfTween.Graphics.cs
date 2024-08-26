using UnityEngine;
using UnityEngine.UI;

namespace Wolffun.Tweening
{
    public static partial class WolfTween
    {
        public static Tweener DOFade(this CanvasGroup canvasGroup, float endValue, float duration)
        {
            var tweener = TweenManager.RentTween<FadeCanvasGroupTweener>();
            tweener.SetUp(canvasGroup, endValue, duration);
            
            TweenManager.RegisterTween(tweener);

            return tweener;
        }
        
        public static Tweener DOFade(this Graphic graphic, float endValue, float duration)
        {
            var tweener = TweenManager.RentTween<FadeGraphicTweener>();
            tweener.SetUp(graphic, endValue, duration);
            
            TweenManager.RegisterTween(tweener);

            return tweener;
        }
    }
}