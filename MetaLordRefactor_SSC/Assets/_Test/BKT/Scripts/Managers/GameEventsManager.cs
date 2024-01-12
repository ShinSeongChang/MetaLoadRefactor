using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임이벤트 매니저
/// 231129_배경택
/// </summary>
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance;

    public CoinEvents coinEvents; // 재화 이벤트
    public RecordEvents recordEvents; // 도감 이벤트
    public ResetEvents resetEvents; // 초기화 이벤트
    public DataEvents dataEvents; //데이터 이벤트

    private void Awake()
    {
        // 싱글턴 패턴 인스턴스가 존재할경우 파괴 아닐경우 instance 지정
        if (instance == null)
        {
            instance = this;            
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        coinEvents = new CoinEvents(); // 재화 이벤트 생성
        recordEvents = new RecordEvents(); // 도감 이벤트 생성
        resetEvents = new ResetEvents(); // 초기화 이벤트 생성
        dataEvents = new DataEvents(); // 데이터 이벤트 생성
    }
}
