using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeNpc : NpcBase, IInteractNpc
{
    //꿀벌 : 15
    public void InteractNpc()
    {
        myDialogue.CheckStateDialogue(15,state);
    }
}
