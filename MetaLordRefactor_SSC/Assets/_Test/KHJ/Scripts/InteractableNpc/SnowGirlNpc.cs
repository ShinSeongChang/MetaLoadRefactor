using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowGirlNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        // 눈사람 27
        myDialogue.CheckStateDialogue(27, state);
    }
}