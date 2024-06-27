using System;
using DG.Tweening;
using ThetanSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Pooling;
using Random = System.Random;

namespace ThetanSDK.UI
{
    public class ShowAnimCurrencyFly : MonoBehaviour
    {
        [SerializeField] private Image _prefabImgCoin;
        [SerializeField] private int _numberInstance = 3;
        [SerializeField] private Vector2 _instanceSizeRandomRange;
        [SerializeField] private Vector2 _expandStartRandomRange ;
        [SerializeField] private Ease _expandEase;
        [SerializeField] private float _startExpandDuration = 0.3f;
        [SerializeField] private float _moveToTargetDelay;
        [SerializeField] private Vector2 _moveToTargetDurationRandomRange;
        [SerializeField] private Ease _moveToTargetEase;
        
        private ObjectPool<GameObject> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                () => Instantiate(_prefabImgCoin.gameObject, this.transform),
                actionOnGet: instance => instance.SetActive(true),
                actionOnRelease: instance => instance.SetActive(false));
        }

        public async void DoAnimCurrencyFly(RectTransform targetPosition, Action callbackOnFirstItemReachTarget = null)
        {
            for (int i = 0; i < _numberInstance; i++)
            {
                var instanceGO = _pool.Get();
                instanceGO.SetActive(true);

                var instanceImg = instanceGO.GetComponent<Image>();

                if (instanceImg == null)
                {
                    _pool.Release(instanceGO);
                    return;
                }
                instanceImg.SetAlphaImg(1);
                var randomInstanceSize = UnityEngine.Random.Range(
                    _instanceSizeRandomRange.x,
                    _instanceSizeRandomRange.y);

                instanceGO.transform.localScale = new Vector3(randomInstanceSize, randomInstanceSize, randomInstanceSize);

                var randomVecStart = ThetanSDKUtilities.RandomVector2(
                    _expandStartRandomRange.x,
                    _expandStartRandomRange.y);
                var startDelta = new Vector3(randomVecStart.x, randomVecStart.y, 0);
                instanceGO.transform.localPosition = Vector3.zero;

                var sequence = DOTween.Sequence();
                sequence.Append(instanceGO.transform.DOLocalMove(startDelta * 0.8f, _startExpandDuration - _moveToTargetDelay)
                    .SetEase(_expandEase)
                    .SetUpdate(true));
                sequence.Append(instanceGO.transform.DOLocalMove(startDelta, _moveToTargetDelay)
                    .SetUpdate(true));

                var moveToTargetDuration = UnityEngine.Random.Range(
                    _moveToTargetDurationRandomRange.x,
                    _moveToTargetDurationRandomRange.y);
                sequence.Append(instanceGO.transform.DOMove(targetPosition.position, moveToTargetDuration)
                    .SetEase(_moveToTargetEase)
                    .SetUpdate(true));
                sequence.Join(instanceImg.DOFade(0, moveToTargetDuration)
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true));

                var itemIndex = i;
                sequence.SetUpdate(true);
                sequence.Play().OnComplete(() =>
                {
                    if(itemIndex == 0)
                        callbackOnFirstItemReachTarget?.Invoke();
                    _pool.Release(instanceGO);
                });
            }
        }
    }
}