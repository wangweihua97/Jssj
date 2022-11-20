using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowFPS : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text Fps;
    
    private float _Interval = 0.5f;
    private int _FrameCount = 0;
    private float _TimeCount = 0;
    private float _FrameRate = 0;
    void Start()
    {
        
    }

    
 
    void Update()
    {
        _FrameCount++;
        _TimeCount += Time.unscaledDeltaTime;
        if (_TimeCount >= _Interval)
        {
            _FrameRate = _FrameCount / _TimeCount;
            _FrameCount = 0;
            _TimeCount -= _Interval;
        }
        Fps.SetText("FPS: " +_FrameRate);
    }
}
