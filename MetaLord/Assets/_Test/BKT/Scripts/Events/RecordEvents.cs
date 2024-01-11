using System;
using UnityEngine;

/// <summary>
/// 도감 이벤트
/// </summary>
public class RecordEvents
{
    /// <summary>
    /// 아이템을 먹었을때 발생하는 이벤트
    /// 231130 배경택
    /// </summary>
    public event Action<int> onGetRecordItem;
    public void GetRecordItem(int _id)
    {
        if(onGetRecordItem != null) // 이벤트함수가 비어있지 않다면
        {
            onGetRecordItem(_id);
        } 
    }

    /// <summary>
    /// 도감 변경사항이 있을때 발생하는 이벤트
    /// 231204 배경택
    /// </summary>
    public event Action onChangeRecord;
    public void ReflectRecord()
    {
        if(onChangeRecord != null)
        {
            onChangeRecord();
        }
    }

    // 도감 선택시 발생하는 이벤트
    public event Action<int> onSelectRecord;
    public void SelectRecord(int _id)
    {
        if(onSelectRecord != null)
        {
            onSelectRecord(_id);
        }
    }

}
