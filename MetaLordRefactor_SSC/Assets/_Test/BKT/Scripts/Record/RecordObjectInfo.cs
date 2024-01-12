using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 도감 오브젝트의 정보_CSV를 통해 반영
/// 231130_배경택
/// </summary>
[Serializable]
public class RecordObjectInfo
{
    public int id;
    public string id_Description;
    public string item_Name;
    public int zone; // 1 = 주방, 2= 거실, 3= 아기방
    public bool obtained;
    public string description;    
}
