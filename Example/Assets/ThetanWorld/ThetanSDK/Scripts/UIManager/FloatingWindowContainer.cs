using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ThetanSDK.Utilities.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class FloatingWindowContainer : MonoBehaviour
    {
        [SerializeField] private Button _btnBlockCanvas;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _defaultTargetFloatingWindowPosition;

        [Header("Animation config")]
        [SerializeField] private float animationShowDuration;
        [SerializeField] private Ease animationShowEase;
        [SerializeField] private float animationCloseDuration;
        [SerializeField] private Ease animationCloseEase;
        
        
        private FloatingWindow _curFloatingWindow;

        public FloatingWindow CurrentFloatingWindow => _curFloatingWindow;

        private void Awake()
        {
            _curFloatingWindow = null;
            _btnBlockCanvas.gameObject.SetActive(false);
            _btnBlockCanvas.onClick.AddListener(OnClickBlockCanvas);
        }

        public void CloseCurrentFloatingWindow()
        {
            if (_curFloatingWindow == null)
                return;
            
            PlayAimationCloseWindow(_curFloatingWindow.gameObject, _curFloatingWindow, () =>
            {
                _btnBlockCanvas.gameObject.SetActive(false);
                
                GlobalLazyPool.Return(_curFloatingWindow.gameObject);
                _curFloatingWindow = null;
            });
        }
        
        public async UniTask<FloatingWindow> ShowFloatingWindow(FloatingWindow prefabFloatingWindow, RectTransform targetTransform = null)
        {
            if (_curFloatingWindow != null)
            {
                throw new AnotherFloatingWindowShowingException();
            }

            _btnBlockCanvas.gameObject.SetActive(true);
            
            var instanceFloatingWindowGO = await GlobalLazyPool.Rent(prefabFloatingWindow.gameObject);
            instanceFloatingWindowGO.SetActive(true);
            instanceFloatingWindowGO.transform.SetParent(_viewport);

            if (!instanceFloatingWindowGO.TryGetComponent<FloatingWindow>(out var instanceFloatingWindow))
            {
                Debug.LogError($"Prefab {prefabFloatingWindow.name} does not contain FloatingWindow component");
                GlobalLazyPool.Return(instanceFloatingWindowGO);
                return null;
            }

            instanceFloatingWindow.Initialize(this);
            var realTargetTransform = targetTransform != null ? targetTransform : _defaultTargetFloatingWindowPosition;

            _curFloatingWindow = instanceFloatingWindow;

            var pivotForFloatingWindow = CalculatePivotForFloatingWindow(realTargetTransform);

            if (instanceFloatingWindowGO.transform is RectTransform floatingWindowTransform)
            {
                floatingWindowTransform.pivot = pivotForFloatingWindow;
            }
            
            var pivotForTarget =  Vector2.one - 2 * pivotForFloatingWindow;

            Vector3 floatingWindowPosition = _viewport.transform.InverseTransformPoint(realTargetTransform.position);
            
            var rect = realTargetTransform.rect;
            floatingWindowPosition.x += pivotForTarget.x * rect.width / 2;
            floatingWindowPosition.y += pivotForTarget.y * rect.height / 2;

            instanceFloatingWindowGO.transform.localPosition = floatingWindowPosition;
            
            PlayAnimationShowWindow(instanceFloatingWindowGO, instanceFloatingWindow);
            
            return _curFloatingWindow;
        }

        private void PlayAnimationShowWindow(GameObject floatingWindowGO, FloatingWindow floatingWindow)
        {
            floatingWindow.OnBeforeShow();
            floatingWindowGO.transform.localScale = Vector3.zero;

            floatingWindowGO.transform.DOScale(Vector3.one, animationShowDuration)
                .SetUpdate(true)
                .SetEase(animationShowEase)
                .OnComplete(() =>
                {
                    floatingWindow.OnAfterShow();
                });
        }

        private void PlayAimationCloseWindow(GameObject floatingWindowGO, FloatingWindow floatingWindow, Action onDoneCallback)
        {
            floatingWindow.OnBeforeClose();

            floatingWindowGO.transform.DOScale(Vector3.zero, animationCloseDuration)
                .SetEase(animationCloseEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    floatingWindow.OnAfterClose();
                    onDoneCallback?.Invoke();
                });
        }

        private Vector2 CalculatePivotForFloatingWindow(RectTransform targetFloatingWindow)
        {
            var screenSize = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
            if (_viewport != null)
            {
                var viewportRect = _viewport.rect;
                screenSize = new Vector2(viewportRect.width, viewportRect.height);
            }
            
            Vector2 result = Vector2.zero;

            var targetPosition = _viewport.transform.InverseTransformPoint(targetFloatingWindow.position);

            if (targetPosition.x < -screenSize.x / 6)
            {
                result.x = 0;
            }
            else if (targetPosition.x > screenSize.x / 6)
            {
                result.x = 1;
            }
            else
            {
                result.x = 0.5f;
            }
            
            if (targetPosition.y < -screenSize.y / 6)
            {
                result.y = 0;
            }
            else if (targetPosition.y > screenSize.y / 6)
            {
                result.y = 1;
            }
            else
            {
                result.y = 0.5f;
            }

            return result;
        }


        private void OnClickBlockCanvas()
        {
            CloseCurrentFloatingWindow();
        }
    }

    public class AnotherFloatingWindowShowingException : Exception
    {
    }
}
