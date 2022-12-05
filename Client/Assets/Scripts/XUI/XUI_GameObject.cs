using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class XUI_GameObject : UIBehaviour
{
    private RectTransform _rectTransform;

    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = transform as RectTransform;
            }
            return _rectTransform;
        }
    }

    private CanvasGroup _canvasGroup;
    public CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.CreateCanvasGroup();
            }
            return _canvasGroup;
        }
    }
    
    public void SetActiveByCanvasGroup(bool bActive)
    {
        if (bActive && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        CanvasGroup.SetActiveByCanvasGroup(bActive);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        _rectTransform = null;
    }
}
