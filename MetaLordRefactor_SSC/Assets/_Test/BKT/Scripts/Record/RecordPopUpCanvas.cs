using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 수집 아이템 획득시 팝업 캔버스
/// 231231 배경택
/// </summary>
public class RecordPopUpCanvas : MonoBehaviour
{
    private void Awake()
    {
        GameEventsManager.instance.recordEvents.onGetRecordItem += PopUI;
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEventsManager.instance.recordEvents.onGetRecordItem += PopUI;
    }

    // 획득시 UI 팝업
    private void PopUI(int _id)
    {
        // 아이디가 저장되어있다면 UI 팝업되지 않도록 return
        if (DataManager.instance.savedGamePlayData.coinAndRecordItem[_id] == 1) return;

        ReflectItemID(_id);
        gameObject.SetActive(true);
        Controller_Physics.SwitchCameraLock(true);
    }

    // ID값을 통해서 이미지에 반영
    private void ReflectItemID(int _id)
    {
        transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite =
            Resources.Load<Sprite>("Object/" + RecordManager.instance.recordObjectInfos[_id].id_Description);        
    }

    // 버튼 클릭시 UI 꺼짐
    public void ClickButton()
    {
        gameObject.SetActive(false);
        Controller_Physics.SwitchCameraLock(false);

        Debug.Log("현재 도감 카운트" + RecordManager.instance.recordObjectInfos);
        Debug.Log("도감 카운트" + RecordManager.instance.endGameCount);

        // 채운 도감개수가 전체 도감 개수 이상이라면 게임 종료
        if(RecordManager.instance.endGameCount >= RecordManager.instance.recordObjectInfos.Count)
        {            
            MainSceneManager.instance.EndGame(); 
        }
    }
}
