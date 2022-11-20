using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour
{

    public float2 MinMax;
    public Slider Slider;
    public TMP_Text Value;

    public Action<float> SliderValueChanged;
    
    
    void Awake()
    {
        Slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float v)
    {
        float vailValue = math.lerp(MinMax.x, MinMax.y, v);
        Value.SetText(vailValue.ToString().Split(".").First());
        SliderValueChanged?.Invoke(vailValue);
    }

    public void SetValue(float v)
    {
        v = (v - MinMax.x) / (MinMax.y - MinMax.x);
        Slider.value = v;
    }
    
}
