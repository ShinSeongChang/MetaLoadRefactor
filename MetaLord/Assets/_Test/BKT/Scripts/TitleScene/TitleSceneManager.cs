using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleSceneManager : MonoBehaviour
{
    [Header("Title Display")]
    [SerializeField] private GameObject anykeyDisplay; //최초 아무키 누르면 사라지는 디스플레이
    [SerializeField] private GameObject titleDisplay; // 타이틀 화면

    private bool isPushAnyKey = false;

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //}

        if (!isPushAnyKey && Input.anyKeyDown) //아무키나 누르면 타이틀 화면이 나옴
        {
            isPushAnyKey = true;
            anykeyDisplay.SetActive(false);
            titleDisplay.SetActive(true);
            AudioManager.instance.PlaySFX(AudioManager.instance.clip_Click);
        }
    }

    // 나가기 버튼 게임 종료
    public void onExitButton()
    {
        Application.Quit();
    }
}