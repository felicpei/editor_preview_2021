using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;


[RequireComponent(typeof(XUI_Image))]
public class XUI_Button : Button
{
    private const string ImageName = "ImageDisplay";
    private const string TextName = "Text";

    public delegate void VoidDelegate(GameObject go);

    public VoidDelegate OnDown;
    public VoidDelegate OnUp;
    public VoidDelegate OnEnter;
    public VoidDelegate OnExit;

    protected bool m_isEnable = true;

    public bool isEnable
    {
        set
        {
            if (m_isEnable != value)
            {
                SetButtonEnable(value);
            }

            m_isEnable = value;
        }
        get { return m_isEnable; }
    }

    public bool UseTween;

    public void SetUseTween(bool b)
    {
        if (UseTween != b)
        {
            if (b)
            {
                UseDisplayImage = true;
            }
        }

        UseTween = b;
    }

    private bool _useDisplayImage;

    public bool UseDisplayImage
    {
        set
        {
            if (_useDisplayImage != value)
            {
                SetButtonUseDisplayImage(value);
            }

            _useDisplayImage = value;
        }
        get { return _useDisplayImage; }
    }

    public XUI_Image DisplayImage { private set; get; }

    private RectTransform _rectTransform;

    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = gameObject.GetComponent<RectTransform>();
            }

            return _rectTransform;
        }
    }


    [SerializeField] private bool isDebug;

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if (transform.GetComponent<XUI_Image>() == null)
        {
            transform.gameObject.AddComponent<XUI_Image>();
        }
#endif
        if (transform.localScale.z <= 0)
        {
            var localScale = transform.localScale;
            localScale.z = 1;
            transform.localScale = localScale;
        }

        var img = transform.Find(ImageName);
        DisplayImage = img != null ? img.GetComponent<XUI_Image>() : transform.GetComponent<XUI_Image>();
        
        //debug button
        if (Application.isPlaying && isDebug && !Dbg.IsDebugBuild)
        {
            SetActiveByCanvasGroup(false);
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

    public void SetActiveByCanvasGroup(bool b)
    {
        if (b && isDebug && !Dbg.IsDebugBuild)
        {
            return;
        }

        CanvasGroup.SetActiveByCanvasGroup(b);
    }

    public bool GetActive()
    {
        return CanvasGroup.alpha >= 1;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnDown?.Invoke(gameObject);

        if (UseTween)
        {
            OnDownTween();
        }
    }

    private Tween _tween;

    private Vector3 _originalScale;

    private void OnDownTween()
    {
        if (!isEnable)
        {
            return;
        }

        KillTween();

        _originalScale = DisplayImage.RectTransform.localScale;
        _tween = DisplayImage.transform.DOScale(_originalScale * 0.85f, 0.1f);
    }

    private void OnUpTween()
    {
        KillTween();
    }

    private void KillTween()
    {
        _tween?.Kill();
        RestScale();
    }

    public void RestScale()
    {
        if (_originalScale == Vector3.zero) _originalScale = Vector3.one;
        DisplayImage.transform.localScale = _originalScale * 1;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        OnUp?.Invoke(gameObject);
        if (UseTween)
        {
            OnUpTween();
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnEnter?.Invoke(gameObject);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnExit?.Invoke(gameObject);
    }

    public void FormatStyle()
    {
        var txt = GetComponentsInChildren<Text>();
        for (var i = 0; i < txt.Length; i++)
        {
            txt[i].fontSize = 26;
            txt[i].rectTransform.anchorMin = Vector2.zero;
            txt[i].rectTransform.anchorMax = Vector2.one;
            txt[i].rectTransform.anchoredPosition = Vector2.zero;
            txt[i].rectTransform.sizeDelta = Vector2.zero;
        }

        GetComponent<XUI_Image>().Alpha = 0;
        if (DisplayImage != null)
        {
            DisplayImage.rectTransform.anchorMin = Vector2.zero;
            DisplayImage.rectTransform.anchorMax = Vector2.one;
            DisplayImage.rectTransform.anchoredPosition = Vector2.zero;
            DisplayImage.rectTransform.sizeDelta = Vector2.zero;
        }
    }


    public void SetButtonUseDisplayImage(bool bDisplayImage)
    {
#if UNITY_EDITOR

        //如果是use tween, 需要区分点击区域与事件区域
        if (!enabled) return;
        var sourceImage = GetComponent<XUI_Image>();
        if (sourceImage == null) return;
        if (!sourceImage.enabled) return;

        var img = transform.Find(ImageName);
        if (bDisplayImage)
        {
            XUI_Image displayImage = null;
            if (img == null)
            {
                var imageObj = new GameObject();
                imageObj.layer = Layers.UI;
                imageObj.name = ImageName;
                imageObj.transform.SetParent(transform);
                displayImage = imageObj.AddComponent<XUI_Image>();
            }
            else
            {
                img.gameObject.layer = Layers.UI;
                if (img.gameObject.activeSelf == false)
                {
                    img.gameObject.SetActiveSafe(true);
                    displayImage = img.GetComponent<XUI_Image>();
                }
            }

            if (displayImage != null)
            {
                displayImage.rectTransform.sizeDelta = sourceImage.rectTransform.sizeDelta;
                displayImage.RectTransform.anchoredPosition = Vector2.zero;
                displayImage.color = sourceImage.color;
                displayImage.material = sourceImage.material;
                displayImage.type = sourceImage.type;
                displayImage.fillAmount = sourceImage.fillAmount;
                displayImage.fillCenter = sourceImage.fillCenter;
                displayImage.sprite = sourceImage.sprite;
                displayImage.raycastTarget = false;
                displayImage.transform.SetAsFirstSibling();
                sourceImage.Alpha = 0;

                var objs = transform.GetComponentsInChildren<Transform>(true);
                for (var i = 0; i < objs.Length; i++)
                {
                    if (objs[i].parent != transform) continue;
                    if (objs[i] == transform) continue;
                    if (objs[i] == displayImage.transform) continue;
                    objs[i].SetParent(displayImage.transform);
                }
            }
        }
        else
        {
            if (img != null)
            {
                var objs = img.transform.GetComponentsInChildren<Transform>(true);
                for (var i = 0; i < objs.Length; i++)
                {
                    if (objs[i].parent != img.transform) continue;
                    if (objs[i] == transform) continue;
                    objs[i].SetParent(transform);
                }

                img.gameObject.SetActiveSafe(false);
            }

            sourceImage.Alpha = 1;
        }

#endif
    }

    private void SetButtonEnable(bool enable)
    {
        interactable = enable;
        if (DisplayImage == null)
        {
            return;
        }
        
        DisplayImage.material = enable ? InternalResource.Inst.ImageDefault : InternalResource.Inst.ImageGray;
        var xUI_Images = DisplayImage.GetComponentsInChildren<XUI_Image>();
        foreach (var img in xUI_Images)
        {
            img.material = enable ? InternalResource.Inst.ImageDefault : InternalResource.Inst.ImageGray;
        }
    }

    protected override void OnDestroy()
    {
        _rectTransform = null;
    }
}