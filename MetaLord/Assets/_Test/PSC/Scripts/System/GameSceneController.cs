using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController
{
    static GameObject gameObject;

    public void LoadScene(SceneList currentScene, SceneList nextScene)
    {
        
    }

    public static IEnumerator LoadSceneAsync(SceneList currentScene, SceneList nextScene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene.ToString());


        while (!asyncLoad.isDone)
        {
            yield return new WaitForSeconds(10f);
        }

        // 현재 씬을 비활성화 또는 종료
        //gameObject.SetActive(false); // 현재 스크립트를 가진 게임 오브젝트를 비활성화
        // 또는
        SceneManager.UnloadSceneAsync(currentScene.ToString()); // 현재 씬을 언로드
    }

    public enum SceneList
    {
        TitleScene,
        StoryScene,
        MainScene
    }
}
