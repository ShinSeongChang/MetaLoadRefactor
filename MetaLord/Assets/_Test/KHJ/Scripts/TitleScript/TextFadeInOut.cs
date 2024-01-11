using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFadeInOut : MonoBehaviour
{
    [SerializeField]
    private float fadeTime;  //페이드 되는 시간
    private TMP_Text fadeText; //페이드 효과에 사용되는 텍스트
                             // private Coroutine fadeInOut;
    private void Awake()
    {
        fadeText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        StartCoroutine(FadeInOut());
    }

    private void OnDisable()
    {
        //StopCoroutine(fadeInOut);
        StopCoroutine(FadeInOut());
    }

    private IEnumerator FadeInOut()
    {
        while (true)
        {
            yield return StartCoroutine(Fade(1, 0));

            yield return StartCoroutine(Fade(0, 1));
        }
    }

    private IEnumerator Fade(float start, float end)
    {
        float current = 0;
        float percent = 0;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / fadeTime;

            Color color = fadeText.color;
            color.a = Mathf.Lerp(start, end, percent);
            fadeText.color = color;

            yield return null;
        }

    }
}
