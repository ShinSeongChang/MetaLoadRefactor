using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        //오리 39
        myDialogue.CheckStateDialogue(39, state);
    }
}

