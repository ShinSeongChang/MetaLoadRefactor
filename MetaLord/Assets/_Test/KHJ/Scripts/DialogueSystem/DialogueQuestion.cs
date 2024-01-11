using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueQuestion
{
    public string questionID; //대사 ID

    public string questionContextes; //질문 내용

    public string nextTextNum; //이동 대사 내용 ID
}