using UnityEngine;
using System.Collections;

public abstract class SceneBase : MonoBehaviour
{
    public string SceneName;
    public SceneDeploy SceneDeploy;
    
    private void Awake()
    {
        SceneLoader.OnSceneEnter(this);
    }

    private void OnDestroy()
    {
        OnLeave();
        SceneLoader.OnSceneLeave(this);
    }

    public virtual IEnumerator Init()
    {
        yield break;
    }

    public virtual void OnLeave()
    {
        GameWorld.OnSceneLeave();
    }
}