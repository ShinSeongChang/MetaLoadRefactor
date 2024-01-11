using System;
using UnityEngine;

/// <summary>
/// 유틸리티 클래스
/// 231219 배경택
/// </summary>
public static class Utility
{
    // 도감 아이템을 Enum순서대로 가져오기 위한 배열
    private static RecordList[] recordListArr =
        {
            RecordList.FrenchFries,
            RecordList.FishBones,
            RecordList.BrokenPottery,
            RecordList.WineGlass,
            RecordList.ChineseKnife,
            RecordList.ForkSpoonSet,
            RecordList.CupCake,
            RecordList.HalloweenPumpkin,
            RecordList.AcousticGuitar,
            RecordList.CandyCane,
            RecordList.ToyShovel,
            RecordList.RecordCoin,
            RecordList.WoodenTrain,
            RecordList.BlueStar,
            RecordList.RemoteControl,
            RecordList.Cube,
            RecordList.WoodenShield,
            RecordList.GoldenChessPiece,
            RecordList.LightSaber,
            RecordList.TechDeck,
        };

    // 외부에서 아이템 호출시
    public static int GetRecordId(int index)
    {
        return (int)recordListArr[index];
    }

    /// <summary>
    /// 자식 오브젝트 찾는 함수
    /// 231220 배경택
    /// </summary>
    /// <param name="_rootObj"> 최상단 오브젝트 </param>
    /// <param name="_objName"> 찾고싶은 오브젝트 이름 </param>
    /// <returns></returns>
    public static GameObject FindChildObj(this GameObject _rootObj, string _objName)
    {
        GameObject resultObject = default; // 결과 오브젝트
        GameObject tempObject = default; // 임시 저장을 위한 오브젝트
        for (int i = 0; i < _rootObj.transform.childCount; i++) // 자식 오브젝트 숫자만큼 순회
        {
            tempObject = _rootObj.transform.GetChild(i).gameObject;
            if (tempObject.name.Equals(_objName)) // 임시오브젝트명과 찾는 이름이 같다면
            {
                resultObject = tempObject; // 결과 오브젝트에 임시 오브젝트 저장
                return resultObject; // 결과 반환
            }
            else
            {
                resultObject = FindChildObj(tempObject, _objName); // 재귀적으로 검색

                // 예외사항
                if (resultObject == null || resultObject == default) { /* Pass */ }
                else { return resultObject; } // 결과를 찾았다면 반환
            }
        }

        return resultObject;
    }
}

/// <summary>
/// 구역 Enum
/// </summary>
public enum Zone
{
    Total = 0,
    Kitchen = 1,
    LivingRoom = 2,
    BabyRoom = 3
}

/// <summary>
/// 도감 아이템 Enum
/// </summary>
public enum RecordList
{
    FrenchFries = 100,
    FishBones = 101,
    BrokenPottery = 102,
    WineGlass = 103,
    ChineseKnife = 104,
    ForkSpoonSet = 105,
    CupCake = 106,
    HalloweenPumpkin = 200,
    AcousticGuitar = 201,
    CandyCane = 202,
    ToyShovel = 203,
    RecordCoin = 204,
    WoodenTrain = 205,
    BlueStar = 300,
    RemoteControl = 301,
    Cube = 302,
    WoodenShield = 303,
    GoldenChessPiece = 304, 
    LightSaber = 305,
    TechDeck = 306,
}

/// <summary>
/// 코인 타입
/// </summary>
public enum CoinType
{
    SMALL_COIN,
    BIG_COIN
}

/// <summary>
/// 총 발사 모드
/// </summary>
public enum GunMode
{
    Paint,
    Grab,
    Bond,
}

// 업그레이드 종류
public enum UpgradeCategory
{
    Range = 0,
    Amount = 1
}


// 환경설정 버튼 사이즈
public enum ButtonSize
{
    Small = 0,
    Middle,
    Large
}