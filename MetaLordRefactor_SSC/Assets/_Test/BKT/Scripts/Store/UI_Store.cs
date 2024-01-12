using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 상점 UI 클래스
/// 231220 배경택
/// </summary>
public class UI_Store : MonoBehaviour
{
    [SerializeField] private GameObject currentCoinUI;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {        
        currentCoinUI.transform.GetChild(0).GetChild(1).GetComponent<UI_CurrentCoin>().StopFadeOut();
        currentCoinUI.GetComponent<CanvasGroup>().alpha = 1f;
    }

    private void OnDisable()
    {
        currentCoinUI.GetComponent<CanvasGroup>().alpha = 0f;
    }
}
