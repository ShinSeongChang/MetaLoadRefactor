using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 지역 정렬 클래스
/// 231231 배경택
/// </summary>
public class SortZone : MonoBehaviour
{
    [SerializeField] private Zone zone;

    public void ClickButton()
    {
        RecordManager.instance.SortZone((int)zone);
    }
}
