using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상점 스킬 해금 클래스
/// 231218 배경택
/// </summary>
public class Store_SkillUnlock : StoreObject
{
    [Header("스킬 정보")]
    [SerializeField] private string skillName;
    [SerializeField] private int cost;
    [SerializeField] private string skillExplain;

    private TMP_Text nameObject;
    private TMP_Text costObject;
    private TMP_Text explainObject;
    private GameObject soldOutObject;

    [SerializeField] private GunMode gunMode;

    protected override void Awake()
    {
        base.Awake();
        // 스킬 정보 입력할 게임 오브젝트 캐싱
        nameObject = Utility.FindChildObj(this.gameObject, "Text(Name)").GetComponent<TMP_Text>();
        costObject = Utility.FindChildObj(this.gameObject, "Text(Cost)").GetComponent<TMP_Text>();
        explainObject = Utility.FindChildObj(this.gameObject, "Text(Explain)").GetComponent<TMP_Text>();
        soldOutObject = Utility.FindChildObj(this.gameObject, "SoldOut");

        // 스킬 정보 입력
        nameObject.text = skillName;
        costObject.text = cost.ToString() + "개";
        explainObject.text = skillExplain;
        soldOutObject.SetActive(false);

        // 금액에 코스트 반영
        price = cost;
    }

    protected override void SaveData()
    {
        base.SaveData();       
        DataManager.instance.savedGamePlayData.skill_Catch = isCanBuy;   
    }

    protected override void LoadData()
    {
        isCanBuy = DataManager.instance.savedGamePlayData.skill_Catch;
        ReflectToPlayer();
        base.LoadData();   
    }


    // 스킬 구매시 호출되는 함수
    protected override void BuyStoreObject()
    {
        if (IsCanBuy() == false)
        {
            PlayCantBuySound();
            return;
        }
        else PlayCanBuySound();


        isCanBuy = false;
        base.BuyStoreObject();
        ReflectToPlayer();
    }

    private void ReflectToPlayer()
    {
        if (isCanBuy) return; // 구매 가능한 경우

        soldOutObject.SetActive(true);
        costObject.text = "-개";
        GameEventsManager.instance.coinEvents.UnlockGunMode(gunMode);
    }
}
