using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ThetanSDK.Utilities.Pooling;

namespace ThetanSDK.UI
{
    public class ScreenContainer : MonoBehaviour
    {
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private GameObject _screenContainerPrefab;
        [SerializeField] private Button _btnBack;
        [SerializeField] private bool _isAnimateFirstScreenInStack;
        [SerializeField] private bool _isCanPopToEmpty;

        private List<GameObject> _screenContainerPool = new List<GameObject>();

        private Stack<ScreenActive> stackScreenActive = new Stack<ScreenActive>();

        public Screen CurrentScreen => stackScreenActive.Count == 0 ? null : stackScreenActive.Peek().Screen;

        [Header("Animation config")]
        [SerializeField] private float pushScreenDuration;
        [SerializeField] private float popScreenDuration;

        private Action _onClickCloseScreenCallback;

        private bool _allowButtonCloseScreen;

        private void Awake()
        {
            if(_btnBack)
            {
                _btnBack.gameObject.SetActive(false);
                _btnBack.onClick.AddListener(PopScreen);
            }
        }


        private struct ScreenActive
        {
            public GameObject ScreenContainer;
            public Screen Screen;
        }

        public async UniTask<Screen> PushScreen(Screen prefabScreen)
        {
            var screenContainer = GetScreenContainer();
            
            screenContainer.transform.SetAsLastSibling();

            
            var intanceScreenGO = await GlobalLazyPool.Rent(prefabScreen.gameObject);

            intanceScreenGO.SetActive(true);
            
            intanceScreenGO.transform.SetParent(screenContainer.transform);
            intanceScreenGO.transform.localScale = Vector3.one;
            intanceScreenGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var instanceScreen = intanceScreenGO.GetComponent<Screen>();
            
            instanceScreen.SetScreenContainer(this);
            
            if(_allowButtonCloseScreen)
                instanceScreen.EnableButtonCloseScreen();
            else
                instanceScreen.DisableButtonCloseScreen();

            if (intanceScreenGO.transform is RectTransform screenRectTransform)
            {
                screenRectTransform.offsetMin = Vector2.zero;
                screenRectTransform.offsetMax = Vector2.one;
            }

            if (screenContainer.transform is RectTransform screenContainerTransform)
            {
                if (stackScreenActive.Count > 0)
                {
                    var curScreenActive = stackScreenActive.Peek();
                    
                    screenContainerTransform.localPosition = new Vector3(_viewport.rect.width, 0, 0);

                    var newScreenActive = new ScreenActive()
                    {
                        ScreenContainer = screenContainer,
                        Screen = instanceScreen,
                    };

                    stackScreenActive.Push(newScreenActive);
                    PlayAnimPushScreen(curScreenActive, newScreenActive);
                    
                    if(_btnBack)
                        _btnBack.gameObject.SetActive(true);
                }
                else if (_isAnimateFirstScreenInStack)
                {
                    screenContainerTransform.localPosition = new Vector3(_viewport.rect.width, 0, 0);

                    var newScreenActive = new ScreenActive()
                    {
                        ScreenContainer = screenContainer,
                        Screen = instanceScreen,
                    };

                    stackScreenActive.Push(newScreenActive);
                    PlayAnimPushScreen(new ScreenActive(), newScreenActive);
                    
                    if(_btnBack && _isCanPopToEmpty)
                        _btnBack.gameObject.SetActive(true);
                }
                else
                {
                    screenContainerTransform.localPosition = new Vector3(0, 0, 0);

                    var newScreenActive = new ScreenActive()
                    {
                        ScreenContainer = screenContainer,
                        Screen = instanceScreen,
                    };
                    
                    newScreenActive.Screen.OnBeforePushScreen();
                    stackScreenActive.Push(newScreenActive);
                    newScreenActive.Screen.OnAfterPushScreen();
                }
            }

            return instanceScreen;
        }

        public async UniTask<Screen> ReplaceScreenStackByScreen(Screen prefabScreen, bool isKeepRootScreen = false)
        {
            var screenContainer = GetScreenContainer();
            
            screenContainer.transform.SetAsLastSibling();

            var intanceScreenGO = await GlobalLazyPool.Rent(prefabScreen.gameObject);

            intanceScreenGO.SetActive(true);
            
            intanceScreenGO.transform.SetParent(screenContainer.transform);
            intanceScreenGO.transform.localScale = Vector3.one;
            intanceScreenGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var instanceScreen = intanceScreenGO.GetComponent<Screen>();
            
            instanceScreen.SetScreenContainer(this);
            
            if(_allowButtonCloseScreen)
                instanceScreen.EnableButtonCloseScreen();
            else
                instanceScreen.DisableButtonCloseScreen();

            if (intanceScreenGO.transform is RectTransform screenRectTransform)
            {
                screenRectTransform.offsetMin = Vector2.zero;
                screenRectTransform.offsetMax = Vector2.zero;
            }

            if (screenContainer.transform is RectTransform screenContainerTransform)
            {
                if (stackScreenActive.Count > 0)
                {
                    var curScreenActive = stackScreenActive.Peek();

                    screenContainerTransform.localPosition = new Vector3(_viewport.rect.width, 0, 0);

                    var newScreenActive = new ScreenActive()
                    {
                        ScreenContainer = screenContainer,
                        Screen = instanceScreen,
                    };

                    newScreenActive.Screen.OnBeforePushScreen();
                    PlayAnimPushScreen(new ScreenActive(), newScreenActive, () =>
                    {
                        int numberOfScreenKeepOnStack = isKeepRootScreen ? 1 : 0;
                        
                        while (stackScreenActive.Count > numberOfScreenKeepOnStack)
                        {
                            stackScreenActive.Peek().Screen.OnBeforePopScreen();
                
                            var screen = stackScreenActive.Pop();
                
                            screen.Screen.OnAfterPopScreen();
                
                            GlobalLazyPool.Return(screen.Screen.gameObject);
                            ReturnScreenContainerToPool(screen.ScreenContainer);
                        }
                        
                        stackScreenActive.Push(newScreenActive);
                        newScreenActive.Screen.OnAfterPushScreen();
                    });

                    if (_btnBack != null)
                    {
                        if (_isCanPopToEmpty)
                            _btnBack.gameObject.SetActive(true);
                        else
                            _btnBack.gameObject.SetActive(false);
                    }
                }
                else if (_isAnimateFirstScreenInStack)
                {
                    screenContainerTransform.localPosition = new Vector3(_viewport.rect.width, 0, 0);

                    var newScreenActive = new ScreenActive()
                    {
                        ScreenContainer = screenContainer,
                        Screen = instanceScreen,
                    };
                    
                    newScreenActive.Screen.OnBeforePushScreen();

                    stackScreenActive.Push(newScreenActive);
                    PlayAnimPushScreen(new ScreenActive(), newScreenActive, () =>
                    {
                        newScreenActive.Screen.OnAfterPushScreen();
                    });

                    if (_btnBack && _isCanPopToEmpty)
                        _btnBack.gameObject.SetActive(true);
                }
                else
                {
                    screenContainerTransform.localPosition = new Vector3(0, 0, 0);

                    var newScreenActive = new ScreenActive()
                    {
                        ScreenContainer = screenContainer,
                        Screen = instanceScreen,
                    };

                    newScreenActive.Screen.OnBeforePushScreen();
                    stackScreenActive.Push(newScreenActive);
                    newScreenActive.Screen.OnAfterPushScreen();
                }
            }

            return instanceScreen;
        }
        
        public void PopScreen()
        {
            if (stackScreenActive.Count == 0)
                return;
            var curActiveScreen = stackScreenActive.Peek();
            
            curActiveScreen.Screen.OnBeforePopScreen();
            stackScreenActive.Pop();

            if ((stackScreenActive.Count <= 1 && !_isAnimateFirstScreenInStack) ||
                stackScreenActive.Count <= 0)
            {
                if(_btnBack)
                    _btnBack.gameObject.SetActive(false);
            }

            if (stackScreenActive.Count == 0 && !_isAnimateFirstScreenInStack)
            {
                curActiveScreen.Screen.OnAfterPopScreen();
                GlobalLazyPool.Return(curActiveScreen.Screen.gameObject);
                ReturnScreenContainerToPool(curActiveScreen.ScreenContainer);
            }
            else if (stackScreenActive.Count == 0 && _isAnimateFirstScreenInStack)
            {
                PlayAnimPopScreen(curActiveScreen, new ScreenActive(), () =>
                {
                    GlobalLazyPool.Return(curActiveScreen.Screen.gameObject);
                    ReturnScreenContainerToPool(curActiveScreen.ScreenContainer);
                });
            }
            else
            {
                var nextActiveScreen = stackScreenActive.Peek();
                
                if(_allowButtonCloseScreen)
                    nextActiveScreen.Screen.EnableButtonCloseScreen();
                else
                    nextActiveScreen.Screen.DisableButtonCloseScreen();
                
                PlayAnimPopScreen(curActiveScreen, nextActiveScreen, () =>
                {
                    GlobalLazyPool.Return(curActiveScreen.Screen.gameObject);
                    ReturnScreenContainerToPool(curActiveScreen.ScreenContainer);
                });
            }
        }

        public void PopToRoot()
        {
            if(_btnBack)
                _btnBack.gameObject.SetActive(false);
            
            while (stackScreenActive.Count > 1)
            {
                stackScreenActive.Peek().Screen.OnBeforePopScreen();
                
                var screen = stackScreenActive.Pop();
                
                screen.Screen.OnAfterPopScreen();
                
                GlobalLazyPool.Return(screen.Screen.gameObject);
                ReturnScreenContainerToPool(screen.ScreenContainer);
            }
            
            stackScreenActive.Peek().ScreenContainer.transform.localPosition = Vector3.zero;
        }

        public void PopAllScreen()
        {
            if(_btnBack)
                _btnBack.gameObject.SetActive(false);
            
            while (stackScreenActive.Count > 0)
            {
                stackScreenActive.Peek().Screen.OnBeforePopScreen();
                
                var screen = stackScreenActive.Pop();
                
                screen.Screen.OnAfterPopScreen();
                
                GlobalLazyPool.Return(screen.Screen.gameObject);
                ReturnScreenContainerToPool(screen.ScreenContainer);
            }
        }

        private void PlayAnimPopScreen(ScreenActive curScreenActive, ScreenActive nextScreenActive, Action onDone)
        {
            if (curScreenActive.ScreenContainer.transform is RectTransform curScreenTransform)
            {
                curScreenTransform.DOLocalMoveX(_viewport.rect.width, popScreenDuration).OnComplete(() =>
                {
                    curScreenActive.Screen.OnAfterPopScreen();
                    onDone?.Invoke();
                });
            }
            
            if (nextScreenActive.ScreenContainer != null &&
                nextScreenActive.ScreenContainer.transform is RectTransform newScreenTransform)
            {
                newScreenTransform.DOLocalMoveX(0, popScreenDuration);
            }
        }

        private void PlayAnimPushScreen(ScreenActive curScreenActive, ScreenActive newScreenActive, Action onDoneCallback = null)
        {
            if (curScreenActive.ScreenContainer != null &&
                curScreenActive.ScreenContainer.transform is RectTransform curScreenTransform)
            {
                curScreenTransform.DOLocalMoveX(-_viewport.rect.width, pushScreenDuration);
            }
            
            newScreenActive.Screen.OnBeforePushScreen();
            if (newScreenActive.ScreenContainer.transform is RectTransform newScreenTransform)
            {
                newScreenTransform.DOLocalMoveX(0, pushScreenDuration).OnComplete(() =>
                {
                    onDoneCallback?.Invoke();
                    newScreenActive.Screen.OnAfterPushScreen();
                });
            }
        }

        private GameObject GetScreenContainer()
        {
            if (_screenContainerPool.Count == 0)
                return CreateScreenContainer();

            var screenContainer = _screenContainerPool[_screenContainerPool.Count - 1];
            
            screenContainer.SetActive(true);
            _screenContainerPool.RemoveAt(_screenContainerPool.Count - 1);

            return screenContainer;
        }

        private void ReturnScreenContainerToPool(GameObject screenContainer)
        {
            screenContainer.SetActive(false);

            _screenContainerPool.Add(screenContainer);
        }

        private GameObject CreateScreenContainer()
        {
            var screenContainer = Instantiate(_screenContainerPrefab, _viewport);

            if (screenContainer.transform is RectTransform rectTransformScreenContainer)
            {
                //rectTransformScreenContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _viewport.rect.width);
                //rectTransformScreenContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _viewport.rect.height);
                
                //rectTransformScreenContainer.anchorMin = Vector2.zero;
                //rectTransformScreenContainer.anchorMax = Vector2.one;
            }
            
            screenContainer.SetActive(true);
            return screenContainer;
        }

        public void RegisterOnClickCloseScreen(Action callback)
        {
            _onClickCloseScreenCallback += callback;
        }

        public void UnRegisterOnClickCloseScreen(Action callback)
        {
            _onClickCloseScreenCallback -= callback;
        }

        public void DisableButtonCloseScreen()
        {
            _allowButtonCloseScreen = false;

            if (stackScreenActive != null && stackScreenActive.Count > 0)
            {
                var screen = stackScreenActive.Peek().Screen;
                screen.DisableButtonCloseScreen();
            }
        }

        public void EnableButtonCloseScreen()
        {
            _allowButtonCloseScreen = true;
            
            if (stackScreenActive != null && stackScreenActive.Count > 0)
            {
                var screen = stackScreenActive.Peek().Screen;
                screen.EnableButtonCloseScreen();
            }
        }
        
        public void NotifyOnClickCloseScreen()
        {
            if (!_allowButtonCloseScreen)
                return;
            
            _onClickCloseScreenCallback?.Invoke();
        }
    }
}
