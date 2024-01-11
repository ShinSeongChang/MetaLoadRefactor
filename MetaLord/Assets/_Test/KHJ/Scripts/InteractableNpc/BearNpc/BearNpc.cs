using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        // 곰 36
        myDialogue.CheckStateDialogue(36, state);
    }

}
