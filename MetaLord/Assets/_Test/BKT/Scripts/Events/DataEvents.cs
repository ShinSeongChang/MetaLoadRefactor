using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 데이터 이벤트(세이브,로드) 클래스
/// 231227 배경택
/// </summary>
public class DataEvents
{
    //저장 이벤트
    public event Action onSaveData;

    public void SaveData()
    {
        if (onSaveData != null)
        {
            onSaveData();
        }
    }

    //불러오기 이벤트
    public event Action onLoadData;

    public void LoadData()
    {
        if (onLoadData != null)
        {
            onLoadData();
        }
    }
}
