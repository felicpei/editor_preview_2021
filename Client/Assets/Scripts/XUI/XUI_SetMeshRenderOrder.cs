using UnityEngine;
using System.Collections;

public class XUI_SetMeshRenderOrder : MonoBehaviour 
{
    public SoringLayer SoringLayer;
    public int sortingOrder;

    private void OnEnable()
    {
        Sort();
    }

    public void Sort()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>(true);
        for(int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if(r)
            {
                r.sortingLayerID = SortingLayer.NameToID(SoringLayer.ToString());
                r.sortingOrder = sortingOrder;
            }
        }
    }
    
    public void SortById(int layerId, int orderId)
    {
        var renderers = GetComponentsInChildren<MeshRenderer>(true);
        for(int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if(r)
            {
                r.sortingLayerID = layerId;
                r.sortingOrder = orderId;
            }
        }
    }
}