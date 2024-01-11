using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GameSceneController;

public class TitleController : MonoBehaviour
{
    SceneList currentScene;
    SceneList nextScene;
    SceneList continueScene;

    private void Awake()
    {
        currentScene = SceneList.TitleScene;
        nextScene = SceneList.StoryScene;
        continueScene = SceneList.MainScene;
    }

    public void StartGame()
    {
        if(SoundManager.instance != null)
        {
            SoundManager.instance.gameObject.SetActive(false);            
        }

        StartInfo.instance.isLoaded = false;

        StartCoroutine(LoadSceneAsync(currentScene, nextScene));

    }

    public void ContinueGame()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.gameObject.SetActive(false);
        }

        StartInfo.instance.isLoaded = true;

        string jsonFilePath = Path.Combine(Application.persistentDataPath, "JsonData");        

        if (File.Exists(jsonFilePath))
        {
            LoadingController.LoadScene("MainScene");
        }
        else
        {
            StartGame();
        }
            //StartCoroutine(LoadSceneAsync(currentScene, continueScene));
    }
}
