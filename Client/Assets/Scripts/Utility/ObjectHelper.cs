using UnityEngine;

public static class ObjectHelper
{
    public static GameObject CreateGameObject()
    {
        return new GameObject();
    }

    public static GameObject CreateGameObject(string name, params System.Type[] components)
    {
        return new GameObject(name, components);
    }
    
    public static void DontDestroyOnSceneChanged(this GameObject gameObject)
    {
        Object.DontDestroyOnLoad(gameObject);
    }

    public static T AddComponentIfNotExists<T>(this GameObject gameObj) where T : MonoBehaviour
    {
        var component = gameObj.GetComponent<T>();
        if (component == null)
        {
            component = gameObj.AddComponent<T>();
        }
        return component;
    }
    
    public static GameObject InstantiateGo(this Object source)
    {
        var result = Object.Instantiate(source) as GameObject;
        return result;
    }
    
    public static GameObject InstantiateGo(this Object source, Transform parent)
    {
        var result =  Object.Instantiate(source, parent) as GameObject;
        return result;
    }
    
    public static GameObject InstantiateGo(this Object source, Transform parent, bool worldStay)
    {
        var result =  Object.Instantiate(source, parent, worldStay) as GameObject;
        return result;
    }
    
    public static GameObject InstantiateGo(this Object source, Vector3 position, Quaternion rotation)
    {
        var result =  Object.Instantiate(source, position, rotation) as GameObject;
        return result;
    }

    private static Transform GetTransform(Object target)
    {
        var gameObject = target as GameObject;
        if (gameObject)
        {
            return gameObject.transform;
        }
        
        var transform = target as Transform;
        if (transform)
        {
            return transform;
        }

        var component = target as Component;
        if (component)
        {
            return component.transform;
        }
       
        return null;
    }
    
    public static GameObject Find(Object target, string path)
    {
        GameObject targetGo = null;
        var transform = GetTransform(target);
        if (transform != null)
        {
            transform = transform.Find(path);
            if (transform != null)
            {
                targetGo = transform.gameObject;
            }
        }
        return targetGo;
    }
    
    public static GameObject Lookup(Object target, string name)
    {
        var transform = GetTransform(target);
        if (transform != null)
        {
            foreach (Transform t in transform)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }

                var findGameObject = Lookup(t.gameObject, name);
                if (findGameObject != null)
                {
                    return findGameObject;
                }
            }
        }
        return null;
    }

    public static void DestroyAllChild(this GameObject obj, string dontDestroyName = null)
    {
        foreach (Transform child in obj.transform)
        {
            if (!string.IsNullOrEmpty(dontDestroyName) && child.gameObject.name.Contains(dontDestroyName))
            {
                continue;
            }

            Object.Destroy(child.gameObject);
        }

        obj.transform.DetachChildren();
    }

    public static void SetLayer(this Transform transform, int layer)
    {
        if (transform.gameObject.layer == layer) return;
        if (transform)
        {
            var ts = transform.GetComponentsInChildren<Transform>();
            if (ts != null)
            {
                var count = ts.Length;
                for (var i = 0; i < count; i++)
                {
                    var t = ts[i];
                    t.gameObject.layer = layer;
                }
            }
        }
    }

    public static void SetActiveSafe(this GameObject gameObj, bool bActive)
    {
        if (gameObj != null)
        {
            if (gameObj.activeSelf != bActive)
            {
                gameObj.SetActive(bActive);
            }   
        }
    }

    public static bool SetActiveByCanvasGroup(this CanvasGroup canvas, bool bActive)
    {
        if (bActive && !canvas.gameObject.activeSelf)
        {
            canvas.gameObject.SetActive(true);
        }

        if (canvas.GetActiveState() != bActive)
        {
            canvas.alpha = bActive ? 1 : 0;
            canvas.blocksRaycasts = bActive;
            canvas.interactable = bActive;
            return true;
        }

        return false;
    }
    
    public static bool GetActiveState(this CanvasGroup c)
    {
        return c.alpha > 0;
    }

    public static bool GetActiveState(this XUI_GameObject c)
    {
        return c.CanvasGroup.alpha > 0;
    }

    public static bool GetActiveState(this XUI_Image c)
    {
        return c.CanvasGroup.alpha > 0;
    }
}