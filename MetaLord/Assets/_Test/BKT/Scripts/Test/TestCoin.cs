using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 테스트 코인, OnPointerDown 함수 내용 추후 먹는 코인에 반영하면 됨
/// 231130_배경택
/// </summary>
public class TestCoin : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private CoinType mytype; // 인스펙터창에서 코인 타입 선택

    [SerializeField] private const int SMALL_COIN_VALUE = 5; // 작은코인 값
    [SerializeField] private const int BIG_COIN_VALUE = 100; // 큰 코인 값

    // 화면에서 마우스로 클릭시 실행되는 함수
    public void OnPointerDown(PointerEventData eventData)
    {
        if(mytype == CoinType.SMALL_COIN) // 작은 코인일 경우
        {
            CoinManager.instance.GetCoin(SMALL_COIN_VALUE);
        }
        else if(mytype == CoinType.BIG_COIN) // 큰 코인일 경우
        {
            CoinManager.instance.GetCoin(BIG_COIN_VALUE);
        }
    }
}