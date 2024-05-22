using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultipleDegitCodeField : MonoBehaviour
{
    [Serializable]
    private struct InputConfig
    {
        public TextMeshProUGUI txt;
        public Image img;
    }
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private List<InputConfig> _listTxtDigitCode;
    [SerializeField] private Color colorEmpty;
    [SerializeField] private Color colorWithValue;

    private void Awake()
    {
        _inputField.onValueChanged.AddListener(HandleInputValueChange);
        HandleInputValueChange(string.Empty);
    }

    private void HandleInputValueChange(string value)
    {
        for (int i = 0; i < _listTxtDigitCode.Count; i++)
        {
            var txtDigitCode = _listTxtDigitCode[i];
            if (string.IsNullOrEmpty(value) ||
                i >= value.Length)
            {
                txtDigitCode.txt.text = string.Empty;
            }
            else
            {
                txtDigitCode.txt.text = value[i].ToString();
            }

            if (i == value.Length ||
                (value.Length == _listTxtDigitCode.Count && i == _listTxtDigitCode.Count - 1))
            {
                txtDigitCode.img.color = colorWithValue;
            }
            else
            {
                txtDigitCode.img.color = colorEmpty;
            }

            
        }
        
        
    }
}
