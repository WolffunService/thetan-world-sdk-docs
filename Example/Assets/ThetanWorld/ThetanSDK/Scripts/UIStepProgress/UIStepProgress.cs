using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI.Authen.UIProcess;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class UIStepProgress : MonoBehaviour
    {
        [SerializeField] private Slider _sliderProgress;
        [SerializeField] private TextMeshProUGUI _txtCurrentProgress;

        private AuthenProcessInfo _authenProcessInfo;

        private void Awake()
        {
            _txtCurrentProgress.text = string.Empty;
        }

        public void InitDataProcessInfo(AuthenProcessInfo authenProcessInfo)
        {
            _authenProcessInfo = authenProcessInfo;
            
            _sliderProgress.minValue = 1;
            _sliderProgress.maxValue = authenProcessInfo.ListStepProcess.Count;
            _sliderProgress.value = authenProcessInfo.currentStepIndex + 1;

            _txtCurrentProgress.text = authenProcessInfo.ListStepProcess[authenProcessInfo.currentStepIndex].StepName;
        }

        public void ChangeProcessStepIndex(int newStepIndex)
        {
            if (newStepIndex < 0 || newStepIndex >= _authenProcessInfo.ListStepProcess.Count)
            {
                Debug.LogError($"Cannot update UI for new step index {newStepIndex}");
                return;
            }
            
            _sliderProgress.value = newStepIndex + 1;
            _txtCurrentProgress.text = _authenProcessInfo.ListStepProcess[newStepIndex].StepName;
        }
    }
}

