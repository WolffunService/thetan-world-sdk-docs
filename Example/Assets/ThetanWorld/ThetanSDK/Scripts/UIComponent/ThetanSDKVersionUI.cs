using System.Collections;
using System.Collections.Generic;
using ThetanSDK;
using TMPro;
using UnityEngine;

namespace ThetanSDK.UI
{
    public class ThetanSDKVersionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _txtVersion;

        void Start()
        {
            _txtVersion.text = ThetanSDKManager.Instance.Version;
        }

    }
}
