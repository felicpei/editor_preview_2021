using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasLoader : MonoBehaviour
{
    private static AtlasLoader _inst;
    
    public static void Init()
    {
        if (_inst != null)
        {
            return;
        }

        var spriteMgr = new GameObject("SpriteAtlas");
        _inst = spriteMgr.AddComponent<AtlasLoader>();
        DontDestroyOnLoad(spriteMgr);
        SpriteAtlasManager.atlasRequested += LoadSprite;
    }

    public static void ClearCache()
    {
        using (var enumerator = SpriteCache.GetEnumerator())
        {
            using (enumerator)
            {
                Resources.UnloadAsset(enumerator.Current.Value);
            }
        }

        SpriteCache.Clear();
    }

    private static readonly Dictionary<string, SpriteAtlas> SpriteCache = new();

    public static void LoadSprite(string t, Action<SpriteAtlas> action)
    {
        //缓存有，直接取缓存中的
        if (SpriteCache.TryGetValue(t, out var spiritList))
        {
            action(spiritList);
            return;
        }

        //读取图集
        var fullPath = string.Format("Atlas/{0}.spriteatlas", t);
        XResource.Load(fullPath, obj =>
        {
            var sa = obj as SpriteAtlas;
            SpriteCache[t] = sa;
            action(sa);    
        });
    }
}
