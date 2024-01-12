using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrossLineSetting : MonoBehaviour
{
    [SerializeField] GameObject crossLine; //경로 Player >> CameraSystem >> GroundCamera >> Canvas >> adjustedReticle

    // 선택된 이미지 적용을 위한 변수
    [SerializeField] private Sprite selected;
    [SerializeField] private Sprite unSelected;
    [SerializeField] private Image smallImage;
    [SerializeField] private Image middleImage;
    [SerializeField] private Image largeImage;
    [SerializeField] private TMP_Text smallText;
    [SerializeField] private TMP_Text middleText;
    [SerializeField] private TMP_Text largeText;

    private ButtonSize selectedButton;

    private void Start()
    {        
        if (PlayerPrefs.HasKey("CrossLineSetting"))
        {
            LoadData();
        }
        else
        {
            Init();
        }
    }

    private void OnDisable()
    {        
        SaveData();
    }

    // 버튼 누를시 호출되는 함수
    private void ChangeCrossLineSize(ButtonSize inputButton)
    {
        smallImage.sprite = unSelected;
        middleImage.sprite = unSelected;
        largeImage.sprite = unSelected;
        smallText.color = new(0f, 0f, 0f);
        middleText.color = new(0f, 0f, 0f);
        largeText.color = new(0f, 0f, 0f);

        switch (inputButton)
        {
            case ButtonSize.Small:
                crossLine.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                smallImage.sprite = selected;
                smallText.color = new(1f, 1f, 1f);
                break;

            case ButtonSize.Middle:
                crossLine.transform.localScale = new Vector3(1f, 1f, 1f);
                middleImage.sprite = selected;
                middleText.color = new(1f, 1f, 1f);
                break;

            case ButtonSize.Large:
                crossLine.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                largeImage.sprite = selected;
                largeText.color = new(1f, 1f, 1f);
                break;
        }
    }

    // (소) 버튼 클릭시
    public void PushSmall()
    {
        selectedButton = ButtonSize.Small;
        ChangeCrossLineSize(selectedButton);
    }

    // (중) 버튼 클릭시
    public void PushMiddle()
    {
        selectedButton = ButtonSize.Middle;
        ChangeCrossLineSize(selectedButton);
    }

    // (대) 버튼 클릭시
    public void PushLarge()
    {
        selectedButton = ButtonSize.Large;
        ChangeCrossLineSize(selectedButton);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("CrossLineSetting", (int)selectedButton);
    }

    private void LoadData()
    {
        selectedButton = (ButtonSize)PlayerPrefs.GetInt("CrossLineSetting");
        ChangeCrossLineSize(selectedButton);
    }

    public void Init()
    {
        PushMiddle();
    }
}
