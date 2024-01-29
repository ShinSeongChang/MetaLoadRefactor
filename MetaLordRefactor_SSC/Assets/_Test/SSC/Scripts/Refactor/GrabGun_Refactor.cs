using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Controller_Physics;

public class GrabGun_Refactor : GunBase
{
    public static GrabGun_Refactor instance;    
    public int GrabShot { get { return -ammo; } set { ammo = -value; } }

    int excludedLayer;
    GameObject targetObj = null;
    Rigidbody targetRigid = null;      
    List<Collider> colliders;
    string grabCancelText = "그랩취소";

    protected override void Awake()
    {
        base.Awake();

        instance = this;
        brush.splatChannel = 2;        
        mode = GunMode.Grab;
        excludedLayer = LayerMask.NameToLayer("Player");
        myLayer = 1 << LayerMask.GetMask("MovedObject", "GrabedObject");
    }


    public override bool ShootGun()
    {
        // 모드 해금전 사격 입력시 UI 출력
        if(!state.UsedGrabGun())
        {
            if(state.CanFire)
            {
                state.CheckUnlockUi();       
                state.FadeOutCrossHair();
            }

            return false;
        }

        return OneShotGrab();
    }

    bool OneShotGrab()
    {
        // 크로스헤어 변경
        state.CheckRangeCrossHair();

        // 그랩중인 상태일 때 사격 입력이 되었다면
        if (targetRigid || state.Ammo < - ammo)
        {            
            // 들고있는 그랩상태 해제
            CancelObj();
            return false;
        }

        // GunBase와 개별적인 레이어 체크
        if (!CheckLayer())
        {            
            return false;
        }

        FollowingObj();
        return true;
    }

    void FixedUpdate()
    {
        GrabObj();
    }

    public void GrabObj()
    {
        if (targetRigid)
        {            
            if (state.GetConnectObject() == targetRigid || state.GetOnClimbe())
            {
                CancelObj();
                return;
            }

            state.cameraController.RotateSomethingAtCameraCenter(state.grabCorrectPoint);

            Vector3 dir = state.grabCorrectPoint.position -  targetRigid.position;
            float scala = dir.magnitude;
            scala = Mathf.Max(scala, state.speed);
            state.grabLine.enabled = true;
            state.grabLine.SetPosition(0, state.GunHolderHand.position);
            state.grabLine.SetPosition(1, state.pickupPoint.position);

            if (dir.magnitude > .5f && dir.magnitude <50)
            {
                Vector3 power = dir * state.speed;
                if (power.magnitude > 100)
                    power = power.normalized * 100;
                targetRigid.velocity = Vector3.zero;
                targetRigid.AddForce(power, ForceMode.VelocityChange);

            }
            else if(dir.magnitude <= .5f)
            {
                targetRigid.velocity = Vector3.zero;
                targetRigid.AddForce(dir.normalized);
            }
            else 
            {
                state.CancelGrabText(grabCancelText);
                CancelObj();
            }
        }
    }


    public void CancelObj()
    {        
        if(targetRigid != null)
        {
            targetRigid.mass = 10;
            targetRigid.excludeLayers = 0;
            targetRigid.constraints = RigidbodyConstraints.None;
            targetRigid.useGravity = true;
            targetRigid.velocity = Vector3.down * 2f;
            targetRigid = null;            

        }

        if (targetObj?.GetComponent<MeshCollider>() != null)
        {
            targetObj.GetComponent<MeshCollider>().material.dynamicFriction = 1f;

            if (targetObj.GetComponent<MovedObject_Refactor>() != null)
            {
                targetObj.GetComponent<MovedObject_Refactor>().CancelGrab();
            }

        }

        if (targetObj?.GetComponent<CatchObject_Refactor>() != null)
        {
            targetObj.GetComponent<CatchObject_Refactor>().CancelGrab();
        }        

        state.cameraController.ClearGrabObject();
        if(colliders !=null && colliders.Count > 0)
        {
            foreach (var collider in colliders)
            {
                collider.material.bounceCombine = PhysicMaterialCombine.Average;
                collider.material.bounciness = 0.5f;
                collider.layerOverridePriority = 0;
            }
        }
        colliders = null;
        targetObj = null;
        state.grabLine.enabled = false;
        state.onGrab = false;        
    }

    void FollowingObj( )
    {
        state.onGrab = true;
        targetObj = state.hit.transform.gameObject;        

        // 단일 객체이면
        if(targetObj.GetComponent<MovedObject_Refactor>())
        {
            if(targetObj.GetComponentInParent<CatchObject_Refactor>())
            {
                targetObj = state.hit.transform.parent.gameObject;

                CatchObject_Refactor controll = targetObj.GetComponent<CatchObject_Refactor>();
                controll.ChangedState();
                controll.SetUpMesh();
            }
            else
            {
                targetObj.GetComponent<MovedObject_Refactor>().ChangedState();            
            }
        }
        // 조합된 오브젝트라면
        else
        {
            CatchObject_Refactor controll = targetObj.GetComponent<CatchObject_Refactor>();
            controll.ChangedState();            
            controll.SetUpMesh();
        }

        targetRigid = targetObj.GetComponent<Rigidbody>();
        colliders = targetRigid.GetComponentsInChildren<Collider>().ToList();

        if (colliders != null && colliders.Count > 0)
        {
            foreach (var collider in colliders)
            {
                collider.material.bounceCombine = PhysicMaterialCombine.Minimum;
                collider.material.bounciness = 0;
                collider.layerOverridePriority = -1;
            }

        }

        state.pickupPoint.position = state.hit.point;
        state.cameraController.SetGrabObject(targetObj.transform);
        state.grabCorrectPoint.position = targetRigid.position;
        
        targetRigid.excludeLayers &= ~(1 << excludedLayer);
        targetRigid.constraints = RigidbodyConstraints.FreezeRotation;
        targetRigid.useGravity = false;
        targetRigid.mass *= 2;        

        if (state.Ammo >= -ammo)
        {
            UsedAmmo(ammo);
        }

    }


    protected override bool CheckCanFire()
    {
        // PC 발아래 체크 => 내 발 아래있는 오브젝트는 Grab 안함
        Vector3 start = state.checkPos.position;
        start.y -= 2f;
        Ray checkRay = new Ray(start, -(state.checkPos.up));
        RaycastHit hit;
        Physics.SphereCast(checkRay, 2f, out hit, 20f, myLayer);                

        if (!state.CanFire || hit.transform?.gameObject == state.hit.transform?.gameObject)
        {
            return false;        
        }

        return true;
    }

    bool CheckLayer()
    {
        if (state.hit.transform?.gameObject.layer == LayerMask.NameToLayer("MovedObject") ||
            state.hit.transform?.gameObject.layer == LayerMask.NameToLayer("GrabedObject"))
        {            
            return true;
        }

        return false;
    }

}
