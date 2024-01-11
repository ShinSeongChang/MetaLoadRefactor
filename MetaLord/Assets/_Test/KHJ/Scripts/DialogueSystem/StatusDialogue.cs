using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////12.12 HJ 추가
[System.Serializable]
public class StatusDialogue
{
    public string dialogueID; //대사 ID

    public string speakerName; //말하는 사람 이름

    public string[] contextes; //대사 내용

    public string nextTextNum; //다음 대사 내용 ID
    
    public string currentStatus; //현재 NPC 상태

    //12.19 추가 : 음성 관련
    public string voice; //NPC 목소리 
}


