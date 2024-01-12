using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlayTest : MonoBehaviour
{
    [SerializeField] private GameObject videoPlayer;
    private AudioSource BGMSource;
    private AudioSource SFXSource;

    float save_BGMVolume;
    float save_SFXVolume;

    public void Start()
    {
        if (SoundManager.instance != null)
        {
            BGMSource = SoundManager.instance.transform.GetChild(0).GetComponent<AudioSource>();
            SFXSource = SoundManager.instance.transform.GetChild(1).GetComponent<AudioSource>();
        }

        save_BGMVolume = BGMSource.volume;
        save_SFXVolume = SFXSource.volume;
        BGMSource.volume = 0.3f;
        SFXSource.volume = 0.3f;
    }

    public void TestPlayVideo()
    {
        videoPlayer.SetActive(true);
        BGMSource.mute = true;
        SFXSource.mute = true;

    }

    public void OffCanvas()
    {
       StartCoroutine(OffGame());

    }

    IEnumerator OffGame() 
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
        BGMSource.mute = false;
        SFXSource.mute = false;
        BGMSource.volume = save_BGMVolume;
        SFXSource.volume = save_SFXVolume;
    }
}
