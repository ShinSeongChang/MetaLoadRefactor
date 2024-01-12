using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiFadeOut : MonoBehaviour
{
    float fadeOutspeed = 1f;
    Image myImage;
    Color imageOrigin;
    Coroutine fadeOut;


    private void Awake()
    {        
        myImage = GetComponent<Image>();
        imageOrigin = myImage.color;
    }

    private void Start()
    {
        Color tempColor = myImage.color;
        tempColor.a = 0f;
        myImage.color = tempColor;
    }

    public void InitFadeOut()
    {
        if(fadeOut != null || !gameObject.activeInHierarchy)
        {            
            return;
        }

        myImage.color = imageOrigin;
        fadeOut = StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        Color imageColor = myImage.color;

        while (myImage.color.a >= 0f)
        {                        
            imageColor.a -= Time.unscaledDeltaTime * fadeOutspeed;            
            myImage.color = imageColor;

            yield return null;
        }

        fadeOut = null;        
    }


    private void OnDisable()
    {
        fadeOut = null;
        Color tempColor = myImage.color;
        tempColor.a = 0f;
        myImage.color = tempColor;
    }
}
