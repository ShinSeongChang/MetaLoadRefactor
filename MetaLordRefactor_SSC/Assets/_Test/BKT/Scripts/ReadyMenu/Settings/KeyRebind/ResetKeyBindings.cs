using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ResetKeyBindings : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    // 전체 바인딩 리셋
    public void ResetAllBindings()
    {
        foreach (InputActionMap map in inputReader.inputActions.asset.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
        GameEventsManager.instance.resetEvents.ResetAllBindings();
    }
}
