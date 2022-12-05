using System;
using UnityEngine.UI;
using UnityEngine;

public class XUI_Text : Text
{
    //翻译开关
    public bool needLocalization = true;
    
    //当切换语言的时候，遍历languageChanged发事件，遍历过程中，销毁了XUI_TEXT，但是遍历仍会继续，所以Refresh还有机会再次发生，所以需要做个开关阻止Refresh。
    private bool _isDestroyed;            
  
    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }
    protected override void Start()
    {
        base.Start();
        if (needLocalization)
        {
            GameEventCenter.AddListener(GameEvent.LanguageChanged, Refresh);
        }
    }

    private void Refresh(object o = null)
    {
        if (_isDestroyed)
        {
            return;
        }
        SetAllDirty();
    }


    protected override void OnDestroy()
    {
        if (needLocalization)
        {
            GameEventCenter.RemoveListener(GameEvent.LanguageChanged, Refresh);
        }
        
        _isDestroyed = true;
        base.OnDestroy();
    }

    public string GetBaseText()
    {
        return base.text;
    }

    public float TextAlpha
    {
        get => color.a;
        set
        {
            var c = color;
            c.a = value;
            color = c;
        }
    }
    
    public override float preferredHeight => cachedTextGeneratorForLayout.GetPreferredHeight(text, GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f))) / pixelsPerUnit;

    public override float preferredWidth => cachedTextGeneratorForLayout.GetPreferredWidth(text, GetGenerationSettings(Vector2.zero)) / pixelsPerUnit;

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
    
    public override string text
    {
        get
        {
#if UNITY_EDITOR
                //if(UnityEditor.EditorApplication.isPlaying && needLocalization)
                    //return LanguageMgr.instance.GetLocalizedString(base.text);
                return base.text;
#else
                //if (needLocalization)
                    //return LanguageMgr.instance.GetLocalizedString(base.text);
                return base.text;
            #endif
        }
        set
        {
            if (value != base.text)
            {
                base.text = value;
                if (underLine)
                {
                    SyncText();
                }
            }
        }
    }

    public new int fontSize
    {
        get => base.fontSize;
        set
        {
            base.fontSize = value;
            if (underLine)
            {
                SyncText();
            }
        }
    }

    public new Color color
    {
        get => base.color;
        set
        {
            base.color = value;
            if (underLine)
            {
                SyncText();
            }
        }
    }


    private CanvasGroup _canvasGroup;
    public CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = XUI_Utility.CreateCanvasGroup(gameObject);
            }
            return _canvasGroup;
        }
    }
    public void SetActiveByCanvasGroup(bool b)
    {
        CanvasGroup.SetActiveByCanvasGroup(b);
    }

    public float Width
    {
        get => rectTransform.sizeDelta.x;
        set
        {
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = value;
            rectTransform.sizeDelta = sizeDelta;
        }
    }

    public bool underLine
    {
        get => m_underLine;
        set
        {
            var sync = m_underLine && !value;
            if (!m_underLine && value)
            {
                sync = true;
            }

            m_underLine = value;
            if (sync)
            {
                SyncText();
            }
        }
    }
    [SerializeField]
    protected bool m_underLine;
    protected XUI_Text m_textUnderLine;

    public void SyncText()
    {
        if (m_underLine)
        {
            var textT = transform.Find("underline");
            if (textT == null)
            {
                var textGo = new GameObject();
                textGo.name = "underline";
                textGo.transform.SetParent(gameObject.transform);
                textGo.transform.SetAsLastSibling();
                textGo.layer = gameObject.layer;
                m_textUnderLine = textGo.AddComponent<XUI_Text>();
                m_textUnderLine.fontSize = fontSize;
                m_textUnderLine.material = material;
                m_textUnderLine.alignment = alignment;
                m_textUnderLine.needLocalization = false;
                
                var rt = m_textUnderLine.rectTransform;
                //设置下划线坐标和位置  
                rt.anchoredPosition3D = Vector3.zero;
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.anchorMin = Vector2.zero;

                m_textUnderLine.color = color;
                m_textUnderLine.text = "_";
                var perLineWidth = m_textUnderLine.preferredWidth;      //单个下划线宽度  
                var width = preferredWidth;
                var lineCount = (int)Mathf.RoundToInt(width / perLineWidth);
                for (var i = 1; i <= lineCount; i++)
                {
                    m_textUnderLine.text += "_";
                }

                m_textUnderLine.raycastTarget = false;
            }
            else
            {
                m_textUnderLine = textT.GetComponent<XUI_Text>();
                if (m_textUnderLine != null)
                {
                    m_textUnderLine.fontSize = fontSize;
                    m_textUnderLine.material = material;
                    m_textUnderLine.alignment = alignment;
                    var rt = m_textUnderLine.rectTransform;
                    //设置下划线坐标和位置  
                    rt.anchoredPosition3D = Vector3.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.offsetMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.anchorMin = Vector2.zero;
                    m_textUnderLine.color = color;
                    m_textUnderLine.text = "_";
                    var perlineWidth = m_textUnderLine.preferredWidth;      //单个下划线宽度  
                    var width = preferredWidth;
                    var lineCount = (int)Mathf.Round(width / perlineWidth);
                    for (var i = 1; i < lineCount; i++)
                    {
                        m_textUnderLine.text += "_";
                    }
                    m_textUnderLine.raycastTarget = false;
                }
            }
        }
        else
        {
            m_textUnderLine = null;
            var textT = transform.Find("underline");
            if (textT != null)
            {
                DestroyImmediate(textT.gameObject, true);
            }
        }
    }

#if UNITY_EDITOR
    private static readonly Vector3[] FourCorners = new Vector3[4];
    void OnDrawGizmos()
    {
        if (raycastTarget)
        {
            var rect = transform as RectTransform;
            if (rect != null)
            {
                rect.GetWorldCorners(FourCorners);
            }
            
            Gizmos.color = Color.green;
            for (var i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(FourCorners[i], FourCorners[(i + 1) % 4]);
            }
        }
    }

    public void AddOutLine()
    {
        var shadow = GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = gameObject.AddComponent<Shadow>();
        }
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.useGraphicAlpha = true;

        var outLine = GetComponent<Outline>();
        if (outLine == null)
        {
            outLine = gameObject.AddComponent<Outline>();
        }
        outLine.effectDistance = new Vector2(1f, -1f);
        outLine.effectColor = new Color(0f, 0f, 0f, 0.7f);
        outLine.useGraphicAlpha = true;
    }

    public void RemoveOutLine()
    {
        var outLine = GetComponent<Outline>();
        if (outLine != null)
        {
            DestroyImmediate(outLine);
        }

        var shadow = GetComponent<Shadow>();
        if (shadow != null)
        {
            DestroyImmediate(shadow);
        }

    }
#endif
}
