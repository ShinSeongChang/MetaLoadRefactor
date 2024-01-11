using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public GameObject recordUI; // 여기에 UI GameObject를 연결합니다.
    public GameObject storeUI;
    public GameObject settingUI;
    
    private bool isRecordUIVisible = false; 
    private bool isSettingUIVisible = false;
    private bool isStoreUIVisible = false;

    private void Start()
    {
        recordUI.SetActive(false);
    }

    void Update()
    {
        // 수집 도감
        if (Input.GetKeyDown(KeyCode.I))
        {
            // I 키를 누르면 UI를 켜기 or 끄기
            isRecordUIVisible = !isRecordUIVisible;
            recordUI.SetActive(isRecordUIVisible);
        }

        // 상점
        if (Input.GetKeyDown(KeyCode.K))
        {
            // K 키를 누르면 UI를 켜기 or 끄기
            isStoreUIVisible = !isStoreUIVisible;
            storeUI.SetActive(isStoreUIVisible);
        }

        //환경설정창
        if (Input.GetKeyDown(KeyCode.W))
        {
            // W 키를 누르면 UI를 켜기 or 끄기
            isSettingUIVisible = !isSettingUIVisible;
            settingUI.SetActive(isSettingUIVisible);
        }

        // 저장
        if (Input.GetKeyDown(KeyCode.Z))
        {
            DataManager.instance.SaveGameData();
            Debug.Log("저장버튼 눌렸음");
        }

        // 불러오기
        if (Input.GetKeyDown(KeyCode.X))
        {
            DataManager.instance.LoadGameData();
            Debug.Log("불러오기버튼 눌렸음");

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ESC 키를 누르면 UI 끄기
            if (isRecordUIVisible)
            {
                recordUI.SetActive(false);
                isRecordUIVisible = false;
            }
            else if (isSettingUIVisible)
            {
                settingUI.SetActive(false);
                isSettingUIVisible = false;
            }
            else if (isStoreUIVisible)
            {
                storeUI.SetActive(false);
            }
        }


    }
}
