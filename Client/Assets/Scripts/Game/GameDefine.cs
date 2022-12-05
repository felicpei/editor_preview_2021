
using UnityEngine;

public enum SoringLayer
{
    Splash = 0,
    Main = 1,
    View = 2,
    Tips = 4,
    Loading = 5,
    Default = 6,
}

public static class Layers
{
    public const int Default  = 0;
    public const int TransparentFx = 1;
    public const int IgnoreRaycast = 2;
    public const int Water = 4;
    public const int UI = 5;
  
    public static SoringLayer GetSortingLayer(XUI_Layer uiLayer)
    {
        return uiLayer switch
        {
            XUI_Layer.Splash => SoringLayer.Splash,
            XUI_Layer.Main => SoringLayer.Main,
            XUI_Layer.View3D => SoringLayer.View,
            XUI_Layer.Tips => SoringLayer.Tips,
            XUI_Layer.Loding => SoringLayer.Loading,
            _ => SoringLayer.Default
        };
    }
}

public static class LayersMask
{
    public const int All = 0x0fffffff;
    public const int None = 0;
    public const int Def = 1 << Layers.Default;
    public const int TransparentFx = 1 << Layers.TransparentFx;
    public const int IgnoreRaycast = 1 << Layers.IgnoreRaycast;
    public const int UI = 1 << Layers.UI;
    public const int AllUi = UI;
}

public static class Tags
{

}

