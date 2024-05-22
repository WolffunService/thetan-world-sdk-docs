using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Wolffun.RestAPI;

namespace ThetanSDK.Utilities
{
    public class SetScreenUI : MonoBehaviour
    {

        // Use this for initialization
        [SerializeField] CanvasScaler canvasUi;
        [SerializeField] EventSystem dragHold;
        [SerializeField] Camera scaleModel;

        private float scaler;
        private int width;
        private int height;

        private float GetScale(int width, int height, Vector2 scalerReferenceResolution, float scalerMatchWidthOrHeight)
        {
            return Mathf.Pow(width / scalerReferenceResolution.x, 1f - scalerMatchWidthOrHeight) *
                   Mathf.Pow(height / scalerReferenceResolution.y, scalerMatchWidthOrHeight);
        }

        void Awake()
        {
            SetCanvasScaler();
            SetDragThrHold();
        }

        // canvas scale 
        public void SetCanvasScaler()
        {
            height = Screen.height;
            width = Screen.width;

            if (Utils.GetCurrentScreenType() == Utils.ScreenType.Landscape)
            {
                float sceenScale = 16f / 9;
                scaler = GetScale(height, width, new Vector2(width, height), 1f);
                if (scaler >= sceenScale)
                {
                    if (canvasUi != null)
                        canvasUi.matchWidthOrHeight = 1;
                    if (scaleModel != null)
                        scaleModel.orthographicSize = 5f;
                }
                else
                {
                    if (canvasUi != null)
                        canvasUi.matchWidthOrHeight = 0;
                    if (scaleModel != null)
                        scaleModel.orthographicSize = 6;
                }
            }
            else
            {
                float sceenScale = 9f / 16;
                scaler = GetScale(height, width, new Vector2(width, height), 1f);
                if (scaler >= sceenScale)
                {
                    if (canvasUi != null)
                        canvasUi.matchWidthOrHeight = 1;
                    if (scaleModel != null)
                        scaleModel.orthographicSize = 5f;
                }
                else
                {
                    if (canvasUi != null)
                        canvasUi.matchWidthOrHeight = 0;
                    if (scaleModel != null)
                        scaleModel.orthographicSize = 6;
                }
            }
            //if (scaleModel)
            //scaleModel.orthographicSize = sceenScale / ((float)width/height);
        }

        // set draghold with event system
        public void SetDragThrHold()
        {
            if (dragHold != null)
            {
                int defaultValue = dragHold.pixelDragThreshold;
                dragHold.pixelDragThreshold = Mathf.Max(defaultValue, (int)(defaultValue * Screen.dpi / 160f));
            }

        }
    }
}