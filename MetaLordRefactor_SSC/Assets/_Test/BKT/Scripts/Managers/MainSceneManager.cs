using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인씬에서 한번에 MoveObject에 id값을 입력해주는 함수
/// </summary>
public class MainSceneManager : MonoBehaviour
{
    static public MainSceneManager instance;

    [SerializeField] private GameObject endingCanvas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;            
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        InputIdToObject();
    }

    // 오브젝트 ID값 이름 순서로 정렬하여 입력
    public void InputIdToObject()
    {
        MoveObjectData[] moveObjects = FindObjectsByType<MoveObjectData>(FindObjectsSortMode.None);
        Coin[] coinObjects = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        
        Array.Sort(moveObjects, (first,second) => string.Compare(first.name, second.name));
        Array.Sort(coinObjects, (first,second) => string.Compare(first.name, second.name));

        for (int i = 0; i < moveObjects.Length; i++)
        {
            moveObjects[i].id = i;            
        }

        for(int i = 0; i < coinObjects.Length; i++)
        {            
            coinObjects[i].id = i;
        }
    }

    // 게임 끝나는 시점에 호출하는 함수
    public void EndGame()
    {
        StartCoroutine(EndingCredit());
    }

    IEnumerator EndingCredit()
    {
        yield return new WaitForSeconds(2f);        
        endingCanvas.SetActive(true);
    }
}
