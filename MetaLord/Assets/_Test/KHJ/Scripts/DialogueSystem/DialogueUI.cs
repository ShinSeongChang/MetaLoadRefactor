using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    #region 다이얼로그 매직넘버 관련 ID
    // TODO  다이얼로그 매직넘버 enum 이름으로 할당해서 npc 에서 이름으로 시작하게끔 수정 예정
    //ShowDialogue
    // 01 ~ 11 : 튜토리얼 토끼
    // 12 ~ 14 : 양
    // 15 ~ 17 : 꿀벌
    // 18 ~ 20 : 공룡
    // 21 ~ 23 : 오빌윌버
    // 24 ~ 26 : 뿔고래(norwhal?)
    // 27 ~ 28 : 눈사람(snow Girl) 
    // 30 ~ 32 : 모험가(Adventure)
    // 33 ~ 35 : 로봇
    // 36 ~ 38 : 아빠곰 * 아기곰 대화로 넘어감
    // 39 ~ 41 : 오리
    // 42 ~ 44 : 아기곰
    #endregion

    public PlayerInteractNpc playerInteract; //플레이어 상호작용 끝나면 사용한 bool값 되돌리기 위해 존재

    public GameObject dialogueUI; // 대화창 canvas 
    public GameObject questionUI; // 질문 선택지 canvas

    [SerializeField] private GameObject dialogueBox;  //대화창 ui 가지고 있는 부모 오브젝트
    [SerializeField] private TMP_Text dialogueText;   //하위 tmp 오브젝트

    [SerializeField] InputReader reader;

    [Header("TextFontSize")] //텍스트 설정 관련 
    public float minFontSize = 10f;   
    public float maxFontSize = 50f;   

    [Header("Response")]
    public bool isResponse = false;
    private DialogueTypingEffect myTextEffect;

    [Header("TextSound")] //사운드 출력 위한 오디오
    //12.15 사운드 작업을 위한 추가
    private AudioSource myAduio = default;
    private AudioClip textSound = default;

    void Start()
    {
        myAduio = GetComponent<AudioSource>();
        myAduio.clip = textSound;
        myTextEffect = GetComponent<DialogueTypingEffect>();

        //대화 관련 ui 다 비활성화 시켜줍니다.
        CloseDialogueUI(); 
        CloseTutoQuestion();
    }

    //대화 출력중 플레이어가 키를 한 번 더 누르면 완전한 문장이 나오게끔 제어 하는 메서드
    private IEnumerator RunTypingEffect(string dialouge, int voice) 
    {
        //int toneNum = voice;
        myTextEffect.Run(dialouge, dialogueText, myAduio, voice); //매직넘버 톤리스트 결정하는거임
        while (myTextEffect.isTypingRunning)
        {
            yield return null;
            if(reader.InteractKey)
            {
                reader.CancelInteract();
                myTextEffect.Stop();
                myTextEffect.fadeImgae.SetActive(true);
            }
        }
    }

    #region 대화,질문 캔버스 On OFF 메서드
    public void OpenDialogueUI()
    {
        dialogueUI.SetActive(true);
    }
    public void CloseDialogueUI() 
    {
        dialogueUI.SetActive(false);
    }

    public void  OpenTutoQuestion()
    {
        questionUI.SetActive(true);
    }
    public void CloseTutoQuestion()
    {
        questionUI.SetActive(false);
    }
    #endregion

    // 대화가 종료된 뒤 특정 행위가 있는지 없는지 체크하기 위한 메서드
    // 메서드를 사용하여 변경하는 경우 대화가 끝난 뒤 멈춰있게됨
    public void ChangeResponeBoolValue(bool value)
    {
        isResponse = value;
    }

    //12.12 추가
    //상태에 맞게끔 대화 시작하는 메서드입니다.
    private IEnumerator StepThroughStateDialogue(int keyNum)
    {
        //keyNum 대사 csv 파일 ID 를 대입하면 해당하는 대화를 딕셔너리에서 가져옵니다.
        if (keyNum == 0)
        {
            yield break;
        }
        while (true)
        {
            //DialogueDBManager에서 저장한 값들을 필요에 맞게끔 넣어줍니다.
            //id 대화를 판별하는 기준이 되는 번호입니다.
            //nextId 다음 대화를 진행하기 위한 번호입니다.
            //ContextArray 대화가 들어있는 배열입니다. 한줄만 있을수도 있고 여러줄이 있을경우 그만큼 출력됩니다.
            // voice 톤을 결정하기 위한 값이 들어있습니다 0 굵은 톤, 1 중간 톤, 2 높은 톤
            string id = DialogueDBManager.instance.statusDialogueDic[keyNum].dialogueID; 
            string nextId = DialogueDBManager.instance.statusDialogueDic[keyNum].nextTextNum;
            string[] contexteArray = DialogueDBManager.instance.statusDialogueDic[keyNum].contextes;
            int voice = int.Parse(DialogueDBManager.instance.statusDialogueDic[keyNum].voice);

            //이 반복문에서는 contexArray 에 들어있는 string[] 값들을 분류해서 출력합니다.
            for (int i = 0; i < contexteArray.Length; i++)
            {
                reader.CancelInteract();
                //dialogue 에는 출력할 하나의 문장이 들어갑니다.
                string dialogue = DialogueDBManager.instance.statusDialogueDic[keyNum].contextes[i];
                
                //이름 TMP에 저장하기 위한 내용입니다.
                dialogueBox.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = 
                    DialogueDBManager.instance.statusDialogueDic[keyNum].speakerName.Trim();
                //yield return myTextEffect.Run(dialogue, dialogueText);
                //Debug.Log(dialogue);

                //여기서 대사 출력 타이핑 효과와 음성 출력이 이루어집니다.
                yield return RunTypingEffect(dialogue, voice);
                //만약 대화ID당 한사람만 말하는 것이 아니라면 이 안에다가 voice 를 넣어줘야할듯
                //현재는 위에 주석 달아놓은 것처럼 변경하기 시간이 걸려서 다음 ID로 이어지게끔 구현

                //완성된 문장을 출력하기 위한 내용인데 이걸 지워야할지 
                //myTextEffect 타이핑 효과에서 한번 완전한 문장을 만들어주는데 어디서 
                dialogueText.text = myTextEffect.PrintCompleteSentence(dialogue); //요거를 라인변경된 값으로 바꿔줘야함
                
                myTextEffect.fadeImgae.SetActive(true);

                yield return null;
                //Debug.Log("E키 누르기 전인데 실행될려나?");

                yield return new WaitUntil(() => reader.InteractKey);
                //Debug.Log("E키 누른뒤인데 작동 안하나?");
                myTextEffect.fadeImgae.SetActive(false);
            }

            if (isResponse)
            {
                //TODO 추가적인 넣고싶은 기능 넣으면 될 듯
                OpenTutoQuestion();
            }

            if (nextId != "" && nextId != null)
            {
                Debug.LogFormat("{0}<== 여기가 nextId에 값이 들어있을때인데 무슨 값이죠?", nextId);
                keyNum = int.Parse(nextId);
            }
            else if (nextId == "" || nextId == null)
            {
                // TODO 대화창 닫고 플레이어 이동제한 해제
                Debug.Log("여기온거면 끝난거임");
                yield return new WaitUntil(() => reader.InteractKey);
                reader.CancelInteract();
                myTextEffect.fadeImgae.SetActive(false);
                if (isResponse == false)
                {
                    CloseDialogueUI();
                    //플레이어 움직임 다시 제어
                    //testPlayer.isMove = true;
                    playerInteract.isInteract = true; //다시 대화할 수 있게끔
                    //23.12.26 
                    PlayerInteractNpc.isTalking = false; // 대화 끝난거확인 
                    Controller_Physics.SwitchCameraLock(false);
                }
                break;
            }

            Debug.Log("이건 마지막 eKey 위에 디버그");
        }
        yield break;
    }

    public void ShowStateTutorialDialogue(int keyNum)
    {
        OpenDialogueUI();
        isResponse = true;
        StartCoroutine(StepThroughStateDialogue(keyNum));
    }

    public void ShowStateDialogue(int keyNum)
    {
        OpenDialogueUI();
        StartCoroutine(StepThroughStateDialogue(keyNum));
    }

    public void CheckStateDialogue(int keyNum, npcState _state)
    {
        //status
        // 0 = 기본적인 상태
        // 1 = 접착제 묻은 상태
        // 2 = 물건 붙은 상태

        if (_state == npcState.objectAttached)
        {
            
            ShowStateDialogue(keyNum + 2);
        }
        else if (_state == npcState.glued)
        {
            ShowStateDialogue(keyNum + 1);
        }
        else
        {
            ShowStateDialogue(keyNum);
        }
    }
    public void CheckStateTutorialDialogue(npcState _state)
    {
        //status
        // 0 = 기본적인 상태
        // 1 = 접착제 묻은 상태
        // 2 = 물건 붙은 상태

        if (_state == npcState.objectAttached)
        {
            ShowStateDialogue(11);
        }
        else if (_state == npcState.glued)
        {
            ShowStateDialogue(10);
        }
        else
        {
            ShowStateTutorialDialogue(1); // 토끼 첫번째 대사 
        }
    }
}
