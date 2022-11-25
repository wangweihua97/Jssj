using System;
using Game.ECS;
using UnityEngine;
using UnityEngine.UI;

public class GMView : MonoBehaviour
{
    public Button SettingButton;
    public SettingView SettingView;
    public Button BeginButton;

    private void Awake()
    {
        SettingButton.onClick.AddListener(ShowOrHideSettingView);
        BeginButton.onClick.AddListener(CreatMonsters);
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

    void CreatMonsters()
    {
        GameWorld.Instance.CreatMonster();
    }
}