using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class XUI_GraphicRaycaster : GraphicRaycaster
{
    protected override void Awake()
    {
        base.Awake();
        _canvas = GetComponent<Canvas>();
    }

    public Camera TargetCamera;

    public override Camera eventCamera
    {
        get
        {
            if (TargetCamera == null)
            {
                TargetCamera = base.eventCamera;
            }

            return TargetCamera;
        }
    }

    private Canvas _canvas;

    [NonSerialized] private readonly List<Graphic> _raycastResults = new List<Graphic>();

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (_canvas == null)
            return;

        var canvasGraphics = GraphicRegistry.GetGraphicsForCanvas(_canvas);
        if (canvasGraphics == null || canvasGraphics.Count == 0)
            return;

        int displayIndex;
        var currentEventCamera = eventCamera; // Propery can call Camera.main, so cache the reference

        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay || currentEventCamera == null)
        {
            displayIndex = _canvas.targetDisplay;
        }
        else
        {
            displayIndex = currentEventCamera.targetDisplay;
        }

        var eventPosition = Display.RelativeMouseAt(eventData.position);
        if (eventPosition != Vector3.zero)
        {
            var eventDisplayIndex = (int)eventPosition.z;
            if (eventDisplayIndex != displayIndex)
            {
                return;
            }
        }
        else
        {
            eventPosition = eventData.position;
        }

        // Convert to view space
        Vector2 pos;
        if (currentEventCamera == null)
        {
            float w = Screen.width;
            float h = Screen.height;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                w = Display.displays[displayIndex].systemWidth;
                h = Display.displays[displayIndex].systemHeight;
            }

            pos = new Vector2(eventPosition.x / w, eventPosition.y / h);
        }
        else
        {
            pos = currentEventCamera.ScreenToViewportPoint(eventPosition);
        }

        if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
        {
            return;
        }

        var hitDistance = float.MaxValue;
        var ray = new Ray();
        if (currentEventCamera != null)
        {
            ray = currentEventCamera.ScreenPointToRay(eventPosition);
        }

        _raycastResults.Clear();
        Raycast2(currentEventCamera, eventPosition, canvasGraphics, _raycastResults);
        var totalCount = _raycastResults.Count;
        for (var index = 0; index < totalCount; index++)
        {
            var go = _raycastResults[index].gameObject;
            var appendGraphic = true;

            if (ignoreReversedGraphics)
            {
                if (currentEventCamera == null)
                {
                    var dir = go.transform.rotation * Vector3.forward;
                    appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
                }
                else
                {
                    var cameraForward = currentEventCamera.transform.rotation * Vector3.forward;
                    var dir = go.transform.rotation * Vector3.forward;
                    appendGraphic = Vector3.Dot(cameraForward, dir) > 0;
                }
            }

            if (appendGraphic)
            {
                float distance;
                if (currentEventCamera == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    distance = 0;
                }
                else
                {
                    var trans = go.transform;
                    var transForward = trans.forward;
                    distance = Vector3.Dot(transForward, trans.position - currentEventCamera.transform.position) / Vector3.Dot(transForward, ray.direction);
                    if (distance < 0)
                    {
                        continue;
                    }
                }

                if (distance >= hitDistance)
                {
                    continue;
                }
                var castResult = new RaycastResult
                {
                    gameObject = go,
                    module = this,
                    distance = distance,
                    screenPosition = eventPosition,
                    index = resultAppendList.Count,
                    depth = _raycastResults[index].depth,
                    sortingLayer = _canvas.sortingLayerID,
                    sortingOrder = _canvas.sortingOrder
                };
                resultAppendList.Add(castResult);
            }
        }
    }

    /// <summary>
    /// Perform a raycast into the screen and collect all graphics underneath it.
    /// </summary>
    private static void Raycast2(Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
    {
        var totalCount = foundGraphics.Count;
        Graphic upGraphic = null;
        var upIndex = -1;

        for (var i = 0; i < totalCount; ++i)
        {
            var graphic = foundGraphics[i];
            if (graphic == null)
            {
                continue;
            }

            if (!graphic.raycastTarget)
            {
                continue;
            }

            var depth = graphic.depth;
            if (depth == -1 || graphic.canvasRenderer.cull)
            {
                continue;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera))
            {
                continue;
            }

            if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
            {
                continue;
            }

            if (graphic.Raycast(pointerPosition, eventCamera))
            {
                if (depth > upIndex)
                {
                    upIndex = depth;
                    upGraphic = graphic;
                }
            }
        }

        if (upGraphic != null)
        {
            results.Add(upGraphic);
        }
    }
}