using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;


public class StartInfo : MonoBehaviour
{
    public static StartInfo instance;

    public bool isLoaded;

    // PlayerPref 정보를 가져올 변수
    public int info_ScreenMode;
    public int info_ResolutionIndex;
    public float info_Brightness;
    public ButtonSize info_TextSize;
    public ButtonSize info_CrossLine;
    public float info_MasterSlider;
    public float info_BGMSlider;
    public float info_SFXSlider;
    public int info_MasterToggle;
    public int info_BGMToggle;
    public int info_SFXToggle;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        SetPersonalSetting();
    }

    // 개인 설정 셋팅
    public void SetPersonalSetting()
    {
        if (PlayerPrefs.HasKey("DisplaySetting"))
        {
            LoadPersonalSetting(); // 개인 환경설정 불러오기
            //SetAudio();
        }
        else InitPersonalSetting();
    }

    // 개인 환경설정 불러오기
    private void LoadPersonalSetting()
    {
        info_ScreenMode = PlayerPrefs.GetInt("DisplaySetting");
        info_ResolutionIndex = PlayerPrefs.GetInt("ResolutionSettings");
        info_Brightness = PlayerPrefs.GetFloat("BrightnessSetting");
        info_CrossLine = (ButtonSize)PlayerPrefs.GetInt("CrossLineSetting");
        info_TextSize = (ButtonSize)PlayerPrefs.GetInt("TextSizeSetting");

        info_MasterSlider = PlayerPrefs.GetFloat("MasterSlider");
        info_BGMSlider = PlayerPrefs.GetFloat("BGMSlider");
        info_SFXSlider = PlayerPrefs.GetFloat("SFXSlider");

        info_MasterToggle = PlayerPrefs.GetInt("MasterToggle");
        info_BGMToggle = PlayerPrefs.GetInt("BGMToggle");
        info_SFXToggle = PlayerPrefs.GetInt("SFXToggle");
    }

    // 개인 설정 초기화
    private void InitPersonalSetting()
    {
        Screen.fullScreen = true;
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
        info_MasterToggle = 1;
        info_BGMToggle = 1;
        info_SFXToggle = 1;
        info_MasterSlider = 1f;
        info_BGMSlider = 1f;
        info_SFXSlider = 1f;
    }
}
