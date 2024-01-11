using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 수집 아이템 스크립트
/// 231219 배경택
/// </summary>
public class RecordItem : MonoBehaviour
{
    [SerializeField] RecordList recordItem;

    const int FALSE = 1;
    const int TRUE = 0;
    private int isExist = TRUE; // 존재하는가

    private void Awake()
    {
        GameEventsManager.instance.dataEvents.onSaveData += SaveData;
        GameEventsManager.instance.dataEvents.onLoadData += LoadData;
        
    }

    private void Start()
    {
    }
 
    private void OnDestroy()
    {
        GameEventsManager.instance.dataEvents.onSaveData -= SaveData;
        GameEventsManager.instance.dataEvents.onLoadData -= LoadData;
    }

    // 수집아이템 활성화 여부 저장
    private void SaveData()
    {
        DataManager.instance.savedGamePlayData.coinAndRecordItem[(int)recordItem] = isExist;

    }

    // 수집아이템 활성화 여부 불러오기
    private void LoadData()
    {
        isExist = DataManager.instance.savedGamePlayData.coinAndRecordItem[(int)recordItem];
        CheckIsExist();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (EffectManager.instance)
                EffectManager.instance.PlayEffect(EffectList.ItemGet, transform.position, Quaternion.identity);


            GameEventsManager.instance.recordEvents.GetRecordItem((int)recordItem);

            // 전체 수집품 카운트 증가
            UpRecordCount();

            // 사운드 추가
            int id = (int)ItemSoundList.GetRecordItemSound;
            SoundManager.instance.PlaySound(GroupList.Item, id);
            
            isExist = FALSE;
            gameObject.SetActive(false);
        }
    }

    // 수집아이템 존재 여부 체크
    private void CheckIsExist()
    {
        if (isExist == FALSE)
        {
            GameEventsManager.instance.recordEvents.GetRecordItem((int)recordItem); // 도감에 적용
            gameObject.SetActive(false); //비활성화로 변경
            UpRecordCount(); // 전체 수집품 카운트 증가
        }
    }

    private void UpRecordCount()
    {
            RecordManager.instance.endGameCount++; // 게임 엔딩을 위해 게임종료 카운트 ++

    }
}
