using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class ButtonOpenUrl : MonoBehaviour
    {
        [SerializeField] private Button _btnOpenUrl;
        [SerializeField] private string _url;

        private void Start()
        {
            _btnOpenUrl.onClick.AddListener(() => Application.OpenURL(_url));
        }
    }
}