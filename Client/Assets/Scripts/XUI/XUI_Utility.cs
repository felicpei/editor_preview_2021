using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum EAtlas
{
    
}

public static class XUI_Utility
{
    public static CanvasGroup CreateCanvasGroup(this GameObject uiObject)
    {
        var group = uiObject.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = uiObject.gameObject.AddComponent<CanvasGroup>();
        }
        group.ignoreParentGroups = false;
        return group;
    }

    public static void SetSprite(this XUI_Image image, EAtlas path, string spriteName, bool nativeSize = false)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            //Debug.LogError("XUI_Image SetSprite 不要为空! path:" + path + " spriteName:" + spriteName);
            image.Sprite = null;
            image.SetActiveByCanvasGroup(false);
        }
        else
        {
            var bSame = image.Sprite != null && spriteName == image.Sprite.name;
            if (!bSame)
            {
                AtlasLoader.LoadSprite(path.ToString(), atlas =>
                {
                    image.Sprite = atlas.GetSprite(spriteName);
                    if (nativeSize) image.SetNativeSize();
                    image.SetActiveByCanvasGroup(true);
                });
            }
        }
    }

    public static void SetRawImageTexture(this RawImage rawImage, string path, bool resetAlpha = true)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        XResource.Load(path, o =>
        {
            var texture = o as Texture;
            if (rawImage)
            {
                rawImage.texture = texture;
                if (resetAlpha)
                {
                    var color = rawImage.color;
                    color.a = 1f;
                    rawImage.color = color;
                }
            }
        });
    }

    public static string ToPercent(this float num, int digits = 0)
    {
        if (digits <= 0)
        {
            return Mathf.FloorToInt(num * 100 + 0.5f) + "%";
        }
        return Math.Round(num * 100, digits) + "%";
    }

    public static string TransNumTo_K_M(this int num)
    {
        if (num >= 100000000)
        {
            return string.Format("{0}M", Math.Round(num / 1000000f, 0));
        }

        if(num >= 10000000)
        {
            return string.Format("{0:0.0}M", Math.Round(num / 1000000f, 1));
        }
        if (num >= 1000000)
        {
            return string.Format("{0:0.00}M", Math.Round(num / 1000000f, 2));
        }
        if (num > 100000)
        {
            return string.Format("{0}K", Math.Round(num / 1000f, 0));
        }
        if (num > 10000)
        {
            return string.Format("{0:0.0}K", Math.Round(num / 1000f, 1));
        }
        if (num > 1000)
        {
            return string.Format("{0:0.00}K", Math.Round(num / 1000f, 2));
        }

        return num.ToString();
    }

    public static string TransNumTo_K_M(this long num)
    {
        var isMinus = num < 0;
        if (isMinus)
        {
            num = -num;
        }
        var str = DoTransNumTo_K_M(num);
        if (isMinus)
        {
            str = "-" + str;
        }
        
        return str;
    }

    private static string DoTransNumTo_K_M(long num)
    {
        
        if (num >= 100000000000000)
        {
            return string.Format("{0}T", Math.Round(num / 1000000000000f, 0));
        }

        if (num >= 10000000000000)
        {
            return string.Format("{0:0.0}T", Math.Round(num / 1000000000000f, 1));
        }
        if (num >= 1000000000000)
        {
            return string.Format("{0:0.00}T", Math.Round(num / 1000000000000f, 2));
        }
        if (num >= 100000000000)
        {
            return string.Format("{0}B", Math.Round(num / 1000000000f, 0));
        }
        if (num >= 10000000000)
        {
            return string.Format("{0:0.0}B", Math.Round(num / 1000000000f, 1));
        }
        if (num >= 1000000000)
        {
            return string.Format("{0:0.00}B", Math.Round(num / 1000000000f, 2));
        }
        if (num >= 100000000)
        {
            return string.Format("{0}M", Math.Round(num / 1000000f, 0));
        }
        if (num >= 10000000)
        {
            return string.Format("{0:0.0}M", Math.Round(num / 1000000f, 1));
        }
        if (num >= 1000000)
        {
            return string.Format("{0:0.00}M", Math.Round(num / 1000000f, 2));
        }
        if (num > 100000)
        {
            return string.Format("{0}K", Math.Round(num / 1000f, 0));
        }
        if (num > 10000)
        {
            return string.Format("{0:0.0}K", Math.Round(num / 1000f, 1));
        }
        if (num > 1000)
        {
            return string.Format("{0:0.00}K", Math.Round(num / 1000f, 2));
        }

        return num.ToString();
    }

    public static Vector2 WorldPosToUguiAnchoredPosition(Canvas canvas, Vector3 worldPos)
    {
        //得到画布的尺寸
        var uiSize = canvas.GetComponent<RectTransform>().sizeDelta;
        
        //将世界坐标转换为屏幕坐标
        Vector2 screenPos = CameraHelper.Camera.WorldToScreenPoint(worldPos);
        
        //转换为以屏幕中心为原点的屏幕坐标
        Vector2 screenPos2;
        screenPos2.x = screenPos.x - Screen.width / 2f;
        screenPos2.y = screenPos.y - Screen.height / 2f;
        
        //得到UGUI的anchoredPosition
        Vector2 uiPos;
        uiPos.x = screenPos2.x / Screen.width * uiSize.x;
        uiPos.y = screenPos2.y / Screen.height * uiSize.y;
        return uiPos;
    }
    
    
    public static int GetActivityChildCount(this Transform transform)
    {
        var count = 0;
        for (var i = 0; i < transform.childCount; i++)
        {
            var tran = transform.GetChild(i);
            if (tran.gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }
}
