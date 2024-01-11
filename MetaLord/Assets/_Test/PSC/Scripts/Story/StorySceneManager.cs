using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StorySceneManager : MonoBehaviour
{
    [SerializeField]
    AudioSource sfxAudio;
    [SerializeField]
    AudioSource bgmAudio;
    [SerializeField]
    Image image;
    
    [SerializeField]
    Sprite[] images;
    [SerializeField]
    float[] imageTimes;
    [SerializeField]
    int footIndex = 17;
    bool canSkip = true;

    [SerializeField, Range(0, 1)]
    float bgmVolume;

    [SerializeField, Range(0, 1)]
    float sfxVolume;


    private void Start()
    {
        StartCoroutine(PlayStoryRoutine());

        // 음악 소스 뮤트
        if (StartInfo.instance.info_MasterToggle == 1)
        {
            if (StartInfo.instance.info_BGMToggle == 0) bgmAudio.mute = true;
            else bgmAudio.mute = false;

            if (StartInfo.instance.info_SFXToggle == 0) sfxAudio.mute = true;
            else sfxAudio.mute = false;
        }
        else
        {
            bgmAudio.mute = true;
            sfxAudio.mute = true;
        }

        bgmAudio.volume = StartInfo.instance.info_MasterSlider * bgmVolume;
        sfxAudio.volume = StartInfo.instance.info_MasterSlider * sfxVolume;
    }

    /*private void Update()
    {
        if (Input.anyKey && canSkip)
        {
            canSkip = false;
            StopAllCoroutines();
            LoadNextScene();
        }
    }*/

    IEnumerator PlayStoryRoutine()
    {
        int currIndex = 0;
        while (currIndex < images.Length)
        {
            image.sprite = images[currIndex];
            yield return new WaitForSecondsRealtime(imageTimes[currIndex] * 0.5f);
            if (footIndex == currIndex)
            {
                sfxAudio.Play();
                StartCoroutine(VolumeUpRoutine(Time.deltaTime * 2, 20));
            }
            yield return new WaitForSecondsRealtime(imageTimes[currIndex] * 0.5f);
            currIndex += 1;
        }
        bgmAudio.Stop();
        yield return new WaitForSecondsRealtime(1f);
        if (canSkip) LoadNextScene();
    }

    IEnumerator VolumeUpRoutine(float time, int count)
    {
        while (count>0)
        {
            bgmAudio.volume += Time.unscaledDeltaTime;
            yield return new WaitForSecondsRealtime(time);
        }
    }

    public void LoadNextScene()
    {
        LoadingController.LoadScene("MainScene");
        //StartCoroutine(GameSceneController.LoadSceneAsync(currentScene, nextScene));
    }

}
