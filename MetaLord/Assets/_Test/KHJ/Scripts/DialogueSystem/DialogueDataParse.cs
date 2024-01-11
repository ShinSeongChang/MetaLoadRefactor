using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class DialogueDataParse : MonoBehaviour
{
    
    public DialogueQuestion[] ParseQuestionList(TextAsset questionCsvData)
    {
        List<DialogueQuestion> questionList = new List<DialogueQuestion>();
        string[] data = questionCsvData.text.Split('\n');
        
        for (int i = 1; i < data.Length - 1;  i++) 
        {
            string[] row = data[i].Split(new char[] { ',' });
            DialogueQuestion question = new DialogueQuestion();
            for (int j = 0; j < row.Length - 1;j++)
            {
                question.questionID = row[0];
                question.questionContextes = row[1];
                question.nextTextNum = row[2].Trim();
            }
            questionList.Add(question);
        }
        return questionList.ToArray();
    }
    //public Dialogue[] ParseDialogue(TextAsset csvData) //TODO 삭제 예정
    //{
    //    List<Dialogue> dialogueList = new List<Dialogue>();
    //    //ParseDialogue(csvData);
    //    //모든 값은 null값이 아니고 "" 값으로 들어감
    //    string[] data = csvData.text.Split(new char[] { '\n' });

    //    for (int i = 1; i < data.Length - 1;)
    //    {
    //        string[] row = data[i].Split(new char[] { ',' });
    //        Dialogue _dialogue = new Dialogue();
    //        //다이얼로그 클래스의 row[0] = ID, row[1] = 이름, row[2] = 대사, row[3] = 이동 대사 ID 번호
    //        //23.12.12 추가사항 row[4] 에 특수한 상태에 따라 다른 내용을 이곳에 합칠 수 있음
    //        _dialogue.dialogueID = row[0];
    //        _dialogue.speakerName = row[1];
    //        List<string> contextList = new List<string>();
    //        do
    //        {
    //            contextList.Add(row[2]);
    //            if (row[3] != "")
    //            {
    //                //Debug.Log("여기 들어오나요 ?");
    //                _dialogue.nextTextNum = row[3].Trim();
    //                //Debug.LogFormat("{0} <=== 이게 row3", _dialogue.nextTextNum);
    //            }
    //            if (++i < data.Length - 1)
    //            {
    //                row = data[i].Split(new char[] { ',' }); //data i번째의 string 들을 , 로 구분
    //            }
    //            else
    //            {
    //                break;
    //            }
    //        }
    //        while (row[0].ToString() == ""); //|| row[3].ToString() != "");
    //        _dialogue.contextes = contextList.ToArray();
    //        dialogueList.Add(_dialogue);
    //    }
    //    //Debug.Log(dialogueList.Count);
    //    //각 헤더 설정해서 담기
    //    //for (int i = 0; i < dialogueList.Count; i++)
    //    //{
    //    //    Debug.LogFormat("{0} <=== 이게 row0 ID", dialogueList[i].dialogueID);
    //    //    Debug.LogFormat("{0} <=== 이게 row1 NAME", dialogueList[i].speakerName);
    //    //    Debug.LogFormat("{0} <=== 이게 row2 CONTEXT", dialogueList[i].contextes);
    //    //    Debug.LogFormat("{0} <=== 이게 row2 CONTEXT Length", dialogueList[i].contextes.Length);
    //    //    for(int j = 0; j < dialogueList[i].contextes.Length; j++)
    //    //    {
    //    //        //Debug.LogFormat("{0} contextes 대화 내용들 ", dialogueList[i].contextes[j]);
    //    //    }
    //    //    Debug.LogFormat("{0} <=== 이게 row3 NEXTNUM", dialogueList[i].nextTextNum);
    //    //}
    //    return dialogueList.ToArray();
    //}
    public StatusDialogue[] ParseStatusDialogue(TextAsset csvData)
    {
        List<StatusDialogue> statusDialogueList = new List<StatusDialogue>();
        //모든 값은 null값이 아니고 "" 값으로 들어감 << 이게 Debug 찍어보면 아얘 공백 값으로 보이긴 함 
        //++Trim을 해줘야지 나중에 if 문 "" 으로 체크하거나 string.Empty 할 때 체크가 됨.
        //안그러면 이상한 값이 들어있다고 체크를 못함

        string[] data = csvData.text.Split(new char[] { '\n' });

        for (int i = 1; i < data.Length - 1;)
        {
            string[] row = data[i].Split(new char[] { ',' });
            StatusDialogue _statusDialogue = new StatusDialogue();
            //다이얼로그 클래스의 row[0] = ID, row[1] = 이름, row[2] = 대사, row[3] = 이동 대사 ID 번호
            //23.12.12 추가사항 row[4] 에 특수한 상태에 따라 다른 내용 추가 예정 
            _statusDialogue.dialogueID = row[0].Trim();
            _statusDialogue.speakerName = row[1].Trim();
            _statusDialogue.currentStatus = row[4].Trim();
            _statusDialogue.voice = row[5].Trim();
            List<string> contextList = new List<string>();
            do
            {
                contextList.Add(row[2]);
                if (row[3] != "")
                {
                    _statusDialogue.nextTextNum = row[3].Trim(); //if(string = "" || string.Empty) 검출할거면 무조건 trim 해주기   
                }
                if (++i < data.Length - 1)
                {
                    row = data[i].Split(new char[] { ',' }); //data i번째의 string 들을 , 로 구분
                }
                else
                {
                    break;
                }
            }
            while (row[0].ToString() == ""); //|| row[3].ToString() != "");
            _statusDialogue.contextes = contextList.ToArray();
            statusDialogueList.Add(_statusDialogue);
        }
        return statusDialogueList.ToArray();
    }
}
