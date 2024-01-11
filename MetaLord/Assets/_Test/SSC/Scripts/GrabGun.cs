
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabGun : GunBase
{
    public static GrabGun instance;    
    protected override void Awake()
    {
        instance = this;
        base.Awake();
        brush.splatChannel = 2;
        //ammo = -55;
        mode = GunMode.Grab;
        excludedLayer = LayerMask.NameToLayer("Player");
    }

    public int GrabShot { get { return -ammo; } set { ammo = -value; } }

    int excludedLayer;
    GameObject targetObj = null;
    Rigidbody targetRigid = null;      
    List<Collider> colliders;
    string grabCancelText = "그랩취소";

    public override bool ShootGun()
    {
        // 언락전 우클릭 입력시 UI 텍스트 출력
        if(!state.UsedGrabGun())
        {
            if(state.CanFire)
            {
                state.CheckUnlockUi();       
                state.FadeOutCrossHair();
            }

            return false;
        }

        if(CheckCanFire() == false)
        {
            // onGrab 상태에서도 들고있는 물건 그랩 해제를 위해
            if (targetRigid || state.Ammo < -ammo)
            {
                CancelObj();
                return false;
            }

            state.CheckRangeCrossHair();
            return false;
        }

        return OneShotGrab();
    }

    bool OneShotGrab()
    {
        if (targetRigid || state.Ammo < - ammo)
        {            
            CancelObj();
            return false;
        }

        if (state.hit.transform == null)
        {
            return false;
        }
        // TODO : 임시야매로 그랩건의 오브젝트 그랩 남발 방지
        else if (state.hit.transform.gameObject.layer != LayerMask.NameToLayer("MovedObject") &&
            state.hit.transform.gameObject.layer != LayerMask.NameToLayer("GrabedObject") ||
            state.hit.transform.parent?.gameObject.layer == LayerMask.NameToLayer("NPC"))
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
            if (state.GetConnectObject() == targetRigid)
            {
                CancelObj();
                return;
            }

            //Vector3 wantValue = state.pickupPoint.position + (state.cameraController.GetGrabOffset(offset));
            //Vector3 up = Vector3.Cross(state.pickupPoint.right, state.pickupPoint.forward);
            //Vector3 right = Vector3.Cross(state.pickupPoint.up, state.pickupPoint.forward);
            //float distance = Vector3.Distance(target, pickup);

            //Vector3 target = targetRigid.position - targetRigid.position.y * Vector3.up;
            //Vector3 pickup = state.pickupPoint.position - state.pickupPoint.position.y * Vector3.up;

            state.cameraController.RotateSomethingAtCameraCenter(state.grabCorrectPoint);

            Vector3 dir = state.grabCorrectPoint.position -  targetRigid.position;
            float scala = dir.magnitude;
            scala = Mathf.Max(scala, state.speed);
            state.grabLine.enabled = true;
            state.grabLine.SetPosition(0, state.GunHolderHand.position);
            state.grabLine.SetPosition(1, state.pickupPoint.position);

           /* Debug.Log(dir.magnitude);
            if(targetRigid.CompareTag("ContactObject") )
            {
                RaycastHit hit;
                Physics.Raycast(targetRigid.position, dir, out hit, 100, myLayer);
                targetRigid.AddForce(dir + hit.normal *.5f);
            }          
            else*/ 
            if (dir.magnitude > .5f && dir.magnitude <50)
            {
                Vector3 power = dir * state.speed;
                if (power.magnitude > 100)
                    power = power.normalized * 100;
                targetRigid.velocity = Vector3.zero;
                targetRigid.AddForce(power, ForceMode.VelocityChange);
                //targetRigid.velocity = dir.normalized * scala;
                //targetRigid.rotation = state.grabCorrectPoint.rotation;
                //targetRigid.velocity += state.pickupPoint.forward * distance;
                //targetRigid.velocity =  up* -dir.y * 3 + right* dir.x * 3 + state.pickupPoint.forward * dir.z * 3;
            }
            else if(dir.magnitude <= .5f)
            {
                targetRigid.velocity = Vector3.zero;
                targetRigid.AddForce(dir.normalized);
                //targetRigid.position = state.grabCorrectPoint.position;
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

            if (targetObj.GetComponent<MovedObject>() != null)
            {
                targetObj.GetComponent<MovedObject>().CancelGrab();
            }

        }

        if (targetObj?.GetComponent<CatchObject>() != null)
        {
            targetObj.GetComponent<CatchObject>().CancelGrab();
        }

        //Debug.LogWarning(Physics.reuseCollisionCallbacks);

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

    /*public void CancleGrab()
    {
        targetObj.GetComponent<Collider>().material.dynamicFriction = 1f;
        targetObj = null;
        state.grabLine.enabled = false;
        targetRigid = null;
        state.onGrab = false;
       // state.isShootingState = false;
    }*/

    void FollowingObj( )
    {
        state.onGrab = true;
        targetObj = state.hit.transform.gameObject;        

        // 그랩 대상의 부모가 없다면
        if (targetObj.transform.parent == null)
        {            
            // 떨어지고 있는 오브젝트라면
            if(targetObj.GetComponent<Rigidbody>() == null)
            {                
                targetObj.GetComponent<MeshCollider>().convex = true;
                targetObj.AddComponent<Rigidbody>();
                targetObj.GetComponent<MovedObject>().ChangedState();
            }
            else
            {
                // 조합된 오브젝트라면
                if(targetObj.GetComponent<CatchObject>() != null)
                {
                    targetObj.GetComponent<CatchObject>().SetUpMesh();
                    targetObj.GetComponent<CatchObject>().ChangedState();
                }
                else
                {
                    targetObj.GetComponent<MeshCollider>().convex = true;
                    targetObj.GetComponent<MovedObject>().ChangedState();

                }

            }
        }
        // 그랩 대상의 부모가 있다면
        else
        {
            // 고정형 오브젝트 경우
            if (targetObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("Default"))
            {                
                // 고정형 부모 오브젝트에 클래스를 검사하여 그랩 취소
                if(targetObj.transform.parent.gameObject.GetComponent<CatchObject>() != null)
                {
                    state.onGrab = false;
                    return;
                }

                // 종속해제
                targetObj.transform.parent = null;
                targetObj.GetComponent<MeshCollider>().convex = true;
                targetObj.AddComponent<Rigidbody>();
                targetObj.GetComponent<MovedObject>().ChangedState();

            }
            // 상위 오브젝트 경우
            else if (targetObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("GrabedObject"))
            {                
                // 그랩 대상을 상위 오브젝트로 변경
                targetObj = targetObj.transform.parent.gameObject;
                CatchObject controll = targetObj.GetComponent<CatchObject>();
                controll.SetUpMesh();
                targetObj.AddComponent<Rigidbody>();
                targetRigid = targetObj.GetComponent<Rigidbody>();
                controll.ChangedState();
            }

        }



        // state.cameraController.SetGrabObject(targetObj.transform);

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
        //state.isShootingState = true;

        if (state.Ammo >= -ammo)
        {
            UsedAmmo(ammo);
        }

    }


    protected override bool CheckCanFire()
    {
        Vector3 start = state.checkPos.position;
        start.y -= 2f;
        Ray checkRay = new Ray(start, -(state.checkPos.up));
        RaycastHit hit;
        Physics.SphereCast(checkRay, 2f, out hit, 20f, myLayer);        

        if (!state.CanFire || hit.transform?.gameObject == state.hit.transform?.gameObject || !state.UsedGrabGun())
        {
            return false;        
        }

        return true;
    }

}
