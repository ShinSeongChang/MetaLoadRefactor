using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CatchObject : MonoBehaviour
{
    Rigidbody myRigid;
    HashSet<MeshCollider> childColid = new HashSet<MeshCollider>();
    LayerMask layerMask;    
    NpcBase targetNpc;
    GunStateController state;

    float ySpeed = default;
    float contactTime = 0f;
    float decrementGravity = 0.25f;
    float decrement = 0.5f;
    float maxGravity = 30f;
    int checkCount = 0;
    bool isSleep = false;    
    bool checkContact;

    string contactTag = "ContactObject";
    string unContactTag = "Untagged";

    private void Awake()
    {
        checkContact = false;        
        layerMask = 1 << LayerMask.NameToLayer("Default") |
        1 << LayerMask.NameToLayer("NPC") |
        1 << LayerMask.NameToLayer("StaticObject") |
        1 << LayerMask.NameToLayer("MovedObject") |
        1 << LayerMask.NameToLayer("GrabedObject");
    }

    // 충돌이 일어난 이동형 오브젝트 MeshCollider 갱신
    public void AddChild(MeshCollider _child)
    {
        childColid.Add(_child);        
    }

    public void SetUpMesh()
    {
        foreach (MeshCollider col in childColid)
        {            
            col.convex = true;
        }
    }

    private void Update()
    {
        // { TODO : 개인 리팩토링
        #region CustomFallingObject

            //// 내 리지드바디가 존재하고, 그랩한 대상이 아닐 때
            if (myRigid && !checkContact)
            {
                if (myRigid.velocity.magnitude <= 0.5f && !isSleep)
                {
                    SleepObj();
                    return;
                }

                // 충돌시간이 일정값 이하면 (공중에 있는 상태면)
                if (contactTime <= 2f && !isSleep)
                {
                    // 임의의 중력가속도 적용
                    Vector3 tempVelocity = myRigid.velocity;
                    ySpeed -= Time.deltaTime * decrementGravity;
                    tempVelocity.y += ySpeed;

                    if (tempVelocity.y >= maxGravity)
                    {
                        tempVelocity.y = maxGravity;
                    }

                    myRigid.velocity = tempVelocity;
                }
            }
        //if(state.usedGravity)
        //{

        //}
        //else
        //{
        //    // 그랩한 대상의 슬립 기준
        //    if (myRigid)
        //    {
        //        if (myRigid.IsSleeping())
        //        {
        //            Destroy(myRigid);

        //            foreach (MeshCollider col in childColid)
        //            {
        //                col.convex = false;
        //            }

        //            checkContact = false;
        //        }
        //    }

        //}

        #endregion
        // } TODO : 개인 리팩토링

    }

    //private void FixedUpdate()
    //{
    //    if (checkContact)
    //    {
    //        gameObject.tag = unContactTag;
    //    }
    //}

    private void OnTriggerEnter(Collider collision)
    {
        // 이미 본드 동작을 하는 오브젝트를 다시 그랩하면 그랩하는순간 충돌면을 체크하여 그랩 해제됨에 따라 상태를 제어할 bool값 추가
        if(checkContact == false || collision.gameObject.layer == LayerMask.NameToLayer("CatchObject") || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {            
            return;
        }        

        // 충돌한 오브젝트의 부모가 없을경우
        if (collision.transform.parent == null)
        {
            // 충돌한 오브젝트를 내 하위에 종속
            collision.transform.parent = transform;

            // 자식 메쉬콜라이더 갱신
            AddChild(collision.transform.GetComponent<MeshCollider>());
            collision.transform.GetComponent<MeshCollider>().convex = true;

            checkContact = false;
            GrabGun.instance.CancelObj();
        }
        // 부모가 존재할 경우
        else
        {
            // 지정 오브젝트는 충돌한 오브젝트의 부모
            GameObject contactObj = collision.transform.parent.gameObject;

            // 부모가 상위 오브젝트 경우
            if (contactObj.gameObject.layer == LayerMask.NameToLayer("GrabedObject"))
            {
                CareeToContact(contactObj);

                checkContact = false;
                GrabGun.instance.CancelObj();
            }
            // 부모가 고정형 오브젝트 경우
            else if (contactObj.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                // 합쳐진 상위 오브젝트일 경우
                if (contactObj.transform.GetComponent<CatchObject>() != null)
                {
                    CareeToContact(contactObj);

                    checkContact = false;
                    GrabGun.instance.CancelObj();
                }
                // 엔피씨일 경우
                else if (collision.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
                {
                    collision.transform.GetComponent<NpcBase>().ChangedState(npcState.objectAttached);
                    GunStateController.AddList(collision.transform.GetComponent<NpcBase>());

                    for (int j = 0; j < transform.childCount; j++)
                    {
                        if(transform.GetChild(j).GetComponent<MovedObject>() == null)
                        {
                            continue;
                        }

                        transform.GetChild(j).GetComponent<MeshCollider>().convex = false;
                        transform.GetChild(j).gameObject.layer = LayerMask.NameToLayer("NPC");
                    }

                    Destroy(myRigid);

                    checkContact = false;
                    GrabGun.instance.CancelObj();
                }
                else if(collision.transform.gameObject.layer == LayerMask.NameToLayer("MovedObject"))
                {
                    Destroy(myRigid);

                    checkContact = false;
                    GrabGun.instance.CancelObj();

                    collision.transform.parent = transform;
                    AddChild(collision.transform.GetComponent<MeshCollider>());
                    //collision.transform.GetComponent<MeshCollider>().convex = true;
                }
                // 단순 고정형 오브젝트일경우
                else
                {
                    // TODO : 스태틱 오브젝트와 어떤 연계를 할 것인지                        
                    transform.gameObject.layer = LayerMask.NameToLayer("Default");

                    for (int j = 0; j < transform.childCount; j++)
                    {
                        if (transform.GetChild(j).GetComponent<MovedObject>() == null)
                        {
                            continue;
                        }

                        transform.GetChild(j).GetComponent<MeshCollider>().convex = false;
                    }

                    Destroy(myRigid);

                    checkContact = false;
                    GrabGun.instance.CancelObj();
                }
            }
            // 아기곰의 경우
            else if (collision.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                collision.transform.GetComponent<NpcBase>().ChangedState(npcState.objectAttached);

                for (int j = 0; j < transform.childCount; j++)
                {
                    if (transform.GetChild(j).GetComponent<MovedObject>() == null)
                    {
                        continue;
                    }

                    transform.GetChild(j).GetComponent<MeshCollider>().convex = false;
                }

                transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                Destroy(myRigid);

                checkContact = false;
                GrabGun.instance.CancelObj();
            }
            // 엔피씨에 붙은 조합형 일 경우
            else if (collision.transform.gameObject.layer == LayerMask.NameToLayer("MovedObject") &&
                    collision.transform.parent?.GetComponent<CatchObject>() != null)
            {
                CareeToContact(contactObj);

                checkContact = false;
                GrabGun.instance.CancelObj();
            }

        }

        //checkContact = false;
        //GrabGun.instance.CancelObj();
    }


    //// 그랩한 물건이 이동형 오브젝트와 부딪힐때마다 물리력 행사 콜백

    private void OnCollisionEnter(Collision collision)
    {
        //if(collision.transform.parent == transform)
        //{            
        //    return;
        //}

        if (checkContact)
        {
            gameObject.tag = contactTag;
        }

        if (collision.transform.parent?.GetComponent<CatchObject>() != null)
        {
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("MovedObject"))
        {
            if (collision.gameObject.GetComponent<PaintTarget>().CheckPainted() ||
                isSleep)
            {
                return;
            }
           

            //if (collision.gameObject.transform.parent?.GetComponent<CatchObject>() != null && checkContact)
            //{
            //    // 고정형, NPC에 붙은 조합오브젝트가 아니라면 물리력 부여
            //    if (collision.transform.parent?.gameObject.layer == LayerMask.NameToLayer("Default") ||
            //        collision.transform.parent?.gameObject.layer == LayerMask.NameToLayer("NPC"))
            //    {
            //        return;
            //    }

            //    collision.gameObject?.transform.parent.GetComponent<CatchObject>().InitOverap();
            //}

            if (collision.gameObject.layer == LayerMask.NameToLayer("MovedObject") && checkContact)
            {
                // 조합 오브젝트일시에는 안막아두면 조합된 오브젝트에서 해당 오브젝트만 물리영향을 받는 버그 발생
                if (collision.transform.parent?.gameObject.layer == LayerMask.NameToLayer("GrabedObject"))
                {
                    return;
                }

                if (collision.gameObject.transform.parent?.GetComponent<CatchObject>() == null)
                {
                    if (collision.gameObject.transform.parent?.GetComponent<CatchObject>() == null)
                    {
                        if (checkContact)
                        {                            
                            Vector3 force = -(collision.contacts[0].normal * 2f);
                            collision.gameObject.GetComponent<MovedObject>().InitOverap(force);
                        }
                        else
                        {                            
                            collision.gameObject.GetComponent<MovedObject>().InitOverap();
                        }

                    }
                }                
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {        
        // 그랩한 오브젝트가 플레이어 닿을시 임시 캔슬처리
        if (checkContact && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GrabGun.instance.CancelObj();
        }

        // { TODO : 개인 리팩토링
        #region CustomFallingObject
        //// 그랩한 MovedObject가 아니면 충돌 포인트 체크
        if (!checkContact && myRigid)
        {
            // { 이 구간은 1프레임에 벌어진 모든 충돌지점을 검사하는 것
            // 충돌지점을 모두 검사
            for (int i = 0; i < collision.contactCount; i++)
            {
                //지점중 하단에서 발생한 충돌을 검사한다.
                if (-(collision.contacts[i].normal.y) <= -0.95f)
                {
                    //유효 충돌체크 이후 반복문 종료
                    checkCount++;
                    break;
                }

            }
            // } 이 구간은 1프레임에 벌어진 모든 충돌지점을 검사하는 것

            ////일정 충돌 시간을 넘었을시 Or 일정속도 이하가 된다면 Sleep 코루틴 시전
            //if (contactTime >= 5f && !isSleep)
            //{                
            //    SleepObj();

            //    //TODO: 일정시간 이후에 슬립하는것이 자연스러워 보이는데 현재 코루틴과 그랩 사이 예외사항 처리가 안되어 주석처리
            //    //sleepCoroutine = StartCoroutine(EnforceSleep);
            //    return;
            //}

            //유효충돌이 60프레임 이상 벌어졌다면(1초 ? )
            if (checkCount >= 60)
            {
                Vector3 tempVelocity = new Vector3(myRigid.velocity.x * decrement, myRigid.velocity.y * decrement, myRigid.velocity.z * decrement);
                myRigid.velocity = tempVelocity;
                //체크 카운트 초기화, 정지값 체크 증가
                checkCount = 0;
                contactTime += 1f;
                return;
            }
        }
        #endregion
        // } TODO : 개인 리팩토링

        // 이미 본드 동작을 하는 오브젝트를 다시 그랩하면 그랩하는순간 충돌면을 체크하여 그랩 해제됨에 따라 상태를 제어할 bool값 추가
        if (checkContact == false || collision.gameObject.layer == LayerMask.NameToLayer("CatchObject"))
        {
            //Debug.Log("CatchObject 막히는 중");
            return;
        }

        if (checkContact)
        {
            gameObject.tag = contactTag;
        }

        // 충돌이 일어나는 지점을 모두 체크
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 dir = collision.contacts[i].normal;

            Ray ray = new Ray(collision.contacts[i].point + dir, -dir);                               

            if (PaintTarget.RayChannel(ray, 1.5f, layerMask) == 0 && collision.gameObject.GetComponent<Controller_Physics>() == null && checkContact)
            {                                
                // 충돌한 오브젝트의 부모가 없을경우
                if (collision.transform.parent == null)
                {                                        
                    // 충돌한 오브젝트를 내 하위에 종속
                    collision.transform.parent = transform;     
                    
                    // 자식 메쉬콜라이더 갱신
                    AddChild(collision.transform.GetComponent<MeshCollider>());
                    collision.transform.GetComponent<MeshCollider>().convex = true;
                }
                // 부모가 존재할 경우
                else
                {
                    // 지정 오브젝트는 충돌한 오브젝트의 부모
                    GameObject contactObj = collision.transform.parent.gameObject;                    

                    // 부모가 상위 오브젝트 경우
                    if (contactObj.gameObject.layer == LayerMask.NameToLayer("GrabedObject"))
                    {                                                                    
                        CareeToContact(contactObj);
                    }
                    // 부모가 고정형 오브젝트 경우
                    else if (contactObj.gameObject.layer == LayerMask.NameToLayer("Default"))
                    {                        
                        // 합쳐진 상위 오브젝트일 경우
                        if(contactObj.transform.GetComponent<CatchObject>() != null)
                        {                            
                            CareeToContact(contactObj);
                        }
                        // 엔피씨일 경우
                        else if (collision.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
                        {                            
                            collision.transform.GetComponent<NpcBase>().ChangedState(npcState.objectAttached);
                            collision.transform.GetComponent<NpcBase>();


                            for (int j = 0; j < transform.childCount; j++)
                            {
                                if (transform.GetChild(j).GetComponent<MovedObject>() == null)
                                {
                                    continue;
                                }

                                transform.GetChild(j).GetComponent<MeshCollider>().convex = false;
                                transform.GetChild(j).gameObject.layer = LayerMask.NameToLayer("NPC");
                            }

                            Destroy(myRigid);
                        }
                        // 단순 고정형 오브젝트일경우
                        else
                        {
                            // TODO : 스태틱 오브젝트와 어떤 연계를 할 것인지                        
                            transform.gameObject.layer = LayerMask.NameToLayer("Default");

                            for (int j = 0; j < transform.childCount; j++)
                            {
                                if (transform.GetChild(j).GetComponent<MovedObject>() == null)
                                {
                                    continue;
                                }

                                transform.GetChild(j).GetComponent<MeshCollider>().convex = false;
                            }

                            Destroy(myRigid);
                        }
                    }
                    // 아기곰의 경우
                    else if (collision.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
                    {
                        collision.transform.GetComponent<NpcBase>().ChangedState(npcState.objectAttached);

                        for (int j = 0; j < transform.childCount; j++)
                        {
                            if (transform.GetChild(j).GetComponent<MovedObject>() == null)
                            {
                                continue;
                            }

                            transform.GetChild(j).GetComponent<MeshCollider>().convex = false;
                        }

                        transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                        Destroy(myRigid);
                    }
                    // 엔피씨에 붙은 조합형 일 경우
                    else if (collision.transform.gameObject.layer == LayerMask.NameToLayer("MovedObject") &&
                            collision.transform.parent?.GetComponent<CatchObject>() != null)
                    {
                        CareeToContact(contactObj);
                    }

                }

                checkContact = false;                                
                GrabGun.instance.CancelObj();
            }
        }
    }

    public void ChangedState()
    {
        myRigid = GetComponent<Rigidbody>();
        myRigid.mass = 10f;
        checkContact = true;
    }
    void CareeToContact(GameObject contactObj)
    {
        GameObject[] myChild = new GameObject[transform.childCount];

        // 내 자식만큼 오브젝트 캐싱
        for (int i = 0; i < myChild.Length; i++)
        {            
            myChild[i] = transform.GetChild(i).gameObject;
        }

        if(myChild.Length > 0)
        {
            // 오브젝트 부모 변경 및 해쉬 갱신
            for (int i = 0; i < myChild.Length; i++)
            {
                myChild[i].transform.parent = contactObj.transform;

                // Trigger Object Pass
                if (myChild[i].GetComponent<MovedObject>() == null)
                {
                    continue;
                }                

                myChild[i].GetComponent<MeshCollider>().convex = false;

                // 충돌한 오브젝트의 상위 오브젝트 해쉬 갱신
                contactObj.transform.GetComponent<CatchObject>().AddChild(myChild[i].transform.GetComponent<MeshCollider>());            
            }

        }

        // 컨트롤러에 담겨있는 Hash중 나 자신을 제거 후 오브젝트채로 파괴
        GunStateController.catchList.Remove(this);
        if (!gameObject.GetComponent<PaintTarget>())
            Destroy(gameObject);
            
    }

    public void CancelGrab()
    {
        checkContact = false;
        ySpeed = 0;
    }

    void SleepObj()
    {        
        checkContact = false;
        myRigid.velocity = Vector3.zero;
        Destroy(myRigid);

        foreach (MeshCollider col in childColid)
        {
            col.convex = false;
        }

        isSleep = false;
        contactTime = 0f;
        ySpeed = 0f;
        gameObject.tag = unContactTag;

        Invoke("ClearTime", 1.5f);
    }

    void ClearTime()
    {
        isSleep = false;
    }

    public void InitOverap()
    {
        foreach (MeshCollider col in childColid)
        {
            col.material.dynamicFriction = 0.6f;
            col.material.bounciness = 0.5f;
            col.convex = true;
        }

        if (!myRigid)
        {
            transform.AddComponent<Rigidbody>();
            myRigid = GetComponent<Rigidbody>();
            myRigid.mass = 1000f;
        }

    }

}
