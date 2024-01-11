using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestRecordItem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] int id = 1;


    public void OnPointerDown(PointerEventData eventData)
    {
        GameEventsManager.instance.recordEvents.GetRecordItem(id);
        Debug.Log("Touch" + id);
    }
}
