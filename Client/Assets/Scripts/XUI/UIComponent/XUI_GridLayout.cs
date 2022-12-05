using System.Collections.Generic;
using UnityEngine;

public class XUI_GridLayout : MonoBehaviour
{
    //public bool FitWidth;
    public bool SortCenter;
    public bool SortOnStart;
    public bool FitHeight;
    public bool CheckActiveByCanvasGroup;
    public Vector2 CellSize;
    public Vector2 Space;

    private List<RectTransform> _activedChilds = new List<RectTransform>();
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

    protected void Start()
    {
        if(SortOnStart)
            Sort();
    }
    
    public void Sort()
    {
        GetChilds();

        float containerWidth = RectTransform.rect.width;
        int col = Mathf.FloorToInt((containerWidth + Space.x) / (CellSize.x + Space.x));
        if (col <= 0)
        {
            //Dbg.LogWarning("一列都放不了？？");
            col = 1;
        }

        var childCount = _activedChilds.Count;
        
        //每列最多几个
        var colCount = childCount > col ? col : childCount;
        var centerX = (colCount - 1) * ((CellSize.x + Space.x) / 2f);
        float startX = SortCenter ? -centerX  :-containerWidth / 2 + CellSize.x / 2;
        float startY;
        
        if (FitHeight)
        {
            int row = Mathf.CeilToInt(childCount / 1f / col);
            float containerHeight = row * CellSize.y + (row - 1 > 0 ? row - 1 : 0) * Space.y;
            RectTransform.pivot = new Vector2(0.5f, 1);
            RectTransform.anchorMin = Vector2.one / 2;
            RectTransform.anchorMax = Vector2.one / 2;
            RectTransform.sizeDelta = new Vector2(containerWidth, containerHeight);
            startY = containerHeight * (1 - RectTransform.pivot.y) - CellSize.y / 2;
        }
        else
        {
            float containerHeight = RectTransform.rect.height;
            startY = containerHeight * (1 - RectTransform.pivot.y) - CellSize.y / 2;
        }


        for (int i = 0; i < childCount; i++)
        {
            RectTransform childRectTransform = _activedChilds[i];
            childRectTransform.anchorMin = new Vector2(.5f, .5f);
            childRectTransform.anchorMax = new Vector2(.5f, .5f);
            childRectTransform.sizeDelta = CellSize;

            int curRow = i / col;
            int curCol = i % col;
            //Debug.Log(string.Format("row:{0}, col:{1}", curRow, curCol));

            float posX = startX + curCol * CellSize.x;
            float posY = startY - curRow * CellSize.y;


            float spaceX = curCol > 0 ? Space.x * curCol : 0;
            float spaceY = curRow > 0 ? Space.y * curRow : 0;

            childRectTransform.localPosition = new Vector2(posX + spaceX, posY - spaceY);
        }
    }

    private void GetChilds()
    {
        _activedChilds.Clear();
        var childCount = RectTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = RectTransform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;
            if (CheckActiveByCanvasGroup)
            {
                var canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup != null && canvasGroup.alpha <= 0)
                    continue;
            }

            try
            {
                var childRectTransform = child as RectTransform;
                if(childRectTransform)
                    _activedChilds.Add(childRectTransform);
            }
            catch (System.InvalidCastException) { }
        }
    }
}
