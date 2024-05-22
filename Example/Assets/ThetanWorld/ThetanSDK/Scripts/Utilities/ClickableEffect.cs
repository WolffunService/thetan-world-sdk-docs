using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Wolffun.Log;

namespace ThetanSDK.Utilities
{
    public class ClickableEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float PREVENT_MULTIPLE_BUTTON_CLICK_COOLDOWN = 0.3f;

        [SerializeField] Selectable selectableController;

        [Header("Scale Effect"), Space(16)] [SerializeField]
        private bool isPlayScaleEffect = true;

        [SerializeField] private Vector3 scaleEnd = new Vector3(0.9f, 0.9f, 0.9f);
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private Ease easeIn;
        [SerializeField] private Ease easeOut;

        [Header("Prevent spam"), Space(16)] [SerializeField]
        private bool isPreventSpamClick = true;

        [SerializeField] private int spamCooldownInMs = 400;

        private static float s_lastTimeStampTriggerAction;

        private Tweener scaleTween = default;

        public Tweener CurrentScaleTween => scaleTween;

        private Vector3 defaultSize;

        private byte pointerDownCurrentlyActiveCount = 0;

        private Sprite normalColorEffect;

#if UNITY_EDITOR
        [ContextMenu("Get Selectable Ref")]
        public void Reset()
        {
            selectableController = gameObject.GetComponent<Selectable>();

            if (!selectableController)
            {
                selectableController = gameObject.AddComponent<Selectable>();
            }

            Navigation navigation = selectableController.navigation;
            navigation.mode = Navigation.Mode.None;
            selectableController.navigation = navigation;

            UnityEditor.EditorUtility.SetDirty(this);
        }



#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetWhenPlayDisableReloadDomain()
        {
            s_lastTimeStampTriggerAction = 0;
        }

        private void Awake()
        {
            try
            {
                defaultSize = transform.localScale;

                if (selectableController != null)
                {
                    Navigation navigation = selectableController.navigation;
                    navigation.mode = Navigation.Mode.None;
                    selectableController.navigation = navigation;
                }
            }
            catch
            {
                CommonLog.LogError("ClickableEffect is error: " + gameObject.name);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //prevent duplicate click on button: only consider first pointer down as click action input----------------------------------------------

            pointerDownCurrentlyActiveCount++;

            if (pointerDownCurrentlyActiveCount > 1)
            {
                return;
            }
            //----------------------------------------------------------------------------------------------------------------------------------------


            if (selectableController != null)
            {
                if (!selectableController.interactable || !selectableController.enabled)
                {
                    return;
                }
            }

            if (isPlayScaleEffect)
            {
                if (scaleTween != default)
                    scaleTween.Complete();

                defaultSize = transform.localScale;

                Vector3 targetSize = scaleEnd;

                scaleTween = transform.DOScale(targetSize, duration);

                scaleTween.SetEase(easeIn);
            }

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //prevent duplicate click on button: only consider last pointer up as click action input-------------------------------------------------
            pointerDownCurrentlyActiveCount--;

            if (pointerDownCurrentlyActiveCount > 0)
            {
                return;
            }
            //----------------------------------------------------------------------------------------------------------------------------------------

            if (selectableController != null)
            {
                if (!selectableController.interactable || !selectableController.enabled)
                {
                    return;
                }
            }


            if (isPlayScaleEffect)
            {
                if (scaleTween != default)
                    scaleTween.Complete();

                scaleTween = transform.DOScale(defaultSize, duration);

                scaleTween.SetEase(easeOut);
            }

            if (isPreventSpamClick)
            {
                //prevent spam click on a button: temporarily disable the raycast for the next interval, prevent the user click continously------------------------------

                PreventSpamCoolDownMultipleClick();

                //-------------------------------------------------------------------------------------------------------------------------------------------------------

                //prevent spam click on multiple buttons: temporarily disable the button, so the button click action don't trigger in the current frame------------------

                float curTime = Time.time;
                if (curTime <= s_lastTimeStampTriggerAction + PREVENT_MULTIPLE_BUTTON_CLICK_COOLDOWN)
                {
                    PreventSpamCoolDownMultipleButtonsClick();
                }
                else
                {
                    s_lastTimeStampTriggerAction = curTime;
                }

                //-------------------------------------------------------------------------------------------------------------------------------------------------------

            }
        }

        private async void PreventSpamCoolDownMultipleClick()
        {
            if (!selectableController || !selectableController.image)
                return;

            selectableController.image.raycastTarget = false;

            await UniTask.Delay(spamCooldownInMs);

            if (selectableController || selectableController.image)
            {
                selectableController.image.raycastTarget = true;
            }
        }

        private async void PreventSpamCoolDownMultipleButtonsClick()
        {
            if (!selectableController)
                return;

            selectableController.interactable = false;

            await UniTask.DelayFrame(1);

            if (selectableController)
            {
                selectableController.interactable = true;
            }
        }
    }
}
