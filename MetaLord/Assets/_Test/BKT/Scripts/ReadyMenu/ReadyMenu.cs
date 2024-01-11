using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
/// <summary>
/// 대기화면 메뉴
/// 231226 배경택
/// </summary>
public class ReadyMenu : MonoBehaviour
{

    [SerializeField] private GameObject canvases;

    private GameObject gameExplainCanvas;
    private GameObject settingsCanvas;
    private GameObject savingCanvas;


    private void Awake()
    {
        gameExplainCanvas = Utility.FindChildObj(canvases, "ExplainCanvas");
        settingsCanvas = Utility.FindChildObj(canvases, "SettingCanvas");
        savingCanvas = Utility.FindChildObj(canvases, "SavingCanvas");
    }

    // 돌아가기 버튼
    public void BackToGame()
    {
        gameObject.SetActive(false);
        CanUseSound();

        Controller_Physics.SwitchCameraLock(false);
    }

    // 도움말 버튼
    public void GameExplain()
    {
        gameExplainCanvas.SetActive(true);
        gameObject.SetActive(false);
        CanUseSound();

    }

    // 환경설정창으로 이동
    public void GoSettings()
    {
        settingsCanvas.SetActive(true);
        gameObject.SetActive(false);
        CanUseSound();
    }

    // 로비로 이동
    public void GoLobby()
    {
        SoundManager.instance.gameObject.SetActive(false);

        LoadingController.LoadScene("TitleScene");
        StartInfo.instance.SetPersonalSetting();
        //SceneManager.LoadScene("TitleScene");
    }

    // 게임 저장
    public void SaveGame()
    {        
        DataManager.instance.SaveGameData();        
        savingCanvas.GetComponent<SavingCanvas>().StartSave();
        gameObject.SetActive(false);
    }

    // 임시 버튼 사용 불가 소리
    public void CantUseSound()
    {
        // 사운드 추가
        SoundManager.instance.PlaySound(GroupList.UI, (int)UISoundList.Cant_BuySound);
    }

    // 사용 가능소리
    private void CanUseSound()
    {
        // 사운드 추가       
        SoundManager.instance.PlaySound(GroupList.UI, (int)UISoundList.Can_BuySound);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
