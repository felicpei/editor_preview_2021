
using UnityEngine;

public interface IXUI
{
    public void BeforeInit(GameObject gameObject, XUI_Layer layerName);

    public XUI_Layer GetLayer();
    
    public GameObject GetGameObject();
    
    public uint GetUniqueId();
    
    public string GetPrefabPath();

    public void Update();

    public void OnInit();

    public void OnShow();

    public void OnDestroy();
}
