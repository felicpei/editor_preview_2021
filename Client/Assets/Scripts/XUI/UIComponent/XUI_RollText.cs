using UnityEngine;

public class XUI_RollText : MonoBehaviour
{
    public RectTransform TextParent;
    public XUI_Text Text;
    public float Speed = 3f;
    public float Delay = 2f;

    private Vector2 _parentSize;
    private State _curState = State.MoveLeft;
    private float _delayTime;

    enum State
    {
        MoveRight,
        MoveLeft,
    }

    public void Start()
    {
        if (TextParent)
        {
            _parentSize = TextParent.sizeDelta;
        }
        Stop2Seconds();
    }

    public void Update()
    {
        if (!Text || !TextParent) return;

        var t = Text.rectTransform;
        if (!t) return;
        var textSize = t.sizeDelta;
        var pos = t.anchoredPosition3D;
        if (textSize.x <= 0 || textSize.x <= _parentSize.x)
        {
            pos.x = 0;
            t.anchoredPosition3D = pos;
            return;
        }

        var std = new Vector2(0, 0.5f);
        t.anchorMax = std;
        t.anchorMin = std;
        t.pivot = std;
        
        if (_delayTime > Time.time) return;
        switch (_curState)
        {
            case State.MoveRight:
                pos.x += Speed * Time.deltaTime;
                t.anchoredPosition3D = pos;
                if (pos.x >= 0)
                {
                    _curState = State.MoveLeft;
                    Stop2Seconds();
                }
                break;
            case State.MoveLeft:
                var endValue = textSize.x - _parentSize.x;
                pos.x -= Speed * Time.deltaTime;
                t.anchoredPosition3D = pos;
                if (pos.x <= -endValue)
                {
                    _curState = State.MoveRight;
                    Stop2Seconds();
                }
                break;
        }
    }

    private void Stop2Seconds()
    {
        _delayTime = Time.time + 2f;
    }
}