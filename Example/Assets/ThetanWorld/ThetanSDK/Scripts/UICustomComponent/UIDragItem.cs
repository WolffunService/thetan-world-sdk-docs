using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace ThetanSDK.UI.CustomComponent
{
    public class UIDragItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Drag UI")] [SerializeField] private RectTransform dragObj;
        [SerializeField] private bool isEffectUIDrag;
        [SerializeField] private DragType dragType;
        [SerializeField] private DirHVType dirType;
        [SerializeField] private float ratio = 1f;
        [SerializeField] private Button _btn;

        [Header("Resize UI")] [SerializeField] private float minSize;
        [SerializeField] private float maxSize;
        [SerializeField] private bool isContrain;

        [Header("Limit On Canvas Screen")] [SerializeField]
        private bool limitOnCanvas;

        [SerializeField] private RectTransform canvas;
        [SerializeField] private Vector2 offset;


        [Header("Camera")] [SerializeField] private string cameraName;

        private Action<PointerEventData> callbacks;
        public RectTransform DragRect => dragObj;
        public Vector2 Scale => new Vector2(dragObj.rect.width, dragObj.rect.height);
        private Vector2 _minCanvasPosition;
        private Vector2 _maxCanvasPosition;
        private Vector3 _prevDir;
        private Camera _cameraViewport;
        private float _totalDistanceMoveButton;

        private const float TOTAL_DISTANCE_DISABLE_BUTTON = 25;

        private void Start()
        {
            if (string.IsNullOrEmpty(cameraName))
            {
                _cameraViewport = Camera.main;
            }
            else
            {
                var cam = GameObject.Find(cameraName);
                var camViewport = cam.GetComponent<Camera>();
                _cameraViewport = camViewport ? camViewport : Camera.main;
            }


            if (!limitOnCanvas) return;
            var position = canvas.position;
            var rect = canvas.rect;
            _minCanvasPosition = new Vector2(
                -rect.width / 2 + dragObj.rect.width / 2,
                -rect.height / 2 + dragObj.rect.height / 2
            ) + offset;

            _maxCanvasPosition = new Vector2(
                rect.width / 2 - dragObj.rect.width / 2,
                rect.height / 2 - dragObj.rect.height / 2
            ) - offset;

        }

        public void Subscribe(Action<PointerEventData> callback)
        {
            callbacks -= callback;
            callbacks += callback;
        }

        public void Unsubscribe(Action<PointerEventData> callback)
        {
            callbacks -= callback;
        }

        public void OnPointerDown(PointerEventData data)
        {
            _totalDistanceMoveButton = 0;
        }

        public void OnPointerUp(PointerEventData data)
        {
            _prevDir = Vector3.zero;
            
            if (_totalDistanceMoveButton >= TOTAL_DISTANCE_DISABLE_BUTTON && _btn)
            {
                Wait1FrameToTurnOnButton();
            }
            
            _totalDistanceMoveButton = 0;
        }

        private async void Wait1FrameToTurnOnButton()
        {
            await UniTask.DelayFrame(1);

            if (_btn)
                _btn.interactable = true;
        }

        public void OnDrag(PointerEventData data)
        {
            callbacks?.Invoke(data);
            float x = (dirType & DirHVType.HORIZONTAL) == DirHVType.HORIZONTAL ? data.delta.x * ratio : 0;
            float y = (dirType & DirHVType.VERTICAL) == DirHVType.VERTICAL ? data.delta.y * ratio : 0;

            _totalDistanceMoveButton += x * x + y * y;

            if (_totalDistanceMoveButton >= TOTAL_DISTANCE_DISABLE_BUTTON && _btn)
            {
                _btn.interactable = false;
            }

            switch (dragType)
            {
                case DragType.MOVE:
                    if (isEffectUIDrag)
                    {
                        var position = dragObj.position + new Vector3(x, y);
                        if (limitOnCanvas)
                        {
                            var localPosition = canvas.transform.InverseTransformPoint(position);
                            
                            var lx = Mathf.Clamp(localPosition.x, _minCanvasPosition.x, _maxCanvasPosition.x);
                            var ly = Mathf.Clamp(localPosition.y, _minCanvasPosition.y, _maxCanvasPosition.y);
                            localPosition = new Vector2(lx, ly);

                            position = canvas.transform.TransformPoint(localPosition);
                        }

                        dragObj.position = position;
                    }

                    break;

                case DragType.ROTATE:
                    var dir = (_cameraViewport.ScreenToViewportPoint(data.position) -
                               _cameraViewport.ScreenToViewportPoint(dragObj.transform.position));

                    var angle = (Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg);
                    if (isEffectUIDrag)
                        dragObj.rotation = Quaternion.Euler(0, 0, -angle);
                    break;

                case DragType.SCALE:
                    if (!isEffectUIDrag) return;

                    var dirs = (_cameraViewport.ScreenToViewportPoint(data.position) -
                                _cameraViewport.ScreenToViewportPoint(dragObj.transform.position));

                    if (dirs.x * _prevDir.x < 0 || dirs.y * _prevDir.y < 0) return;

                    int ax = dirs.x > 0 ? 1 : -1;
                    int ay = dirs.y > 0 ? 1 : -1;
                    float s = Mathf.Abs(data.delta.x) > Mathf.Abs(data.delta.y) ? data.delta.x * ax : data.delta.y * ay;
                    if (isContrain)
                    {
                        float sizeX = Mathf.Clamp(dragObj.sizeDelta.x + s, minSize, maxSize);
                        float sizeY = Mathf.Clamp(dragObj.sizeDelta.y + s, minSize, maxSize);
                        dragObj.sizeDelta = new Vector2(sizeX, sizeY);
                    }
                    else
                    {
                        float sizeX = Mathf.Clamp(dragObj.sizeDelta.x + data.delta.x * ax, minSize, maxSize);
                        float sizeY = Mathf.Clamp(dragObj.sizeDelta.y + data.delta.y * ay, minSize, maxSize);
                        dragObj.sizeDelta = new Vector2(sizeX, sizeY);
                    }

                    _prevDir = dirs;
                    break;
            }
        }

        public void SetSize(Vector2 size)
        {
            float sizeX = Mathf.Clamp(size.x, minSize, maxSize);
            float sizeY = Mathf.Clamp(size.y, minSize, maxSize);
            dragObj.sizeDelta = new Vector2(sizeX, sizeY);
            callbacks?.Invoke(null);
        }

        public void SetMinMaxSize(float min, float max)
        {
            minSize = min;
            maxSize = max;
        }

        public void SetObjectDrag(RectTransform obj)
        {
            dragObj = obj;
        }
    }

    enum DragType
    {
        MOVE,
        ROTATE,
        SCALE,
    }

    [System.Flags]
    enum DirHVType
    {
        nothing = 0,
        HORIZONTAL = 1 << 0,
        VERTICAL = 1 << 1,
        everything
    }
}
