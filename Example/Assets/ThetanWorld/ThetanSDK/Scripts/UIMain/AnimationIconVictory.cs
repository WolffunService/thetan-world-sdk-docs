using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.Tweening;

public class AnimationIconVictory : MonoBehaviour
{
    [SerializeField] private RectTransform _contentVictory;

    [SerializeField] private float scaleUpDuration;
    [SerializeField] private Ease scaleUpEase;

    [SerializeField] private Vector3 heartbeatUpScale;
    [SerializeField] private float heartbeatUpDuration;
    [SerializeField] private Ease heartbeatUpEase;
    [SerializeField] private Vector3 heartbeatDownScale;
    [SerializeField] private float heartbeatDownDuration;
    [SerializeField] private Ease heartbeatDownEase;
    [SerializeField] private int countHeartbeat;
    [SerializeField] private float delayFirstHeartbeat;
    [SerializeField] private float delayIntervalHeartbeat;
    
    [SerializeField] private float delayScaleDown;
    [SerializeField] private float scaleDownDuration;
    [SerializeField] private Ease scaleDownEase;

    private TweenSequence _sequence;
    
    private void Awake()
    {
        _contentVictory.gameObject.SetActive(false);
    }

    public void PlayAnimation()
    {
        _contentVictory.localScale = Vector3.zero;
        _contentVictory.gameObject.SetActive(true);

        if (_sequence != null)
        {
            _sequence.Complete();
            _sequence = null;
        }

        var sequence = WolfTween.GetSequence();

        _sequence = sequence;

        sequence.Append(_contentVictory.DOScale(Vector3.one, scaleUpDuration).SetEase(scaleUpEase));
        
        for (int i = 0; i < countHeartbeat; i++)
        {
            sequence.Append(_contentVictory.DOScale(heartbeatUpScale, heartbeatUpDuration).SetEase(heartbeatUpEase)
                .SetDelay(i == 0 ? delayFirstHeartbeat : delayIntervalHeartbeat));
            sequence.Append(_contentVictory.DOScale(heartbeatDownScale, heartbeatDownDuration)
                .SetEase(heartbeatDownEase));
        }
        
        sequence.Append(_contentVictory.DOScale(Vector3.zero, scaleDownDuration).SetEase(scaleDownEase)
            .SetDelay(delayScaleDown));
        sequence.OnComplete(() =>
        {
            _contentVictory.gameObject.SetActive(false);
            _sequence = null;
        });
    }
}
