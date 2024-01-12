using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedGamePlayData
{
    // 소지한 재화 OK
    public int money;

    // 맵에 배치된 오브젝트(소품) 위치값 저장을 위한 변수
    // 배열 Index번호에 맞춰서 ID값과 X,Y,Z값을 불러옴 OK
    public string[] recordItemTransform = new string[570];

    // 1~99 코인, 100~ 주방,200~ 거실, 300~ 아기방 OK
    public int[] coinAndRecordItem = new int[400];

    //활성화된 기능을 위한 변수 OK
    public bool skill_Catch;  //잡기형 스킬

    //업그레이드된 범위 및 총알 수량을 위한 변수
    public int[] upgrade = new int[2]; // 0 : Range, 1 : Amount

    //플레이어 위치값 저장을 위한 변수 OK
    public string playerTransform;

    // 첫 키 설명 UI
    public bool ui_keyExplain;

    // 첫 코인 획득 UI
    public bool ui_coinExplain;
}
