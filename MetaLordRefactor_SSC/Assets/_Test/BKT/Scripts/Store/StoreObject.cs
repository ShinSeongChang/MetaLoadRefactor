using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 구매물품에 들어가는 스크립트
/// 231204 배경택
/// </summary>
public class StoreObject : MonoBehaviour
{
    protected int price;
    //private GameObject cantBuyImage;
    private GameObject button; 

    protected bool isCanBuy = true;

    [SerializeField] private Sprite canBuyImage;
    [SerializeField] private Sprite canNotBuyImage;

    protected virtual void Awake()
    {        
        //cantBuyImage = Utility.FindChildObj(this.gameObject, "Image(CantBuy)");
        button = Utility.FindChildObj(this.gameObject, "Button");
        GameEventsManager.instance.coinEvents.onChangeCoin += ChangeButtonUI;       
        GameEventsManager.instance.dataEvents.onSaveData += SaveData;
        GameEventsManager.instance.dataEvents.onLoadData += LoadData;
    }

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        //ChangeButtonUI(0);
    }

    protected virtual void SaveData()
    {
        /* pass */
    }

    protected virtual void LoadData()
    {
        Debug.Log("켜졌다");
        gameObject.SetActive(true);
        ChangeButtonUI(0);
    }

    private void OnDestroy()
    {
        GameEventsManager.instance.coinEvents.onChangeCoin -= ChangeButtonUI;

        GameEventsManager.instance.dataEvents.onSaveData -= SaveData;
        GameEventsManager.instance.dataEvents.onLoadData -= LoadData;

    }

    // 상점 UI버튼 누르면 실행되는 함수
    protected virtual void BuyStoreObject() 
    {        
        CoinManager.instance.UseCoin(price); //코인 반영
    }

    /// <summary>
    /// 버튼 검정색 이미지로 덮을지 덮지 않을지 변경
    /// </summary>
    protected void ChangeButtonUI(int temp)
    {
        if (IsCanBuy()) // 구매 가능
        {            
            button.GetComponent<Image>().sprite = canBuyImage;            
        }
        else // 구매 불가
        {            
            button.GetComponent<Image>().sprite = canNotBuyImage;            
        }
    }

    // 구매 가능여부를 체크하는 함수
    protected bool IsCanBuy()
    {
        if (CoinManager.instance.currentCoin >= price && isCanBuy) return true;        
        else return false;
    }

    /// <summary>
    /// 구매 불가 소리 재생
    /// </summary>
    protected void PlayCantBuySound()
    {
        // 사운드 추가
        SoundManager.instance.PlaySound(GroupList.UI, (int)UISoundList.Cant_BuySound);
    }

    /// <summary>
    /// 구매 가능 소리 재생
    /// </summary>
    protected void PlayCanBuySound()
    {
        // 사운드 추가       
        SoundManager.instance.PlaySound(GroupList.UI, (int)UISoundList.Can_BuySound);
    }
}
