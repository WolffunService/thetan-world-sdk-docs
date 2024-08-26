using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Wolffun.Tweening;
using ThetanSDK.UI;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Pooling;

public class PopupContainer : MonoBehaviour
{
    [SerializeField] private PopupBackdrop _prefabBackdrop;
    [SerializeField] private RectTransform _viewport;

    [SerializeField] private float scaleInDuration = 0.15f;
    [SerializeField] private Ease scaleInEase = Ease.OutBack;
    [SerializeField] private float scaleOutDuration = 0.1f;
    [SerializeField] private Ease scaleOutEase = Ease.InBack;
    
    private List<PopupBackdrop> _backdropPool = new List<PopupBackdrop>();

    private Stack<PopupActive> stackPopupActive = new Stack<PopupActive>();

    public Popup CurrentPopup => stackPopupActive.Count <= 0 ? null : stackPopupActive.Peek().popup;

    private struct PopupActive
    {
        public Popup popup;
        public PopupBackdrop backdrop;
    }
    
    public async UniTask<Popup> Push(Popup prefab, PopupOption options)
    {
        PopupBackdrop popupBackdrop = null;
        if (options.IsAllowBackdrop)
        {
            popupBackdrop = GetBackdrop();
            
            popupBackdrop.transform.SetAsLastSibling();
        }

        var instance = SimplePool.Instance.Rent(prefab.gameObject).GetComponent<Popup>();
        instance.SetPopupContainer(this);

        instance.gameObject.SetActive(true);

        var instanceTransform = instance.transform;
        instanceTransform.SetParent(_viewport, false);
        instanceTransform.SetAsLastSibling();
        instanceTransform.localScale = Vector3.zero;
        instanceTransform.localPosition = Vector3.zero;
        instanceTransform.localRotation = Quaternion.identity;
        
        if (instanceTransform.transform is RectTransform popupRectTransform &&
            prefab.transform is RectTransform prefabRectTransform)
        {
            popupRectTransform.offsetMin = prefabRectTransform.offsetMin;
            popupRectTransform.offsetMax = prefabRectTransform.offsetMax;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(popupRectTransform);
        }
        
        instance.OnBeforePushPopup();
        
        stackPopupActive.Push(new PopupActive()
        {
            backdrop = popupBackdrop,
            popup = instance
        });

        if (options.SkipAnimation)
        {
            instanceTransform.localScale = Vector3.one;
            instance.OnAfterPushPopup();
            if(popupBackdrop != null)
                popupBackdrop.SetPopup(this, instance, options.IsBackdropCanClosePopup);
        }
        else if(instanceTransform is RectTransform rt)
        {
            rt.DOScale(Vector3.one, scaleInDuration)
                .SetEase(scaleInEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    instance.OnAfterPushPopup();
                    
                    if(popupBackdrop != null)
                        popupBackdrop.SetPopup(this, instance, options.IsBackdropCanClosePopup);
                });
        }
        else
        {
            instance.OnAfterPushPopup();
                    
            if(popupBackdrop != null)
                popupBackdrop.SetPopup(this, instance, options.IsBackdropCanClosePopup);
        }

        return instance;
    }

    public void Pop()
    {
        if (stackPopupActive.Count == 0)
            return;
        
        var curActivePopup = stackPopupActive.Peek();
        
        curActivePopup.popup.OnBeforePopPopup();

        stackPopupActive.Pop();

        if (curActivePopup.popup.transform is RectTransform rt)
        {
            rt.DOScale(Vector3.zero, scaleOutDuration)
                .SetEase(scaleOutEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    curActivePopup.popup.OnAfterPopPopup();
                    SimplePool.Instance.Return(curActivePopup.popup.gameObject);
                    if(curActivePopup.backdrop != null)
                    {
                        curActivePopup.backdrop.ClearData();
                        ReturnBackdropToPool(curActivePopup.backdrop);
                    }
                });
        }
        else
        {
            curActivePopup.popup.OnAfterPopPopup();
            SimplePool.Instance.Return(curActivePopup.popup.gameObject);
            
            if(curActivePopup.backdrop)
            {
                curActivePopup.backdrop.ClearData();
                ReturnBackdropToPool(curActivePopup.backdrop);
            }
        }
            
    }

    public void PopAll()
    {
        while (stackPopupActive.Count > 0)
        {
            stackPopupActive.Peek().popup.OnBeforePopPopup();
                
            var popup = stackPopupActive.Pop();
                
            popup.popup.OnAfterPopPopup();
                
            SimplePool.Instance.Return(popup.popup.gameObject);
            
            if(popup.backdrop != null)
                ReturnBackdropToPool(popup.backdrop);
        }
    }
    
    private PopupBackdrop GetBackdrop()
    {
        if (_backdropPool.Count == 0)
            return CreateBackdrop();

        var backdrop = _backdropPool[_backdropPool.Count - 1];
            
        backdrop.gameObject.SetActive(true);
        _backdropPool.RemoveAt(_backdropPool.Count - 1);

        return backdrop;
    }
    
    private PopupBackdrop CreateBackdrop()
    {
        var backdrop = Instantiate(_prefabBackdrop, _viewport);

        if (backdrop.transform is RectTransform rectTransformBackdrop)
        {
            rectTransformBackdrop.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _viewport.rect.width);
            rectTransformBackdrop.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _viewport.rect.height);
        }
            
        backdrop.gameObject.SetActive(true);
        return backdrop;
    }
    
    private void ReturnBackdropToPool(PopupBackdrop backdrop)
    {
        if (backdrop == null)
            return;
        
        backdrop.gameObject.SetActive(false);

        _backdropPool.Add(backdrop);
    }
}

public struct PopupOption
{
    public bool IsAllowBackdrop;
    public bool IsBackdropCanClosePopup;
    public bool SkipAnimation;
}
