using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class DialogueTypingEffect : MonoBehaviour
{
    //AudioSource myAduio = default;

    //private float textSpeed = 10f; //텍스트 타이핑 속도 변경 멤버

    float oneLetterSpeed = 0.1f; //한 글자 사이의 텀
    float spaceLetterSpeed = 0.02f; //스페이스 공백 텀

    WaitForSecondsRealtime oneLetterWait; 
    WaitForSecondsRealtime spaceLetterWait;   

    WaitForSecondsRealtime oneSoundWait;  
    WaitForSecondsRealtime spaceSoundWait; 

    public GameObject fadeImgae = default; //대화가 끝난 후 UI적으로 E 버튼 페이드인 페이드 아웃 하기 위한 이미지
    public bool isTypingRunning {  get; private set; } //타이핑 효과가 진행되고 있는지 확인하기 위한 bool값

    private Coroutine typingCoroutine; //타이핑 코루틴 종료를 위한 코루틴 멤버
    private Coroutine textSoundCoroutine; //글자 수에 맞춰서 소리 내는 코루틴 종료를 위한 멤버

    StringBuilder textBuilder = new StringBuilder(); //완전한 텍스트가 저장될 스트링 빌더

    [Header("RandomSound")]
    int randomNumber = 0; //10개의 단음 중에서 랜덤하게 꺼내기 위한 변수 
    
    //사운드 관리를 위한 리스트{
    public AudioClip[] boldTone = default;
    public AudioClip[] middleTone = default;
    public AudioClip[] highTone = default;
    //}사운드 관리를 위한 리스트
    
    private AudioClip myAudioClip = default;

    private void Start()
    {
        oneLetterWait = new WaitForSecondsRealtime(oneLetterSpeed);
        spaceLetterWait = new WaitForSecondsRealtime(spaceLetterSpeed);

        oneSoundWait = new WaitForSecondsRealtime(oneLetterSpeed);
        spaceSoundWait = new WaitForSecondsRealtime(spaceLetterSpeed);
    }

    //타이핑 효과 시작을 위한 메서드 대화창에 출력 가능
    public void Run(string textToType, TMP_Text textLabel, AudioSource myAduio, int toneNumber)
    {
        typingCoroutine = StartCoroutine(WriteEffect(textToType, textLabel));
        textSoundCoroutine = StartCoroutine(TextSoundEffect(textToType, myAduio, toneNumber));
    }
    
    //타이핑 효과 종료를 위한 메서드
    public void Stop()
    {
        StopCoroutine(typingCoroutine);
        StopCoroutine(textSoundCoroutine);
        isTypingRunning = false;
    }

    //TODO 삭제예정 참고용
    //public Coroutine Run(string textToType, TMP_Text textLabel)
    //{
    //    return StartCoroutine(WriteEffect(textToType, textLabel));
    //}

    
    public IEnumerator WriteEffect(string textToType, TMP_Text textLabel)
    {
        //실질적으로 타이핑 효과를 내는 코루틴

        isTypingRunning = true; //타이핑 시작
        textLabel.text = string.Empty; //TMP에 텍스트 창을 한번 비워줍니다.
        textBuilder.Clear(); //텍스트들이 합쳐질 공간을 비워주고
        //이거는 메서드로 빼둘까 싶습니다.
        #region 특정 문자열 체크를 위한 string << 꼭 string 으로 해야 하는지는 모르겠습니다.
        string checkComma = "*";
        string checkLineC = "/"; //특정 문자열 판별을 위한 스트링 변수  /  char 로 비교하는게 좋을지 고민중   string 배열 안에서 멤버들을 비교하다보니 string로 쓰는게 나은 것 같음
        #endregion

        string[] nanugi = textToType.Split(new char[] { ' ' }); //string textToType 을 나눈 뒤 담아두는 배열 nanugi //이름 별로면 ... 변경 예정
        
        //string sumText = string.Empty; //아래와 같은 방식으로 사용하면 삭제해도 무방한 라인
        //이걸 스트링 빌더로 바꿔야하는데


        //string completeSentence = PrintCompleteSentence(textToType); // 타이핑 반복문이 끝나면 완전한 문장을 tmp.text 에 출력하기 위해서 사용하는 메서드이며 특정 문자가 비교된 후의 완전한 문장을 리턴함.
        
        for (int i = 0; i < nanugi.Length; i++)  //split된 string[] 의 길이만큼 반복
        {
            //float duration = Time.unscaledDeltaTime;  //타이핑 진행도 체크를 위한 변수
            //int charIndex = 0;                        //substring 하기 위한 int
            
            if (nanugi[i] == checkLineC)              //특정 문자와 비교 후 같으면 조건문으로 들어가서 특정 행위 실행 후 continue
            {
                //sumText += "\n";
                textBuilder.Append("\n");
                continue;
            }
            else if (nanugi[i] == checkComma)
            {
                //sumText += ",";
                textBuilder.Append(",");
                continue;
            }
            textBuilder.Append(" ");
            for (int j = 0; j < nanugi[i].Length; j++)
            {
                textBuilder.Append(nanugi[i][j].ToString());
                textLabel.text = textBuilder.ToString();
                //yield return new WaitForSecondsRealtime(0.085f); //한 글자 출력되는 텀
                yield return oneLetterWait;
            }
            textLabel.text = textBuilder.ToString();
            yield return spaceLetterWait;
            //yield return new WaitForSecondsRealtime(0.02f); // 스페이스 띄는 시간
            #region 기존에 썼던 while
            //while (charIndex < nanugi[i].Length)    //split된 멤버의 글자 수만큼 돌아가며 substring 으로 진행된 시간에 따라 출력되는 길이가 달라짐
            //{
            //    duration += Time.unscaledDeltaTime * textSpeed;
            //    charIndex = Mathf.FloorToInt(duration);
            //    charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);

            //    textLabel.text = sumText + nanugi[i].Substring(0, charIndex).Trim();  //Trim을 해줘서 공백을 제거한 값을 출력

            //    yield return null;
            //}
            #endregion

            //sumText = string.Empty;  //아마 간소화 가능 간소화 한다면 위에 empty는 살려둬야함
            //sumText += textLabel.text + " ";    //공백이 제거된 값 뒤에 스페이스 추가해서 sumText에 저장
            
        }

        //StartCoroutine(TextSoundEffect(textToType,myAduio,textSound)); //일단 남겨둠
        isTypingRunning = false;                //타이핑 코루틴 종료
        fadeImgae.SetActive(true);              //켜지면 onenable 되서 fadein  fadeout 반복
        textLabel.text = PrintCompleteSentence(textToType); //바로 탈출하게 되면 완전한 문장 출력을 위한 구문
    }

    //사운드 출력을 위한 코루틴 
    public IEnumerator TextSoundEffect(string textToType,AudioSource myAudio, int toneNumber)
    {
        string[] temporaryText = textToType.Split(new char[] { ' ' }); //띄어쓰기 기준으로 나눔
        for(int i = 0; i < temporaryText.Length; i++)                   //나누어진 수 만큼 반복문 돌아감
        {
            if (temporaryText[i] == "/" || temporaryText[i] == "*")        //특정 문자 만나면 사운드 재생을 안하고 넘김.  마침표 및 특정 문자도 추가할지 고민중
            {
                continue;
            }
            //string[] row = testText[i].Split(new char[] { ' ' });
            for (int j = 0; j < temporaryText[i].Length; j++)           //나누어진 멤버의 글자 길이만큼 반복문 돌림
            {
                //if (temporaryText[i] == "/")
                //{
                //    continue;
                //}
                //myAudioClip = ChooseRandomSound(toneNumber);
                //myAduio.PlayOneShot(myAudioClip);
                PlayNpcSound(toneNumber);
                yield return oneSoundWait;
                //yield return new WaitForSecondsRealtime(0.085f); // WaitForSeconds(0.085f);  //scale에 영향 받지 않게끔 realtime으로 변경  이 수는 매직넘버지라 변경 필요
            }
            yield return spaceSoundWait;
            //yield return new WaitForSecondsRealtime(0.02f);//WaitForSeconds(0.02f); //위와 동일


        }
        //for (int i = 0;  i < textToType.Length; i++)
        //{
        //    myAudioClip = ChooseRandomSound(toneNumber);
        //    myAduio.PlayOneShot(myAudioClip);
        //    yield return new WaitForSeconds(0.085f);
        //}
    }

    //출력할 목소리 결정하는 메서드
    private void PlayNpcSound(int toneNumber)
    {
        if(toneNumber == 0)
        {
            randomNumber = Random.Range(0, boldTone.Length);
            
        }
        else if( toneNumber == 1) 
        {
            randomNumber = 10 + Random.Range(0, middleTone.Length);
        }
        else
        {
            randomNumber = 20 + Random.Range(0, highTone.Length);
        }
        SoundManager.instance.PlaySound(GroupList.Npc, randomNumber);
    }

    //완전한 문장을 특정 문자 비교후 반환하는 메서드
    public string PrintCompleteSentence(string text)
    {
        textBuilder.Clear();
        string[] nanugi = text.Split(new char[] { ' ' });
        string completeSentence;
        string chechLineC = "/";
        string checkComma = "*";

        for (int i = 0; i < nanugi.Length; i++)
        {
            if (nanugi[i] == chechLineC)
            {
                //completeSentence += "\n";
                textBuilder.Append("\n");
                //continue;
            }
            else if (nanugi[i] == checkComma)
            {
                //sumText += ",";
                //completeSentence += ", ";
                //textBuilder.Append(", ");
                textBuilder.Append(',');
                //continue;
            }
            else
            {
                textBuilder.Append(" ");
                textBuilder.Append(nanugi[i]);
            }
            //completeSentence += nanugi[i] + " ";
        }
        completeSentence = textBuilder.ToString(); 
        return completeSentence;
    }

    //TODO 삭제예정
    //private void Test()
    //{
    //    int id = (int)NpcSoundList.lowTone01;
    //    SoundManager.instance.PlaySound(GroupList.Npc, id);
    //}
    //private AudioClip ChooseRandomSound(int toneNumber)
    //{
    //    AudioClip myClip;
    //    if (toneNumber == 0)
    //    {
    //        randomNumber = UnityEngine.Random.Range(0, boldTone.Length);
    //        myClip = boldTone[randomNumber];
    //    }
    //    else if (toneNumber == 1)
    //    {
    //        randomNumber = UnityEngine.Random.Range(0, middleTone.Length);
    //        myClip = middleTone[randomNumber];
    //    }
    //    else
    //    {
    //        randomNumber = UnityEngine.Random.Range(0, highTone.Length);
    //        myClip = highTone[randomNumber];
    //    }

    //    return myClip;
    //}

    //TODO 완전히 끝나면 삭제예정
    //TEST TEXT WRITTER 
    //public IEnumerator WriteTest(string textToType, TMP_Text textLabel)
    //{
    //    isTypingRunning = true;
    //    textLabel.text = string.Empty;

    //    float duration = Time.deltaTime;
    //    int charIndex = 0;
    //    //string boldEnterCheck = "<b>";
    //    //string boldExitCheck = "</b>";
    //    //string colorEnterCheck = "<color=#80A0FF>";
    //    //string colorExitCheck = "</color>";
    //    string chechLineC = "/";
    //    string checkComma = "*";
        
    //    string[] nanugi = textToType.Split(new char[] { ' ' });
        
    //    StringBuilder testStringBuilder = new StringBuilder();

    //    testStringBuilder.Clear(); //비워주고

    //    //string sumText = string.Empty;
    //    for (int i = 0; i < nanugi.Length; i++)
    //    {
    //        Debug.Log("{0} < 1",textLabel);

    //        //duration = Time.deltaTime;
    //        charIndex = 0;
    //        //Debug.LogFormat("{0} <<< 이게 sumText", testStringBuilder.ToString());
    //        //Debug.Log(nanugi[i]);
    //        if (nanugi[i] == chechLineC)
    //        {
    //            //Debug.Log("같으면 들어오는건데");
    //            //sumText += "\n";
    //            testStringBuilder.Append("\n");
    //            continue;
    //        }
    //        else if (nanugi[i] == checkComma)
    //        {
    //            //sumText += ",";  
    //            testStringBuilder.Append(",");
    //            continue;
    //        }
    //        #region 굵기 및 색깔 조건 쓸 경우 주석처리
    //        //else if (nanugi[i] == boldEnterCheck)
    //        //{
    //        //    sumText += boldEnterCheck;
    //        //    continue;
    //        //}
    //        //else if (nanugi[i] == boldExitCheck)
    //        //{
    //        //    sumText += boldExitCheck;
    //        //    continue;
    //        //}
    //        //else if(nanugi[i] == colorEnterCheck)
    //        //{
    //        //    sumText += colorEnterCheck;
    //        //    continue;
    //        //}
    //        //else if (nanugi[i] == colorExitCheck)
    //        //{
    //        //    sumText += colorExitCheck;
    //        //    continue;
    //        //}
    //        #endregion
    //        Debug.Log("{0} < 2",textLabel);

    //        for (int j = 0;  j < nanugi[i].Length; j++)
    //        {   
    //            Debug.LogFormat("텍스트 박스 : {0}", textToType);
    //            Debug.LogFormat("스트링 빌더 : {0}", testStringBuilder);
    //            Debug.LogFormat("들어갈 내용 : {0}", nanugi[i][j].ToString());
    //            testStringBuilder.Append(nanugi[i][j].ToString());
    //            textLabel.text = testStringBuilder.ToString();
    //            yield return new WaitForSecondsRealtime(0.085f);

    //            //charIndex++;
    //            //yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.L));

    //            //// 대기 후 한 번 더 키 입력을 기다림
    //            //yield return new WaitUntil(() => !Input.GetKey(KeyCode.L));
    //            Debug.Log("L 누름");
    //            //yield return new WaitForSecondsRealtime(0.001f);
    //            //testStringBuilder.Clear();
    //        }
    //        yield return new WaitForSecondsRealtime(0.02f);

    //        //while (charIndex < nanugi[i].Length)
    //        //{
    //        //    duration += Time.deltaTime * textSpeed;
    //        //    charIndex = Mathf.FloorToInt(duration);
    //        //    charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);


    //        //    //textToType.Substring(0, charIndex);



    //        //    //textLabel.text = sumText + nanugi[i].Substring(0, charIndex);
    //        //    testStringBuilder.Append(textToType[charIndex]);
    //        //    textLabel.text = testStringBuilder.ToString();
    //        //    yield return null;
    //        //    //Debug.Log(textLabel.text);
    //        //    //yield return null;
    //        //    //Debug.LogFormat("{0} <== This is chaiIndex ", charIndex);
    //        //}
    //        //sumText = string.Empty;
    //        //testStringBuilder.Clear();

    //        //sumText += textLabel.text + " ";
    //        testStringBuilder.Append(" ");
    //        Debug.Log("{0} < 4", textLabel);

    //    }

    //    //StartCoroutine(TextSoundEffect(textToType,myAduio,textSound));


    //    isTypingRunning = false;
    //    //Debug.Log("지금 writeEffect 언제 되는거지?");
    //    fadeImgae.SetActive(true);
    //    //textLabel.text = textToType;
    //}
}
