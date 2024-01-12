using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        myDialogue.CheckStateDialogue(12, state);
    }
}
