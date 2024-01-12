using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventurerNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        // 모험가 30
        myDialogue.CheckStateDialogue(30, state);
    }
}