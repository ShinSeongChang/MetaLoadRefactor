using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyBearNpc : NpcBase
{
    public NpcBase parentBear;
    private void Start()
    {
        parentBear = transform.parent.GetComponent<BearNpc>();   
    }
    public override void ChangedState(npcState _change)
    {
        base.ChangedState(_change);
        parentBear.ChangedState(_change);
    }

}
