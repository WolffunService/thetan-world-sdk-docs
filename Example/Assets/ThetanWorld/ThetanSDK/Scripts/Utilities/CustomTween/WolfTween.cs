using UnityEngine;
using UnityEngine.UI;

namespace Wolffun.Tweening
{
    public static partial class WolfTween
    {
        public static TweenSequence GetSequence()
        {
            var sequence = TweenManager.RentTween<TweenSequence>();
            
            TweenManager.RegisterTween(sequence);

            return sequence;
        }

        public static TweenSequence Append(this TweenSequence sequence, Tweener tweener)
        {
            sequence.InsertAt(tweener, sequence.Duration);

            return sequence;
        }

        public static TweenSequence Join(this TweenSequence sequence, Tweener tweener)
        {
            sequence.InsertAt(tweener, sequence.LastInsertPosition);

            return sequence;
        }
        
        public static TweenSequence Append(this TweenSequence sequence, TweenerCore tweener)
        {
            sequence.InsertAt(tweener, sequence.Duration);

            return sequence;
        }

        public static TweenSequence Join(this TweenSequence sequence, TweenerCore tweener)
        {
            sequence.InsertAt(tweener, sequence.LastInsertPosition);

            return sequence;
        }

        public static void Complete(this TweenerCore tweener)
        {
            TweenManager.Complete(tweener);
        }

        public static void Kill(this TweenerCore tweener)
        {
            TweenManager.Kill(tweener);
        }
        
    }
}