using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 오디오 매니저
/// 231215 배경택
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] AudioMixer audioMixer;

    [Header("오디오 소스")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("오디오 클립"),Tooltip("필요한 오디오 클립을 끌어다 넣어주세요")]
    public AudioClip clip_Background;
    public AudioClip clip_Click;    

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PlayBackGroundMusic(); // 최초 음악시작 -> 음악시작위치 조정 필요시 위치 변경

        // 음악 소스 뮤트
        if (StartInfo.instance.info_MasterToggle == 1)
        {
            if (StartInfo.instance.info_BGMToggle == 0) musicSource.mute = true;
            else musicSource.mute = false;

            if (StartInfo.instance.info_SFXToggle == 0) SFXSource.mute = true;
            else SFXSource.mute = false;
        }
        else
        {
            musicSource.mute = true;
            SFXSource.mute = true;
        }
        
        musicSource.volume = StartInfo.instance.info_MasterSlider;
        SFXSource.volume = StartInfo.instance.info_MasterSlider;
    }

    // 최초 음악 시작 시점
    public void PlayBackGroundMusic()
    {
        if(clip_Background != null)
        {
            musicSource.clip = clip_Background;
            musicSource.Play();
        }
        else Debug.Log("오디오 파일이 없습니다.");
    }

    // 배경 음악 변경시
    public void ChangeBackGroundMusic(AudioClip clip)
    {
        if(clip != null) musicSource.clip = clip;
        else Debug.Log("오디오 파일이 없습니다.");
    }

    // 효과음 실행시
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) SFXSource.PlayOneShot(clip);
        else Debug.Log("오디오 파일이 없습니다.");
    }

}
