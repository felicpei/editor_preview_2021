
//////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;


public class XUI_CommonMono : MonoBehaviour
{
    private IXUI _inst;
    public bool Destroyed;
    
    public void Init(IXUI inst)
    {
        _inst = inst;
        _inst.OnInit();
    }
    
    public void OnDestroy()
    {
        if (!Destroyed)
        {
            Destroyed = true;
            XUI_Manager.OnUIDestroy(_inst);
            _inst.OnDestroy();
            _inst = null;
        }
    }
}
