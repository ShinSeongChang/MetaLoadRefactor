using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 현재 코인 보여주는 UI 클래스
/// 배경택
/// </summary>
public class UI_CurrentCoin : MonoBehaviour
    {

    [SerializeField] private GameObject storeUI;

    private TMP_Text coinText;
    private CanvasGroup canvasGroup;

    private Coroutine fadeCoroutine;
    private float fadeDuration = 1f;

    private  void Awake()
    {
        coinText = GetComponent<TMP_Text>();
        canvasGroup = transform.parent.parent.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.coinEvents.onChangeCoin += ReflectCoinToUI;

        if (CoinManager.instance != null)
        {
            ReflectCoinToUI(CoinManager.instance.currentCoin);

        }
    }

    private void OnDisable()
    {
        GameEventsManager.instance.coinEvents.onChangeCoin -= ReflectCoinToUI;
    }

    private  void ReflectCoinToUI(int coin)
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }
        else
        {
            Debug.LogError("coinText를 찾을 수 없습니다!");
        }

        if (canvasGroup != null && !storeUI.activeSelf) // 상점 UI가 꺼져있다면
        {
            if (fadeCoroutine != null)
            {                
                StopFadeOut();
            }

            canvasGroup.alpha = 1f;
            fadeCoroutine = StartCoroutine(FadeOutUI(canvasGroup));
        }
        else
        {
            /* pass */
        }
    }

    /// <summary>
    /// 페이드아웃 제거하는 함수
    /// 231221 배경택
    /// </summary>
    public void StopFadeOut()
    {
        if(fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    }


    /// <summary>
    /// 현재 UI 페이드아웃
    /// </summary>
    /// <param name="canvasGroup"></param>
    /// <returns></returns>
    IEnumerator FadeOutUI(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;          
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
