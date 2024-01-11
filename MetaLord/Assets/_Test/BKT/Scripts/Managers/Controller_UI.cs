using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 컨트롤
/// 231231 배경택
/// </summary>
public class Controller_UI : MonoBehaviour
{
    [Header("인풋 시스템 리더")] //인풋 시스템 리더
    [SerializeField]
    InputReader reader;

    //231219 배경택    
    [SerializeField] private GameObject canvases;
    private GameObject storeUI; // 상점 UI 오브젝트
    private GameObject recordUI; // 도감 UI 오브젝트
    private GameObject readyMenuUI; // 대기모드 UI 오브젝트
    private GameObject explainUI; // 도움말 UI 오브젝트    
    private GameObject firstCoinExPlainUI; // 첫 코인 도움말 UI 오브젝트
    private GameObject savingUI; // 첫 코인 도움말 UI 오브젝트
    private GameObject firstKeyExplainUI; // 첫 코인 도움말 UI 오브젝트
    private GameObject recordPopUpItemUI; // 아이템 팝업
    private GameObject settingsUI; // 설정창 UI 오브젝트
    private GameObject keyRebindUI; // 키 리바인딩 UI 오브젝트

    private bool canInput = true; // 입력 가능여부
    private const float INPUT_DELAYTIME = 0.3f; // 입력 후 대기 시간

    private void Awake()
    {
        // 231231 배경택
        storeUI = Utility.FindChildObj(canvases, "StoreCanvas");
        recordUI = Utility.FindChildObj(canvases, "RecordCanvas");        
        readyMenuUI = Utility.FindChildObj(canvases, "ReadyCanvas");
        explainUI = Utility.FindChildObj(canvases, "ExplainCanvas");
        firstCoinExPlainUI = Utility.FindChildObj(canvases, "CoinExplainCanvas");
        firstKeyExplainUI = Utility.FindChildObj(canvases, "KeyExplainCanvas");
        savingUI = Utility.FindChildObj(canvases, "SavingCanvas");
        recordPopUpItemUI = Utility.FindChildObj(canvases, "RecordPopUpCanvas");
        settingsUI = Utility.FindChildObj(canvases, "SettingCanvas");
        keyRebindUI = Utility.FindChildObj(canvases, "KeyRebindCanvas");
        recordPopUpItemUI = Utility.FindChildObj(canvases, "RecordPopUpCanvas");
    }

    private void Update()
    {        
        if (canInput && !PlayerInteractNpc.isTalking)
        {
            if (reader.StoreKey) // 상점 키 누를 경우 _231219 배경택
            {
                // 설정, 설명 관련 UI 켜져있을 경우 return;
                if (CheckSetAndExplainUITrue()) return;

                // 상점 UI가 이미 켜져있다면
                if (storeUI.activeSelf == true)
                {
                    Controller_Physics.SwitchCameraLock(false);
                    storeUI.SetActive(false); // 중복 버튼을 누를경우 꺼짐    
                }
                else // 상점 UI가 꺼져있다면
                {
                    Controller_Physics.SwitchCameraLock(true);
                    storeUI.SetActive(true);
                    recordUI.SetActive(false);
                    readyMenuUI.SetActive(false);
                }

                StartCoroutine(DelayInput());
            }

            if (reader.RecordKey) // 도감 키 누를 경우 _231219 배경택
            {
                // 설정, 설명 관련 UI 켜져있을 경우 return;
                if (CheckSetAndExplainUITrue()) return;

                // 도감 UI가 켜져있다면
                if (recordUI.activeSelf == true)
                {
                    Controller_Physics.SwitchCameraLock(false);
                    recordUI.SetActive(false); // 중복 버튼을 누를경우 꺼짐
                }
                else // 도감 UI가 꺼져있다면
                {
                    Controller_Physics.SwitchCameraLock(true);
                    recordUI.SetActive(true);
                    storeUI.SetActive(false);
                    readyMenuUI.SetActive(false);
                }

                StartCoroutine(DelayInput());

            }

            //대기메뉴 키 누를 경우 _231231 배경택
            if (!CheckAnyUITrue() && reader.ReadyMenuKey) 
            {
                if (readyMenuUI.activeSelf == true)
                {
                    Controller_Physics.SwitchCameraLock(false);
                    readyMenuUI.SetActive(false); // 중복 버튼을 누를경우 꺼짐
                }
                else
                {
                    Controller_Physics.SwitchCameraLock(true);
                    readyMenuUI.SetActive(true);
                    recordUI.SetActive(false);
                    storeUI.SetActive(false);
                }

                StartCoroutine(DelayInput());

            }

            if (Input.GetKeyDown(KeyCode.Escape)) // 그냥 ESC키 누를경우 꺼짐 (환경설정키가 ESC로 되어있음에 따라 환경설정키는 조건에서 제외)
            {
                if (storeUI.activeSelf == true) // 상점 UI
                {
                    Controller_Physics.SwitchCameraLock(false);
                    storeUI.SetActive(false);
                }
                else if (recordUI.activeSelf == true) // 도감 UI
                {
                    Controller_Physics.SwitchCameraLock(false);
                    recordUI.SetActive(false);
                }
                else if (explainUI.activeSelf == true) // 설명 UI
                {
                    Controller_Physics.SwitchCameraLock(false);
                    explainUI.SetActive(false);
                }
                else if (firstCoinExPlainUI.activeSelf == true) // 첫 코인 설명 UI
                {
                    Controller_Physics.SwitchCameraLock(false);
                    firstCoinExPlainUI.SetActive(false);
                }
                else if (savingUI.activeSelf == true && savingUI.GetComponent<SavingCanvas>().isSaved) // 저장 UI
                {                    
                    savingUI.SetActive(false);
                    readyMenuUI.SetActive(true);
                }
                else if (firstKeyExplainUI.activeSelf == true) // 첫 Key 설명 UI
                {
                    Controller_Physics.SwitchCameraLock(false);
                    firstKeyExplainUI.SetActive(false);
                }
                else if(settingsUI.activeSelf == true) // 환경설정 UI
                {
                    readyMenuUI.SetActive(true);
                    settingsUI.SetActive(false);
                }
                else if(keyRebindUI.activeSelf == true) // 키 리바인드 UI
                {
                    settingsUI.SetActive(true);
                    keyRebindUI.SetActive(false);
                }

                StartCoroutine(DelayInput());
            }
        }        
    }

    // 대기메뉴를 위한 체크
    public bool CheckAnyUITrue()
    {
        if (storeUI.activeSelf
            || recordUI.activeSelf
            || explainUI.activeSelf
            || firstCoinExPlainUI.activeSelf
            || firstKeyExplainUI.activeSelf
            || savingUI.activeSelf
            || recordPopUpItemUI.activeSelf
            || settingsUI.activeSelf
            || keyRebindUI.activeSelf
            || recordPopUpItemUI.activeSelf
            ) return true;

        return false;
    }

    // 설명, 설정 관련 UI 켜져있는지 체크
    public bool CheckSetAndExplainUITrue()
    {
        if (readyMenuUI.activeSelf
            || explainUI.activeSelf
            || firstCoinExPlainUI.activeSelf
            || firstKeyExplainUI.activeSelf
            || savingUI.activeSelf
            || recordPopUpItemUI.activeSelf
            || settingsUI.activeSelf
            || keyRebindUI.activeSelf
            || recordPopUpItemUI.activeSelf
            ) return true;

        return false;
    }

    // 전체 UI 켜져있는지 체크
    public bool CheckAllUISetActiveTrue()
    {
        if (storeUI.activeSelf
            || recordUI.activeSelf
            || CheckSetAndExplainUITrue()
            ) return true;

        return false;
    }


    // 입력 대기시간 코루틴
    IEnumerator DelayInput()
    {
        canInput = false; // 입력 불가
        yield return new WaitForSecondsRealtime(INPUT_DELAYTIME); // 대기시간
        canInput = true; // 입력 가능
    }
}
