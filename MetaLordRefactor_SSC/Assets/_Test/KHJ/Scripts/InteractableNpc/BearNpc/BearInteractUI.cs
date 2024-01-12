using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BearInteractUI : MonoBehaviour
{
    private readonly float fadeTime = 1f;
    public Image fadeImage = default;

    public bool isCanInteractUI = false;

    bool isFadeIn = false;
    bool isFadeOut = false;

    public SphereCollider interactCollider;
    public LayerMask waterLayer;

   
    bool isPlayerIn = false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerIn = true;
            isCanInteractUI = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (isPlayerIn)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, waterLayer))
            {
                if(isFadeIn)
                {                    
                    return;
                }

                StartCoroutine(FadeInImage());                
            }
            else
            {
                if(isFadeOut)
                {                    
                    return;
                }

                StartCoroutine(FadeOutImage());
            }

            //CheckHeadCollider(ray);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerIn = false;
            isCanInteractUI = false;
        }
    }
    public void CheckHeadCollider(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, waterLayer))
        {         
            StartCoroutine(FadeInImage());
            return;
        }

    }

    private IEnumerator FadeInImage()
    {
        if(fadeImage.color.a >= 1f)
        {
            yield break;
        }

        isFadeIn = true;

        float current = 0;
        float percent = 0;
        float start = 0f;
        float end = 1f;
        while (percent < 1)
        {
            //isFadeIn = true;

            current += 2f * Time.deltaTime;
            percent = current / fadeTime;

            Color color = fadeImage.color;
            color.a = Mathf.Lerp(start, end, percent);
            fadeImage.color = color;

            yield return null;
        }        

        isFadeIn = false;
    }
    private IEnumerator FadeOutImage()
    {
        if (fadeImage.color.a <= 0f)
        {
            yield break;
        }

        isFadeOut = true;

        float current = 0;
        float percent = 0;
        float start = 1f;
        float end = 0f;

        while (percent < 1)
        {
            isFadeOut = true;

            current += 2f * Time.deltaTime;
            percent = current / fadeTime;

            Color color = fadeImage.color;
            color.a = Mathf.Lerp(start, end, percent);
            fadeImage.color = color;

            yield return null;
        }     
        
        isFadeOut = false;

    }

}
