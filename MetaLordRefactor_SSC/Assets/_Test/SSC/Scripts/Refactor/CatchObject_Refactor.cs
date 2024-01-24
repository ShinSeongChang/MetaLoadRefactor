using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class CatchObject_Refactor : MovedObject_Refactor
{    
    HashSet<MeshCollider> childColid = new HashSet<MeshCollider>();            

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

    protected override void OnTriggerEnter(Collider collision)
    {
        // layer 체크용으로 Trigger되는 오브젝트 캐싱
        contactObj = collision.gameObject;

        // 트리거를 체크하면 안되는 상황
        if (!CanContact)
        {
            return;
        }

        // 충돌한 오브젝트 캐싱
        GameObject collisionObj = collision.gameObject;

        // 부딪힌 오브젝트의 Combine 상태를 체크한다.
        // 사실상 여기는 Pass 구간? => 떨어지고 있는 GrabedObject에 갖다 댄다면
        if (collisionObj.GetComponent<CatchObject_Refactor>())
        {
            combineObj = collisionObj.GetComponent<CatchObject_Refactor>();

            combineObj.SetHash(combineObj, ToolFunc<MeshCollider>.ReturnToArray(childColid));

            CareeToContact(contactObj.transform.parent.gameObject);
 
        }
        // 충돌한 오브젝트에 CatchObject가 없다면 부모 오브젝트의 Combine 상태를 체크한다.
        else
        {
            // Combine 된 상태라면
            if (collisionObj.GetComponentInParent<CatchObject_Refactor>())
            {
                combineObj = collisionObj.GetComponentInParent<CatchObject_Refactor>();

                combineObj.SetHash(combineObj, ToolFunc<MeshCollider>.ReturnToArray(childColid));

                CareeToContact(contactObj.transform.parent.gameObject);
            }
            // Combine 안된 상태라면 => 고정형 오브젝트, NPC, 그랩이후 해제된 MovedObject들
            else
            {
                combineObj = this;

                // 고정형과 부딪혔을 때
                if (collisionObj.layer == LayerMask.NameToLayer(defaultLayer))
                {
                    gameObject.layer = LayerMask.NameToLayer(defaultLayer);
                }
                // NPC와 부딪혔을 때
                else if (collisionObj.layer == LayerMask.NameToLayer(npcLayer))
                {
                    gameObject.layer = LayerMask.NameToLayer(npcLayer);
                    targetNpc = collisionObj.GetComponent<NpcBase>();
                    targetNpc.ChangedState(npcState.objectAttached);
                }
                // 이동형과 부딪혔을 때
                else if (collisionObj.layer == LayerMask.NameToLayer(movedLayer))
                {
                    collisionObj.transform.SetParent(transform);
                    SetHash(combineObj, collisionObj.GetComponent<MeshCollider>());
                }

                SleepObj();

            }

        }

        ClearState();
        GrabGun_Refactor.instance.CancelObj();
    }


    //// 그랩한 물건이 이동형 오브젝트와 부딪힐때마다 물리력 행사 콜백

    protected override void OnCollisionEnter(Collision collision)
    {
        if (checkContact)
        {
            gameObject.tag = contactTag;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("MovedObject"))
        {
            if (collision.gameObject.GetComponent<PaintTarget>().CheckPainted() ||
                isSleep)
            {
                return;
            }   
            
            // Combine된 오브젝트면 Combine에 물리력 부여           
            if(collision.transform.GetComponentInParent<CatchObject_Refactor>())
            {
                if (checkContact)
                {
                    Vector3 force = -(collision.contacts[0].normal * 2f);
                    collision.gameObject.GetComponentInParent<CatchObject_Refactor>().InitOverap(force);
                }
                else
                {
                    collision.gameObject.GetComponentInParent<CatchObject_Refactor>().InitOverap();
                }
            }
            // 단일 오브젝트면 단일 오브젝트에 물리력 부여           
            else
            {
                if (checkContact)
                {
                    Vector3 force = -(collision.contacts[0].normal * 200f);
                    collision.gameObject.GetComponent<MovedObject_Refactor>().InitOverap(force);
                }
                else
                {                    
                    collision.gameObject.GetComponent<MovedObject_Refactor>().InitOverap();
                }
            }
        }
    }

    protected override void OnCollisionStay(Collision collision)
    {        
        contactObj = collision.gameObject;

        // 그랩한 오브젝트가 플레이어 닿을시 임시 캔슬처리
        if (checkContact && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GrabGun.instance.CancelObj();
        }

        // 그랩한 MovedObject가 아니면 충돌 포인트 체크
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

            //유효충돌이 60프레임 이상 벌어졌다면(1초?)
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

        if (checkContact)
        {
            gameObject.tag = contactTag;
        }

        // 이미 본드 동작을 하는 오브젝트를 다시 그랩하면 그랩하는순간 충돌면을 체크하여 그랩 해제됨에 따라 상태를 제어할 bool값 추가
        if (!CanContact)
        {            
            return;
        }

        // 충돌이 일어나는 지점을 모두 체크
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 dir = collision.contacts[i].normal;

            Ray ray = new Ray(collision.contacts[i].point + dir, -dir);

            GameObject collisionObj = collision.gameObject;
            Collider collider = collisionObj.GetComponent<Collider>();

            if (PaintTarget.RayChannel(ray, 1.5f, layerMask) == 0 && collision.gameObject.GetComponent<Controller_Physics>() == null && checkContact)
            {
                // 부딪힌 오브젝트의 Combine 상태를 체크한다.
                // 사실상 여기는 Pass 구간? => 떨어지고 있는 GrabedObject에 갖다 댄다면
                if (collisionObj.GetComponent<CatchObject_Refactor>())
                {
                    combineObj = collisionObj.GetComponent<CatchObject_Refactor>();
                    combineObj.SetHash(combineObj, ToolFunc<MeshCollider>.ReturnToArray(childColid));
                    CareeToContact(contactObj.transform.parent.gameObject);
                }
                // 충돌한 오브젝트에 CatchObject가 없다면 부모 오브젝트의 Combine 상태를 체크한다.
                else
                {                     
                    // Combine 된 상태라면
                    if (collisionObj.GetComponentInParent<CatchObject_Refactor>())
                    {
                        combineObj = collisionObj.GetComponentInParent<CatchObject_Refactor>();
                        combineObj.SetHash(combineObj, ToolFunc<MeshCollider>.ReturnToArray(childColid));
                        CareeToContact(contactObj.transform.parent.gameObject);

                    }
                    // Combine 안된 상태라면 => 고정형 오브젝트, NPC, 그랩이후 해제된 MovedObject들
                    else
                    {
                        combineObj = this;

                        // 고정형과 부딪혔을 때
                        if (collisionObj.layer == LayerMask.NameToLayer(defaultLayer))
                        {
                            transform.gameObject.layer = LayerMask.NameToLayer(defaultLayer);
                        }
                        // NPC와 부딪혔을 때
                        else if (collisionObj.layer == LayerMask.NameToLayer(npcLayer))
                        {
                            transform.gameObject.layer = LayerMask.NameToLayer(npcLayer);
                            targetNpc = collisionObj.GetComponent<NpcBase>();
                            targetNpc.ChangedState(npcState.objectAttached);
                        }
                        // 이동형과 부딪혔을 때
                        else if (collisionObj.layer == LayerMask.NameToLayer(movedLayer))
                        {
                            collisionObj.transform.SetParent(transform);
                            SetHash(combineObj, collisionObj.GetComponent<MeshCollider>());
                        }

                        SleepObj();
                    }

                }

                ClearState();
                GrabGun_Refactor.instance.CancelObj();
            }
        }
    }

    public override void ChangedState()
    {
        myRigid = transform.AddComponent<Rigidbody>();
        myRigid = GetComponent<Rigidbody>();
        myRigid.mass = 10f;        
        checkCount = 0;
        checkContact = true;

        foreach (MeshCollider col in childColid)
        {
            col.material.dynamicFriction = 0f;
            col.material.bounciness = 0f;
        }

    }

    /// <summary>
    /// 내 자식 오브젝트를 충돌한 조합형 오브젝트에 모두 옮겨주는 메소드
    /// </summary>
    /// <param name="contactObj">충돌한 상위 오브젝트</param>
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
                //myChild[i].transform.SetParent(contactObj.transform);

                // Trigger Object Pass
                if (!myChild[i].GetComponent<MovedObject_Refactor>())
                {
                    continue;
                }                

                myChild[i].GetComponent<MeshCollider>().convex = false;         
            }

        }

        // 컨트롤러에 담겨있는 Hash중 나 자신을 제거 후 오브젝트채로 파괴
        GunStateController.catchList.Remove(this);
        if (!gameObject.GetComponent<PaintTarget>())
            Destroy(gameObject);
            
    }

    protected override void SleepObj()
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

    public override void InitOverap()
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
            myRigid.mass = 10f;
        }

    }

    protected override void StartCaching()
    {
        // 충돌감지 Bool 값 false 설정으로 그랩한 오브젝트만 변환
        checkContact = false;

        // 레이어 마스크 설정
        layerMask = LayerMask.GetMask(defaultLayer, npcLayer, "StaticObject", movedLayer, catchLayer);
    }

    protected override void ClearState()
    {
        if (myRigid != null)
        {
            Destroy(myRigid);
        }

        checkContact = false;

        foreach(var obj in childColid)
        {
            obj.convex = false;
        }
    }

    

}
