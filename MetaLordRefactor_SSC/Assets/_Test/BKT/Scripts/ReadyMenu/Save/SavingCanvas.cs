using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 저장 캔버스 UI
/// 배경택
/// </summary>
public class SavingCanvas : MonoBehaviour
{
    public bool isSaved = false; // 저장이 완료 된 이후 키 조작이 가능하도록 하기 위한 변수

    private void OnDisable()
    {
        transform.GetChild(1).gameObject.SetActive(true); // 저장 중 오브젝트 ON
        transform.GetChild(2).gameObject.SetActive(false); // 저장 완료 오브젝트 OFF
        isSaved = false;
    }

    public void StartSave()
    {
        gameObject.SetActive(true);
        Controller_Physics.SwitchCameraLock(true);
        StartCoroutine(PopSavingUI());
    }

    // 저장 UI창 띄우기
    IEnumerator PopSavingUI()
    {
        string dot = default;
        
        for (int i = 0; i < 3; i++) // . .. ... 점 세개 생성
        {
            dot += ".";
            Debug.Log(dot);
            transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = dot;
            yield return new WaitForSecondsRealtime(0.8f);
            Debug.Log("코루틴");
        }
        transform.GetChild(1).gameObject.SetActive(false); // 저장중 오브젝트 OFF
        transform.GetChild(2).gameObject.SetActive(true); // 저장 완료 오브젝트 ON

        yield return new WaitForSecondsRealtime(1f);
        Controller_Physics.SwitchCameraLock(false); // 카메라 락 해제
        isSaved = true; 
        gameObject.SetActive(false); // UI창 OFF
    }
}
