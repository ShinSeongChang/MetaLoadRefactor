using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// 정렬 UI 클래스
/// 231220 배경택
/// </summary>
public class UI_Sort : MonoBehaviour
{
    // 켜질때 정렬 초기화
    private void OnEnable()
    {
        if (RecordManager.instance != null) RecordManager.instance.ResetRecord();
        else{ /* pass */ }
    }
}
