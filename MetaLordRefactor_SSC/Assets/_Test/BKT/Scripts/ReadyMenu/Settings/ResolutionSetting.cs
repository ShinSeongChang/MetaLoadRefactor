using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// 해상도 세팅
/// 240102_배경택
/// </summary>
public class ResolutionSetting : MonoBehaviour
{
    [SerializeField] private DisplaySetting displaySetting; // 화면모드를 가져올 변수

    private List<string> str_resolution = new List<string>();
    private TMP_Text resText; // 해상도 표현 텍스트
    private int resIndex; // 해상도를 가리킬 인덱스값

    // 정해진 해상도
    [SerializeField]
    private (int width,int height)[] resolutions = { (1920, 1080), (1600, 900), (1360, 768), (1280, 720) };    

    private void Awake()
    {        
        resText = transform.GetChild(0).GetComponent<TMP_Text>(); // 글자 표시 텍스트
        
        // Tuple로 구현
        for (int i = 0; i < resolutions.Length; i++)
        {
            string temp = resolutions[i].width + "x" + resolutions[i].height;
            str_resolution.Add(temp);
        }

        if (PlayerPrefs.HasKey("ResolutionSettings"))
        {
            LoadData();
        }
        else Init();

    }

    private void OnDisable()
    {
        SaveData();
    }


    // 좌측버튼 누를경우
    public void PushLeft()
    {
        if (resIndex < resolutions.Length-1)
        {
            resIndex++;

            InputText(resIndex);
            Screen.SetResolution(resolutions[resIndex].width, resolutions[resIndex].height,Screen.fullScreen);
        }
    }

    // 우측버튼 누를경우
    public void PushRight()
    {
        if (resIndex > 0)
        {
            resIndex--;

            InputText(resIndex);
            Screen.SetResolution(resolutions[resIndex].width, resolutions[resIndex].height,Screen.fullScreen);
        } 

    }

    // 텍스트 입력
    public void InputText(int _resIndex)
    {
        resText.text = str_resolution[_resIndex];
    }

    // 초기화
    public void Init()
    {
        resIndex = 0;
        InputText(resIndex);
        Screen.SetResolution(resolutions[resIndex].width, resolutions[resIndex].height, Screen.fullScreen);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("ResolutionSettings", resIndex);
    }

    private void LoadData()
    {
        resIndex = PlayerPrefs.GetInt("ResolutionSettings");
        InputText(resIndex);
        Screen.SetResolution(resolutions[resIndex].width, resolutions[resIndex].height, Screen.fullScreen);
    }
}
