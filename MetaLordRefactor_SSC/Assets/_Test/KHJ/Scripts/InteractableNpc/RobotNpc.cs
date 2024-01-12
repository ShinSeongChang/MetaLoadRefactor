using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        // 로봇 33
        myDialogue.CheckStateDialogue(33, state);
    }
}