using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string dialogueID; //대사 ID

    public string speakerName; //말하는 사람 이름

    public string[] contextes; //대사 내용

    public string nextTextNum; //다음 대사 내용 ID
    ////12.12 HJ 추가
    //public string currentStatus; //현재 NPC 상태
    //TODO 만약 테스트가 잘 된다면 이곳으로 통합할수도 있음
}
