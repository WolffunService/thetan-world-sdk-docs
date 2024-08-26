using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    [RequireComponent(typeof(Slider))]
    public class CommonSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _txtValue;
        [SerializeField] private Image _imgFill;

        [SerializeField] private Color _colorNotFull = new Color(0, 183 / 255f, 239 / 255f, 1);
        [SerializeField] private Color _colorFull = new Color(236 / 255f, 70 / 255f, 110 / 255f, 1);

        [SerializeField] private string _valueFormatNotFull = "{0}<color=#AFAFB2>/{1}</color>";
        [SerializeField] private string _valueFormatFull = "<color=#FFFFFF>{0}/{1}</color>";

        private Func<float, float> _valueViewConvertFunc;

        private void Awake()
        {
            SetUIBasedOnSliderValue();
        }

        private void SetUIBasedOnSliderValue()
        {
            bool isMaxValue = _slider.value >= _slider.maxValue;
            if (_imgFill)
            {
                _imgFill.color = isMaxValue ? _colorFull : _colorNotFull;
            }

            if (_txtValue)
            {
                var format = isMaxValue ? _valueFormatFull : _valueFormatNotFull;
                
                if(_valueViewConvertFunc != null)
                    _txtValue.text = string.Format(format, _valueViewConvertFunc(_slider.value), _valueViewConvertFunc(_slider.maxValue));
                else
                    _txtValue.text = string.Format(format, _slider.value, _slider.maxValue);
            }
        }

        public void SetValueConvertToStringFunction(Func<float, float> func)
        {
            _valueViewConvertFunc = func;
        }

        public void SetData(float minValue, float maxValue, float value)
        {
            _slider.minValue = minValue;
            _slider.maxValue = maxValue;
            _slider.value = value;
            SetUIBasedOnSliderValue();
        }

        public void SetMinValue(float value)
        {
            _slider.minValue = value;
            SetUIBasedOnSliderValue();
        }

        public void SetMaxValue(float maxValue)
        {
            _slider.maxValue = maxValue;
            SetUIBasedOnSliderValue();
        }

        public void SetValue(float value)
        {
            _slider.value = value;
            SetUIBasedOnSliderValue();
        }

        public void UseColorFull()
        {
            if (_imgFill)
            {
                _imgFill.color = _colorFull;
            }

            if (_txtValue)
            {
                var format = _valueFormatFull;
                
                if(_valueViewConvertFunc != null)
                    _txtValue.text = string.Format(format, _valueViewConvertFunc(_slider.value), _valueViewConvertFunc(_slider.maxValue));
                else
                    _txtValue.text = string.Format(format, _slider.value, _slider.maxValue);
            }
        }
        
        private void Reset()
        {
            _slider = GetComponent<Slider>();
            _imgFill = _slider.fillRect.GetComponent<Image>();
        }
    }
}
