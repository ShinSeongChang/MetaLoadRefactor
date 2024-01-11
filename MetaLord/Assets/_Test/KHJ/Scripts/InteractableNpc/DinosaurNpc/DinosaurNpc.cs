using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinosaurNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        // 공룡 18
        myDialogue.CheckStateDialogue(18, state);
    }
}

