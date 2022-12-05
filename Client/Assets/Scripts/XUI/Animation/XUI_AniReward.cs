using UnityEngine;
using DG.Tweening;

public class XUI_AniReward : MonoBehaviour
{
    public XUI_Image Mask;

    public bool isScale = true;

    public bool isFade = true;

    public void StartAni()
    {
        Mask.SetActiveByCanvasGroup(true);
        if (isScale)
        {
            transform.localScale = Vector3.one * 1.03f;
            ScaleMax();
        }

        if (isFade)
        {
            Mask.Alpha = 0.2f;
            FadeMin();
        }
    }

    private Tweener _tween1;
    private Tweener _tween2;

    private void ScaleMax()
    {
        _tween1?.Kill();
        _tween2?.Kill();
        _tween1 = null;
        _tween2 = null;

        _tween2 = transform.transform.DOScale(1.03f, 0.45f);
        _tween2.onComplete = ScaleMin;
    }

    private void ScaleMin()
    {
        _tween1?.Kill();
        _tween2?.Kill();
        _tween1 = null;
        _tween2 = null;

        _tween1 = transform.transform.DOScale(0.97f, 0.45f);
        _tween1.onComplete = ScaleMax;
    }

    private Tweener _tween3;
    private Tweener _tween4;

    private void FadeMax()
    {
        _tween3?.Kill();
        _tween4?.Kill();
        _tween3 = null;
        _tween4 = null;

        _tween3 = Mask.DOFade(0.2f, 0.45f);
        _tween3.onComplete = FadeMin;
    }

    private void FadeMin()
    {
        _tween3?.Kill();
        _tween4?.Kill();
        _tween3 = null;
        _tween4 = null;

        _tween4 = Mask.DOFade(0f, 0.45f);
        _tween4.onComplete = FadeMax;
    }

    public void StopAni()
    {
        _tween1?.Kill();
        _tween2?.Kill();
        _tween1 = null;
        _tween2 = null;

        _tween3?.Kill();
        _tween4?.Kill();
        _tween3 = null;
        _tween4 = null;

        transform.localScale = Vector3.one;
        Mask.SetActiveByCanvasGroup(false);
    }

    private void OnDestroy()
    {
        StopAni();
    }
}