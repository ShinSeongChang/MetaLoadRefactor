using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TextSizeSetting : MonoBehaviour
{
    private ButtonSize selectedButton;

    // 선택된 이미지 적용을 위한 변수
    [SerializeField] private Sprite selected;
    [SerializeField] private Sprite unSelected;
    [SerializeField] private Image smallImage;
    [SerializeField] private Image middleImage;
    [SerializeField] private Image largeImage;
    [SerializeField] private TMP_Text smallText;
    [SerializeField] private TMP_Text middleText;
    [SerializeField] private TMP_Text largeText;

    TMP_Text[] originTexts; // 크기를 기억할 원본
    List<float> fontSizes;        

    private void Awake()
    {
        // 비활성화된 오브젝트도 전부 가져와서 배열로 담음
        originTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();    
        
        fontSizes = new List<float>();

        // 각각 크기가 다른 텍스트의 원래 크기값을 리스트에 저장해줌
        for (int i = 0; i < originTexts.Length; i++)
        {            
            fontSizes.Add(originTexts[i].fontSize);            
        }
        
        if (PlayerPrefs.HasKey("TextSizeSetting"))
        {
            LoadData();
        }
        else
        {
            Init(); //초기화
        }
    }

    private void OnDisable()
    {        
        SaveData();
    }

    // 버튼 누를시 호출되는 함수
    private void ChangeTextSize(ButtonSize inputButton)
    {
        float tempSize = default;

        smallImage.sprite = unSelected;
        middleImage.sprite = unSelected;
        largeImage.sprite = unSelected;
        smallText.color = new(0f, 0f, 0f);
        middleText.color = new(0f, 0f, 0f);
        largeText.color = new(0f, 0f, 0f);

        switch (inputButton)
        {
            case ButtonSize.Small:
                smallImage.sprite = selected;
                smallText.color = new(1f, 1f, 1f);
                break;

            case ButtonSize.Middle:
                middleImage.sprite = selected;
                middleText.color = new(1f, 1f, 1f);
                break;

            case ButtonSize.Large:
                largeImage.sprite = selected;
                largeText.color = new(1f, 1f, 1f);
                break;
        }

        for (int i = 0; i < originTexts.Length; i++)
        {
            switch (inputButton)
            {
                case ButtonSize.Small:
                    tempSize = fontSizes[i] * 0.8f;
                    break;

                case ButtonSize.Middle:
                    tempSize = fontSizes[i];
                    break;

                case ButtonSize.Large:
                    tempSize = fontSizes[i]* 1.15f;
                    break;
            }
            originTexts[i].fontSize = tempSize;
        }
    }

    // (소) 버튼 클릭시
    public void PushSmall()
    {
        selectedButton = ButtonSize.Small;
        ChangeTextSize(selectedButton);
    }

    // (중) 버튼 클릭시
    public void PushMiddle()
    {
        selectedButton = ButtonSize.Middle;
        ChangeTextSize(selectedButton);
    }

    // (대) 버튼 클릭시
    public void PushLarge()
    {
        selectedButton = ButtonSize.Large;
        ChangeTextSize(selectedButton);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("TextSizeSetting", (int)selectedButton);
    }

    private void LoadData()
    {
        selectedButton = (ButtonSize)PlayerPrefs.GetInt("TextSizeSetting");
        ChangeTextSize(selectedButton);
    }

    public void Init()
    {
        PushMiddle();
    }
}
