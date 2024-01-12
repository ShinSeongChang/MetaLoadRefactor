using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarwhalNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        //뿔고래 24
        myDialogue.CheckStateDialogue(24, state);
    }
}

