using System;
using Game.ECS;
using UnityEngine;
using UnityEngine.UI;

public class GMView : MonoBehaviour
{
    public Button SettingButton;
    public SettingView SettingView;
    public Button BeginButton;
    
    public Button StartBuildingButton;
    public Button EndBuildingButton;

    public static bool IsBuilding = false;
    private void Awake()
    {
        SettingButton.onClick.AddListener(ShowOrHideSettingView);
        BeginButton.onClick.AddListener(CreatMonsters);
        
        StartBuildingButton.onClick.AddListener(StartBuilding);
        EndBuildingButton.onClick.AddListener(EndBuilding);
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

    void StartBuilding()
    {
        IsBuilding = true;
        StartBuildingButton.gameObject.SetActive(false);
        EndBuildingButton.gameObject.SetActive(true);
    }
    
    void EndBuilding()
    {
        IsBuilding = false;
        StartBuildingButton.gameObject.SetActive(true);
        EndBuildingButton.gameObject.SetActive(false);
    }

    void CreatMonsters()
    {
        GameWorld.Instance.CreatMonster();
    }
}