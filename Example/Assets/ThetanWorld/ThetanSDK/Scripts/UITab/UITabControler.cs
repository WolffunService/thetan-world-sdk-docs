using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThetanSDK.Scripts.UITab
{
    public class UITabControler : MonoBehaviour
    {
        [SerializeField] private List<UITabItem> _listUITabItem;

        [SerializeField] private UnityEvent<int> _onSelectItemCallback;

        public UnityEvent<int> OnSelectItemCallback
        {
            get => _onSelectItemCallback;
            set => _onSelectItemCallback = value;
        }

        public void SelectTab(int tabIndex)
        {
            if (_listUITabItem == null)
                return;

            if (tabIndex < 0 || tabIndex >= _listUITabItem.Count)
                return;

            OnSelectItem(tabIndex);
        }
        
        private void Awake()
        {
            if (_listUITabItem != null)
            {
                for (int i = 0; i < _listUITabItem.Count; i++)
                {
                    _listUITabItem[i].Initialize(i, OnSelectItem);   
                }
            }
        }

        private void OnSelectItem(int itemIndex)
        {
            _onSelectItemCallback.Invoke(itemIndex);
            
            for (int i = 0; i < _listUITabItem.Count; i++)
            {
                _listUITabItem[i].SetSelected(i == itemIndex);
            }
        }
    }
}