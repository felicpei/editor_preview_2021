using UnityEngine;
using UnityEngine.UI;


public class XUI_RawImage : RawImage
{
    public bool UVAnimation = false;
    public float SpeedX = 0f;
    public float SpeedY = 0f;

    private float ox;
    private float oy;
    private float wc;
    private float hc;

    private RectTransform _rectTransform;
    public RectTransform RectTransform
    {
        get
        {    
            if(!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }
    
     public float Alpha
    {
        get { return color.a; }
        set
        {
            var c = color;
            c.a = value;
            color = c;
        }
    }
    
    protected override void Start()
    {
        UpdateUv();
    }
    void Update()
    {
        //if (XGameSetting.QualityLimit(XGameSetting.EQuality.Mid))
        //{
            UpdateUv();
        //}
    }

    private void UpdateUv()
    {
        if (!UVAnimation) return;
        wc = rectTransform.rect.width / mainTexture.width;
        hc = rectTransform.rect.height / mainTexture.height;
        ox += Time.deltaTime * SpeedX;
        oy += Time.deltaTime * SpeedY;
        ox = ox % 1;
        oy = oy % 1;
        uvRect = new Rect(ox, oy, wc, hc);
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

#if UNITY_EDITOR
    static Vector3[] fourCorners = new Vector3[4];
    void OnDrawGizmos()
    {
        if (raycastTarget)
        {
            var rectTransform = transform as RectTransform;
            rectTransform.GetWorldCorners(fourCorners);
            Gizmos.color = Color.green;
            for (var i = 0; i < 4; i++)
                Gizmos.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
        }
    }
#endif
}
