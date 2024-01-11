using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObiruWillberNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        //오비르 윌버 21
        myDialogue.CheckStateDialogue(21, state);
    }
}


