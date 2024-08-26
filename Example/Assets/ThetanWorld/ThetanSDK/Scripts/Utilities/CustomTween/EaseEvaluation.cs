using System;
using UnityEngine;

namespace Wolffun.Tweening
{
  public static class EaseEvaluation
  {
    public static float Evaluate(Ease easeType, float time, float duration)
    {
      switch (easeType)
      {
        case Ease.Linear:
          return time / duration;
        case Ease.InSine:
          return (float)(-Math.Cos((double)time / (double)duration * Mathf.PI) + 1.0);
        case Ease.OutSine:
          return (float)Math.Sin((double)time / (double)duration * Mathf.PI);
        case Ease.InOutSine:
          return (float)(-0.5 * (Math.Cos(Mathf.PI * (double)time / (double)duration) - 1.0));
        case Ease.InQuad:
          return (time /= duration) * time;
        case Ease.OutQuad:
          return (float)(-(double)(time /= duration) * ((double)time - 2.0));
        case Ease.InOutQuad:
          return (double)(time /= duration * 0.5f) < 1.0
            ? 0.5f * time * time
            : (float)(-0.5 * ((double)--time * ((double)time - 2.0) - 1.0));
        case Ease.InCubic:
          return (time /= duration) * time * time;
        case Ease.OutCubic:
          return (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time +
                         1.0);
        case Ease.InOutCubic:
          return (double)(time /= duration * 0.5f) < 1.0
            ? 0.5f * time * time * time
            : (float)(0.5 * ((double)(time -= 2f) * (double)time * (double)time + 2.0));
        case Ease.InQuart:
          return (time /= duration) * time * time * time;
        case Ease.OutQuart:
          return (float)-((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time *
            (double)time * (double)time - 1.0);
        case Ease.InOutQuart:
          return (double)(time /= duration * 0.5f) < 1.0
            ? 0.5f * time * time * time * time
            : (float)(-0.5 * ((double)(time -= 2f) * (double)time * (double)time * (double)time - 2.0));
        case Ease.InQuint:
          return (time /= duration) * time * time * time * time;
        case Ease.OutQuint:
          return (float)((double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time * (double)time *
            (double)time * (double)time + 1.0);
        case Ease.InOutQuint:
          return (double)(time /= duration * 0.5f) < 1.0
            ? 0.5f * time * time * time * time * time
            : (float)(0.5 * ((double)(time -= 2f) * (double)time * (double)time * (double)time * (double)time + 2.0));
        case Ease.InExpo:
          return (double)time != 0.0 ? (float)Math.Pow(2.0, 10.0 * ((double)time / (double)duration - 1.0)) : 0.0f;
        case Ease.OutExpo:
          return (double)time == (double)duration
            ? 1f
            : (float)(-Math.Pow(2.0, -10.0 * (double)time / (double)duration) + 1.0);
        case Ease.InOutExpo:
          if ((double)time == 0.0)
            return 0.0f;
          if ((double)time == (double)duration)
            return 1f;
          return (double)(time /= duration * 0.5f) < 1.0
            ? 0.5f * (float)Math.Pow(2.0, 10.0 * ((double)time - 1.0))
            : (float)(0.5 * (-Math.Pow(2.0, -10.0 * (double)--time) + 2.0));
        case Ease.InCirc:
          return (float)-(Math.Sqrt(1.0 - (double)(time /= duration) * (double)time) - 1.0);
        case Ease.OutCirc:
          return (float)Math.Sqrt(1.0 - (double)(time = (float)((double)time / (double)duration - 1.0)) * (double)time);
        case Ease.InOutCirc:
          return (double)(time /= duration * 0.5f) < 1.0
            ? (float)(-0.5 * (Math.Sqrt(1.0 - (double)time * (double)time) - 1.0))
            : (float)(0.5 * (Math.Sqrt(1.0 - (double)(time -= 2f) * (double)time) + 1.0));
        case Ease.InBack:
        {
          double overshootOrAmplitude = 1.70158d;
          double t = time / duration;
          return (float)(t * t * ((overshootOrAmplitude + 1.0) * t - overshootOrAmplitude));
        }
        case Ease.OutBack:
        {
          double overshootOrAmplitude = 1.70158d;
          double t = time / duration - 1;
          return (float)(t * t * ((overshootOrAmplitude + 1.0) * t + overshootOrAmplitude) + 1.0);
        }
        case Ease.InOutBack:
        {
          double overshootOrAmplitude = 1.70158d;
          return (double)(time /= duration * 0.5f) < 1.0
            ? (float)(0.5 * ((double)time * (double)time *
                             (((double)(overshootOrAmplitude *= 1.525f) + 1.0) * (double)time -
                              (double)overshootOrAmplitude)))
            : (float)(0.5 * ((double)(time -= 2f) * (double)time *
              (((double)(overshootOrAmplitude *= 1.525f) + 1.0) * (double)time + (double)overshootOrAmplitude) + 2.0));
        }
        default:
          return (float)(-(double)(time /= duration) * ((double)time - 2.0));
      }
    }
  }
}
