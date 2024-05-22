using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.Scripts.UITab
{
    public class UITabItem : MonoBehaviour
    {
        [SerializeField] private Button _btnSelect;
        [SerializeField] private Image _imgSelected;
        [SerializeField] private TextMeshProUGUI _txtTitle;
        [SerializeField] private Color _colorTextSelected;
        [SerializeField] private Color _colorTextNormal;

        private int _itemIndex;
        private Action<int> _onClickCallback;

        private void Awake()
        {
            _btnSelect.onClick.AddListener(() => _onClickCallback?.Invoke(_itemIndex));
        }

        public void Initialize(int index, Action<int> onClickSelectCallback)
        {
            _itemIndex = index;
            _onClickCallback = onClickSelectCallback;
        }

        public void SetSelected(bool isSelected)
        {
            if(_imgSelected)
                _imgSelected.enabled = isSelected;
            
            if(_txtTitle)
                _txtTitle.color = isSelected ? _colorTextSelected : _colorTextNormal;
        }
    }
}
