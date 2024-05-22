using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI.CustomComponent
{
    [Serializable]
    public class ButtonControlTextColor : Button
    {
        [Header("Shit")]
        [SerializeField] private TextMeshProUGUI _txtButton;
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _disableColor;

#if UNITY_EDITOR
        public TextMeshProUGUI TxtButton
        {
            get => _txtButton;
            set => _txtButton = value;
        }

        public Color NormalColor
        {
            get => _normalColor;
            set => _normalColor = value;
        }

        public Color DisableColor
        {
            get => _disableColor;
            set => _disableColor = value;
        }
#endif

    
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            Color colorText = _normalColor;
        
            switch (state)
            {
                case SelectionState.Disabled:
                    colorText = _disableColor;
                    break;
                default:
                    colorText = _normalColor;
                    break;
            }

            if(_txtButton)
                _txtButton.color = colorText;
        }
    }   
}
