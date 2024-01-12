using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/// <summary>
/// 로딩씬 컨트롤러
/// 240104 배경택
/// </summary>
public class LoadingController : MonoBehaviour
{
    static string nextScene;

    static private bool backToTitle;

    [SerializeField] Image progressBar;

    public static void LoadScene(string sceneName)
    {
        if (sceneName == "TitleScene") backToTitle = true;
        else backToTitle = false;

        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene);
        operation.allowSceneActivation = false;

        float timer = 0f;

        // 비동기 씬이 로딩되었을때 실행되는 이벤트
        operation.completed += OnLoadComplete;

        while (!operation.isDone)
        {
            yield return null;

            if(operation.progress < 0.9f)
            {
                progressBar.fillAmount = operation.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                if(progressBar.fillAmount >= 1f)
                {
                    
                    operation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    // 씬 로딩이 완료될 경우 실행되는 함수
    private void OnLoadComplete(AsyncOperation _operation)
    {
        // 사운드 매니저가 만들어진적이 있다면 재생
        if (SoundManager.instance != null && backToTitle == false)
        {
            SoundManager.instance.gameObject.SetActive(true);
        }

        // 불러오기라면 불러오기 실행
        if (StartInfo.instance.isLoaded == true)
        { 
            DataManager.instance.LoadGameData();                    
        }
    }
}
