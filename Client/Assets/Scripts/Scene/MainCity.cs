using System.Collections;
using UI;

public class MainCity : SceneBase
{
    public override IEnumerator Init()
    {
        yield return base.Init();
        UIDebugEntrance.Show();
    }
}