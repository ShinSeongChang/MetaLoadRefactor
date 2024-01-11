using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueDBManager : MonoBehaviour
{
    public static DialogueDBManager instance;

    //[SerializeField] private TextAsset dialogueCsv;
    [SerializeField] private TextAsset questionCsv;

    //12.12 HJ 추가
    [SerializeField] private TextAsset dialogueStatusCsv;

    //public Dictionary<int, Dialogue> dialogueDic = new Dictionary<int, Dialogue>();
    public Dictionary<int, DialogueQuestion> questionDic = new Dictionary<int, DialogueQuestion>();
    public List<string> dialogueQuestions = new List<string>();

    public static bool isFinish = false;
    //12.12 HJ 추가
    public Dictionary<int, StatusDialogue> statusDialogueDic = new Dictionary<int, StatusDialogue>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DialogueDataParse dialogerParser = GetComponent<DialogueDataParse>();
            //Dialogue[] dialogues = dialogerParser.ParseDialogue(dialogueCsv); //TODO 삭제예정
            DialogueQuestion[] questionArray = dialogerParser.ParseQuestionList(questionCsv);
            //12.12 HJ추가  
            StatusDialogue[] statusDialogues = dialogerParser.ParseStatusDialogue(dialogueStatusCsv);

            // 질문 딕셔너리 저장
            for (int i = 0; i < questionArray.Length; i++)
            {
                questionDic.Add(i + 1, questionArray[i]);
            }
           
            //// 대화 대사 딕셔너리 형식으로 저장
            //for (int i = 0; i < dialogues.Length; i++)
            //{
            //    dialogueDic.Add(i + 1, dialogues[i]);
            //}

            // 상태 대화 대사 딕셔너리 형식으로 저장
            for (int i = 0; i< statusDialogues.Length; i++)
            {
                statusDialogueDic.Add(i+1, statusDialogues[i]);
            }
           
            for (int i = 1; i <= questionDic.Count; i++) 
            {
                dialogueQuestions.Add(questionDic[i].questionContextes.ToString()); // 질문 스트링 저장 완료
            }

            isFinish = true;
        }
    }
}
