using System.Collections;
using System.Collections.Generic;
using Game.Camera;
using Game.GlobalSetting;
using UnityEngine;
using UnityEngine.UI;

public class SettingView : MonoBehaviour
{
    public CustomSlider RotationX;
    public CustomSlider RotationY;
    public Toggle IsOpenShadow;
    public Toggle IsOpenRimLight;
    public Toggle IsOpenColorGlut;

    private Vector3 localEulerAngles;
    void Start()
    {
        RotationX.SliderValueChanged += RotationXChanged;
        RotationY.SliderValueChanged += RotationYChanged;

        localEulerAngles = MainCamera.Instance.transform.localEulerAngles;
        RotationX.SetValue(localEulerAngles.x);
        RotationY.SetValue(localEulerAngles.y);
        IsOpenShadow.onValueChanged.AddListener(IsOpenShadowValueChanged);
        IsOpenRimLight.onValueChanged.AddListener(IsOpenRimLightValueChanged);
        IsOpenColorGlut.onValueChanged.AddListener(IsOpenColorGlutValueChanged);

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

    void IsOpenShadowValueChanged(bool isOpen)
    {
        Setting.IsOpenShadow = isOpen;
    }
    
    void IsOpenRimLightValueChanged(bool isOpen)
    {
        Setting.IsOpenRimLight = isOpen;
    }
    
    void IsOpenColorGlutValueChanged(bool isOpen)
    {
        Setting.IsOpenColorGlut = isOpen;
    }
    
}
