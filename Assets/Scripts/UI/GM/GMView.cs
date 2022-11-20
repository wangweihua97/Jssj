using System;
using UnityEngine;
using UnityEngine.UI;

public class GMView : MonoBehaviour
{
    public Button SettingButton;
    public SettingView SettingView;

    private void Awake()
    {
        SettingButton.onClick.AddListener(ShowOrHideSettingView);
    }

    void ShowOrHideSettingView()
    {
        if (SettingView.gameObject.activeSelf)
        {
            SettingView.gameObject.SetActive(false);
        }
        else
        {
            SettingView.gameObject.SetActive(true);
        }
        
    }
}