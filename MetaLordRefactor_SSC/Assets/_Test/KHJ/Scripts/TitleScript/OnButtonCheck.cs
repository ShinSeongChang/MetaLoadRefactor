using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnButtonCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    TMP_Text myText;

    string currentText;
    float currentSizeNum;

    float sizeUpNum = 60f;

    private void Start()
    {
        myText = transform.GetChild(0).transform.GetComponent<TMP_Text>();
        currentText = myText.text;
        currentSizeNum = myText.fontSize;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 포인터가 버튼에 들어왔을 때 호출될 코드
        SizeUpText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스 포인터가 버튼을 벗어났을 때 호출될 코드
        RollBackSizeText();
    }

    private void SizeUpText()
    {
        if (myText != null) 
        {
            myText.fontSize = sizeUpNum;
            myText.text = "<b>" + currentText + "</b>"; 
        }
    }

    private void RollBackSizeText()
    {
        if(myText != null) 
        {
            myText.fontSize = currentSizeNum;
            myText.text = currentText.Trim();
        }
    }

}
