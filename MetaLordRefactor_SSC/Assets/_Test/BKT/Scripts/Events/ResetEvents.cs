using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetEvents
{
    // 코인 얻었을때 호출되는 이벤트 
    public event Action onResetAllBindings;

    public void ResetAllBindings()
    {
        if (onResetAllBindings != null)
        {
            onResetAllBindings();
        }
    }

}
