using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondGun : GunBase
{
    protected override void Awake()
    {
        base.Awake();
        brush.splatChannel = 1;
        brush.splatScale = state.BondSize;
        ammo = -60;
        mode = GunMode.Bond;
    }

    public override bool ShootGun()
    {
        if (CheckCanFire() == false)
        {
            return false;
        }

        return ShootBond();
    }

    public bool ShootBond()
    {
        if (state.checkSuccessRay)
        {
            Ray muzzleRay = new Ray(state.startPoint, state.hit.point - state.startPoint);
/*
            if (state.minDistance == true)
            {
                muzzleRay = new Ray(state.checkPos.position, state.hit.point - state.startPoint);
                UsedAmmo(muzzleRay, ammo);
                                
                return;
            }*/

            UsedAmmo(muzzleRay, ammo);            
            return true;
        }
        return false;
    }
}
