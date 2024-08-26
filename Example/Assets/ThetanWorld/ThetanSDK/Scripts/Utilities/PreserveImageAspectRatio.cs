using System;
using UnityEngine;

public class PreserveImageAspectRatio : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;

    private float _defaultRatio = 0f;
    private bool _hasSetSize = false;
    
    private void Awake()
    {
        if (_rectTransform)
        {
            
            var size = _rectTransform.rect.size;
            _defaultRatio = size.x / size.y;
        }
    }

    private void LateUpdate()
    {
        if (!_hasSetSize && _rectTransform)
        {
            _hasSetSize = true;
            var size = _rectTransform.rect.size;
            var newRatio = size.x / size.y;
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            if (newRatio < _defaultRatio)
            {
                var newSize = size;
                newSize.y = size.x / _defaultRatio;
                
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
            }
            else
            {
                var newSize = size;
                newSize.x = size.y * _defaultRatio;
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
            }
        }
    }

    private void Reset()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
}
