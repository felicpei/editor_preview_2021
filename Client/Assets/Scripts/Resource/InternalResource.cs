using System;
using UnityEngine;

public class InternalResource : MonoBehaviour
{
    public Material ImageDefault;
    public Material ImageGray;
    public Mesh Quad;
        
    public static InternalResource Inst;

    private void Awake()
    {
        Inst = this;
    }
}