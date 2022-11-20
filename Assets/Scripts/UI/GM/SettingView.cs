using System.Collections;
using System.Collections.Generic;
using Game.Camera;
using UnityEngine;

public class SettingView : MonoBehaviour
{
    public CustomSlider RotationX;
    public CustomSlider RotationY;

    private Vector3 localEulerAngles;
    void Start()
    {
        RotationX.SliderValueChanged += RotationXChanged;
        RotationY.SliderValueChanged += RotationYChanged;

        localEulerAngles = MainCamera.Instance.transform.localEulerAngles;
        RotationX.SetValue(localEulerAngles.x);
        RotationY.SetValue(localEulerAngles.y);
        
    }

    void RotationXChanged(float value)
    {
        Vector3 eulerAngles = MainCamera.Instance.transform.eulerAngles;
        Debug.Log(value);
        MainCamera.Instance.transform.eulerAngles = new Vector3(value ,eulerAngles.y ,eulerAngles.z);
    }
    
    void RotationYChanged(float value)
    {
        Vector3 eulerAngles = MainCamera.Instance.transform.eulerAngles;
        MainCamera.Instance.transform.eulerAngles = new Vector3(eulerAngles.x ,value,eulerAngles.z);
    }
}
