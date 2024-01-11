using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 키 설명하는 캔버스
// 배경택
public class KeyExplainCanvas : MonoBehaviour
{
    // 확인된적이 있는지 체크하는 bool 변수
    private bool isConfirm = false;

    private void Awake()
    {
        GameEventsManager.instance.dataEvents.onSaveData += SaveObject;
        GameEventsManager.instance.dataEvents.onLoadData += LoadObject;
    }

    private void Start()
    {
        if (isConfirm)
        {
            gameObject.SetActive(false); // 이미 확인되었다면 비활성화
            Controller_Physics.SwitchCameraLock(false);
        }
        else Controller_Physics.SwitchCameraLock(true); // 확인되지 않았다면 플레이어 움직임, 카메라 제한
    }

    private void OnDisable()
    {
        isConfirm = true;
    }

    private void OnDestroy()
    {
        GameEventsManager.instance.dataEvents.onSaveData -= SaveObject;
        GameEventsManager.instance.dataEvents.onLoadData -= LoadObject;        
    }

    // 저장
    private void SaveObject()
    {
        DataManager.instance.savedGamePlayData.ui_keyExplain = isConfirm;

    }

    // 불러오기
    private void LoadObject()
    {
        isConfirm = DataManager.instance.savedGamePlayData.ui_keyExplain;
    }
}
