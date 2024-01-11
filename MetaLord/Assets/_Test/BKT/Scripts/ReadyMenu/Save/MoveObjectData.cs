using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;


/// <summary>
/// 움직이는 오브젝트 데이터 저장 및 로드를 위한 클래스
/// 231227 배경택
/// </summary>
public class MoveObjectData : MonoBehaviour
{
    public Vector3 pos;
    public Vector3 rotation;
    public bool isMoved;

    [Header("움직이는 오브젝트 ID")]
    public int id; // 배열의 인덱스에 저장을 하기위한 id

    private void Awake()
    {
        GameEventsManager.instance.dataEvents.onSaveData += SaveData;
        GameEventsManager.instance.dataEvents.onLoadData += LoadData;
        
    }

    private void Start()
    {
        // 게임시작하며 현재 position 저장
        pos = transform.position;
        rotation = transform.eulerAngles;
    }

    private void OnDestroy()
    {
        GameEventsManager.instance.dataEvents.onSaveData -= SaveData;
        GameEventsManager.instance.dataEvents.onLoadData -= LoadData;
        
    }

    // 오브젝트 저장
    public void SaveData()
    {
        if (pos != transform.position) 
        {
            isMoved = true;
            pos = transform.position;
            rotation = transform.eulerAngles;
        }

        string jsonData = JsonUtility.ToJson(GetComponent<MoveObjectData>());   // 저장할 Json Data        

        DataManager.instance.savedGamePlayData.recordItemTransform[id] = jsonData;
    }

    // 오브젝트 불러오기
    public void LoadData()
    {
        string jsonData = DataManager.instance.savedGamePlayData.recordItemTransform[id]; // 불러올 Json Data

        JsonUtility.FromJsonOverwrite(jsonData,GetComponent<MoveObjectData>()); // json 파일 덮어쓰기

        transform.position = pos;
        transform.eulerAngles = rotation;
        
        // 움직였던 오브젝트라면 물리력 주입
        if (isMoved) 
        {
            GetComponent<MovedObject>().InitOverap();
        }

        // 불러온 오브젝트들은 전부 움직이지 않았던 오브젝트로 설정
        isMoved = false;
    }
}
