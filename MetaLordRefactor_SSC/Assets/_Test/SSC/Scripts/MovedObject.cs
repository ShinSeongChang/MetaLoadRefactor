using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class MovedObject : MonoBehaviour
{
    public LayerMask layerMask;
    Rigidbody myRigid;
    MeshCollider myColid;    
    NpcBase targetNpc;
    SphereCollider[] triigerCollider;
    GunStateController state;

    Vector3 originPos;
    Quaternion originRot;
    
    // Json에 저장해야할 목록 3가지
    Vector3 savePos;
    Quaternion saveRot;   
    bool isMoved;
    Vector3 saveVelocity;


    float ySpeed = default;    
    float contactTime = 0f;
    float decrementGravity = 0.5f;
    float decrement = 0.5f;
    float maxGravity = 30f;
    int checkCount = 0;
    bool isSleep = false;
    bool checkContact = false;
    bool isConstact = false;

    string contactTag = "ContactObject";
    string unContactTag = "Untagged";

    Coroutine sleepCoroutine;

    static int test = 0;
    private void Awake()
    {        
        checkContact = false;
        myColid = GetComponent<MeshCollider>();

        layerMask = 1 << LayerMask.NameToLayer("Default") |
        1 << LayerMask.NameToLayer("NPC") |
        1 << LayerMask.NameToLayer("StaticObject") |
        1 << LayerMask.NameToLayer("MovedObject") |
        1 << LayerMask.NameToLayer("GrabedObject");
    }

    void Update()
    {
            // 내 리지드바디가 존재하고, 그랩한 대상이 아닐 때 (그랩한 대상은 낙하속도 X)
            if (myRigid && !checkContact)
            {                
                if( myRigid.velocity.magnitude <= 0.5f && !isSleep)//(contactTime >= 3)
                {
                    SleepObj();
                    return;
                }

                // 충돌시간이 일정값 이하면 (공중에 있는 상태면)
                if (contactTime < 3f && !isSleep)
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
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (checkContact == false || transform.parent != null || collision.gameObject.layer == LayerMask.NameToLayer("CatchObject") || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {            
            return;
        }        

        GameObject parentObj = null;            

        // 충돌한 오브젝트의 부모가 없을경우
        if (collision.transform.parent == null)
        {

            // 상위 오브젝트 생성
            parentObj = new GameObject("test"+test);
            parentObj.transform.gameObject.layer = LayerMask.NameToLayer("GrabedObject");
            CatchObject controll = parentObj.AddComponent<CatchObject>();
            GunStateController.AddList(controll);
            parentObj.transform.position = collision.ClosestPoint(transform.position);

            // 그랩한 오브젝트와 충돌한 오브젝트 모두 상위 오브젝트 종속, HashSet 갱신                    
            transform.parent = parentObj.transform;
            collision.transform.parent = parentObj.transform;
            //GunStateController.AddList(collision.gameObject.GetComponent<MovedObject>());
            controll.AddChild(transform.GetComponent<MeshCollider>());
            controll.AddChild(collision.transform.GetComponent<MeshCollider>());

            ClearState();
            GrabGun.instance.CancelObj();
        }
        // 부모가 존재할 경우
        else
        {
            GameObject contactObj = collision.gameObject;

            // TODO : 레이어 체크가 아닌 CatchObject 스크립트로 여부 확인
            // 부모가 상위 오브젝트 경우
            if (contactObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("GrabedObject"))
            {
                // HashSet 갱신    
                transform.parent = contactObj.transform.parent;
                contactObj.transform.parent.GetComponent<CatchObject>().AddChild(transform.GetComponent<MeshCollider>());

                ClearState();
                GrabGun.instance.CancelObj();

            }
            // 부모가 고정형 오브젝트 경우
            else if (contactObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                // 부딪힌 오브젝트도 고정형이면
                if (contactObj.transform.gameObject.layer == LayerMask.NameToLayer("Default"))
                {
                    // 클래스 여부로 기존 고정형 오브젝트인지 합쳐진 오브젝트인지
                    if (contactObj.transform.parent.GetComponent<CatchObject>())
                    {
                        transform.SetParent(contactObj.transform.parent);

                        ClearState();
                        GrabGun.instance.CancelObj();
                    }
                    else
                    {
                        // 상위 오브젝트 생성
                        parentObj = new GameObject("test" + test++);
                        parentObj.transform.gameObject.layer = LayerMask.NameToLayer("Default");
                        CatchObject controll = parentObj.AddComponent<CatchObject>();
                        GunStateController.AddList(controll);
                        parentObj.transform.position = collision.ClosestPoint(transform.position);

                        // 그랩한 오브젝트 상위 고정형 오브젝트 종속, HashSet 갱신    
                        transform.parent = parentObj.transform;
                        controll.AddChild(transform.GetComponent<MeshCollider>());

                        ClearState();
                        GrabGun.instance.CancelObj();

                    }
                }
                else if (contactObj.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
                {                    
                    parentObj = new GameObject("test" + test++);
                    parentObj.transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                    //transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                    CatchObject controll = parentObj.AddComponent<CatchObject>();
                    GunStateController.AddList(controll);                    
                    parentObj.transform.position = collision.ClosestPoint(transform.position);

                    // 그랩한 오브젝트 상위 오브젝트 종속, HashSet 갱신    
                    transform.parent = parentObj.transform;
                    controll.AddChild(transform.GetComponent<MeshCollider>());                    
                    targetNpc = collision.transform.GetComponent<NpcBase>();
                    GunStateController.AddList(targetNpc);                    
                    targetNpc.ChangedState(npcState.objectAttached);

                    ClearState();
                    GrabGun.instance.CancelObj();
                }
                else
                {
                    // 충돌한 이동형 오브젝트가 고정형에 집합된 형태라면 
                    if (contactObj.transform.parent.GetComponent<CatchObject>() != null &&
                        contactObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("Default"))
                    {
                        //Debug.Log("===");
                        transform.parent = contactObj.transform.parent;
                        checkContact = false;
                        ClearState();
                        GrabGun.instance.CancelObj();

                        ClearState();
                        GrabGun.instance.CancelObj();
                        return;
                    }
                    // 상위 오브젝트 생성
                    parentObj = new GameObject("test" + test++);
                    parentObj.transform.gameObject.layer = LayerMask.NameToLayer("GrabedObject");
                    CatchObject controll = parentObj.AddComponent<CatchObject>();
                    GunStateController.AddList(controll);
                    parentObj.transform.position = collision.ClosestPoint(transform.position);

                    // 그랩한 오브젝트와 충돌한 오브젝트 모두 상위 오브젝트 종속, HashSet 갱신    
                    transform.parent = parentObj.transform;
                    collision.transform.parent = parentObj.transform;
                    controll.AddChild(transform.GetComponent<MeshCollider>());
                    controll.AddChild(collision.transform.GetComponent<MeshCollider>());

                    ClearState();
                    GrabGun.instance.CancelObj();

                }
            }
            // 아기곰의 경우
            else if (contactObj.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                parentObj = new GameObject();
                parentObj.transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                //transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                CatchObject controll = parentObj.AddComponent<CatchObject>();
                GunStateController.AddList(controll);
                parentObj.transform.position = collision.ClosestPoint(transform.position);

                // 그랩한 오브젝트 상위 오브젝트 종속, HashSet 갱신    
                transform.parent = parentObj.transform;
                controll.AddChild(transform.GetComponent<MeshCollider>());
                targetNpc = collision.transform.GetComponent<NpcBase>();
                targetNpc.ChangedState(npcState.objectAttached);

                ClearState();
                GrabGun.instance.CancelObj();
            }
            // 엔피씨에 붙은 조합형 일 경우
            else if (contactObj.transform.gameObject.layer == LayerMask.NameToLayer("MovedObject") &&
                contactObj.transform.parent?.GetComponent<CatchObject>() != null)
            {
                CatchObject controll = contactObj.transform.parent.GetComponent<CatchObject>();

                // 그랩한 오브젝트 상위 오브젝트 종속, HashSet 갱신    
                transform.parent = contactObj.transform.parent;
                controll.AddChild(transform.GetComponent<MeshCollider>());

                ClearState();
                GrabGun.instance.CancelObj();
            }

        }

        //ClearState();
        //GrabGun.instance.CancelObj();

    }

    //// 그랩한 물건이 이동형 오브젝트와 부딪힐때마다 물리력 행사 콜백
    private void OnCollisionEnter(Collision collision)
    {        
        if (collision.gameObject.CompareTag(contactTag))
        {
            ySpeed = 0;

            if(myRigid)
            {
                Vector3 temp = myRigid.velocity;
                temp.y = 0;
                myRigid.velocity = temp;
            }

        }
        if (checkContact)
        {
        
            gameObject.tag = contactTag;            
        }

        // 충돌한 오브젝트가 이동형 오브젝트라면
        if (collision.gameObject.layer == LayerMask.NameToLayer("MovedObject"))
        {
            // PaintTaget에 bool값 체크 존재, 페인팅된 대상에는 물리력을 부여 x Or 내가 페인팅된 상태면 X Or 특정 불값을 통해 연쇄적으로 서로에게 물리부여 상황 벗어나기
            if (collision.gameObject.GetComponent<PaintTarget>().CheckPainted() ||
                transform.childCount != 0 ||
                isSleep ||
                collision.gameObject.CompareTag(contactTag))
            {
                return;
            }

            // 대상이 조합형 오브젝트라면 And 내가 그랩한 오브젝트만 주변 오브젝트에 물리력을 부여한다.     
            if (collision.gameObject.transform.parent?.GetComponent<CatchObject>() != null && checkContact)
            {
                // GrabedObejct 레이어인 조합오브젝트에만 영향을 줘야함
                // 고정형, NPC에 붙은 조합오브젝트가 아니라면 물리력 부여
                if (collision.transform.parent?.gameObject.layer == LayerMask.NameToLayer("Default") ||
                    collision.transform.parent?.gameObject.layer == LayerMask.NameToLayer("NPC"))
                {
                    return;
                }

                // 조합된 오브젝트에 물리력 부여
                collision.gameObject?.transform.parent.GetComponent<CatchObject>().InitOverap();
            }
            // 대상이 단일 이동형 오브젝트라면 And 내가 그랩한 오브젝트만 주변 오브젝트에 물리력을 부여한다.            
            else if (collision.gameObject.layer == LayerMask.NameToLayer("MovedObject") && myRigid)
            {
                // 조합 오브젝트가 아닌 이동형 오브젝트에만 물리력 부여
                if(collision.gameObject.transform.parent?.GetComponent<CatchObject>() == null)
                {
                    if(checkContact)
                    {
                        Vector3 force;

                        if (collision.contacts[0].normal.y >= 0.5f)
                        {
                            myRigid.velocity = Vector3.zero;
                            force = Vector3.zero;
                        }
                        else
                        {
                            force = -(collision.contacts[0].normal) * 2f;
                        }
           
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

    // 충돌지점 본드 체크
    private void OnCollisionStay(Collision collision)
    {       
        // 그랩한 오브젝트가 플레이어 닿을시 임시 캔슬처리
        if (checkContact && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GrabGun.instance.CancelObj();
        }

        //그랩한 MovedObject가 아니면 충돌 포인트 체크
        if (!checkContact && myRigid)
        {            
            // { 이 구간은 1프레임에 벌어진 모든 충돌지점을 검사하는 것
            // 충돌지점을 모두 검사
            for (int i = 0; i < collision.contactCount; i++)
            {
                    //지점중 하단에서 발생한 충돌을 검사한다.
                if(-(collision.contacts[i].normal.y) <= -0.95f)
                {
                    //유효 충돌체크 이후 반복문 종료
                    checkCount++;
                    break;
                }

            }            

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

        if(collision.gameObject.CompareTag(contactTag))
        {
            ySpeed = 0;

            if(myRigid)
            {
                Vector3 temp = myRigid.velocity;
                temp.y = 0;
                myRigid.velocity = temp;
            }            
        }

        // 이미 본드 동작을 하는 오브젝트를 다시 그랩하면 그랩하는순간 충돌면을 체크하여 그랩 해제됨에 따라 상태를 제어할 bool값 추가
        if (checkContact == false || collision.gameObject.layer == LayerMask.NameToLayer("CatchObject"))
        {
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
                GameObject parentObj = null;    
                
                // 충돌한 오브젝트의 부모가 없을경우
                if(collision.transform.parent == null)
                {                    
                    // 상위 오브젝트 생성
                    parentObj = new GameObject("test" + test++);
                    parentObj.transform.gameObject.layer = LayerMask.NameToLayer("GrabedObject");                    
                    CatchObject controll = parentObj.AddComponent<CatchObject>();
                    GunStateController.AddList(controll);
                    parentObj.transform.position = collision.contacts[i].point;

                    // 그랩한 오브젝트와 충돌한 오브젝트 모두 상위 오브젝트 종속, HashSet 갱신                    
                    transform.parent = parentObj.transform;
                    collision.transform.parent = parentObj.transform;
                    //GunStateController.AddList(collision.gameObject.GetComponent<MovedObject>());
                    controll.AddChild(transform.GetComponent<MeshCollider>());
                    controll.AddChild(collision.transform.GetComponent<MeshCollider>());                    
                }
                // 부모가 존재할 경우
                else
                {                    
                    GameObject contactObj = collision.gameObject;                    

                    // TODO : 레이어 체크가 아닌 CatchObject 스크립트로 여부 확인
                    // 부모가 상위 오브젝트 경우
                    if(contactObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("GrabedObject"))
                    {                                        
                        // HashSet 갱신    
                        transform.parent = contactObj.transform.parent;
                        contactObj.transform.parent.GetComponent<CatchObject>().AddChild(transform.GetComponent<MeshCollider>());        

                    }
                    // 부모가 고정형 오브젝트 경우
                    else if(contactObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("Default"))
                    {
                        // 부딪힌 오브젝트도 고정형이면
                        if(contactObj.transform.gameObject.layer == LayerMask.NameToLayer("Default"))
                        {
                            // 클래스 여부로 기존 고정형 오브젝트인지 합쳐진 오브젝트인지
                            if(contactObj.transform.parent.GetComponent<CatchObject>())
                            {
                                transform.SetParent(contactObj.transform.parent);
                            }
                            else 
                            {
                                // 상위 오브젝트 생성
                                parentObj = new GameObject("test" + test++);
                                parentObj.transform.gameObject.layer = LayerMask.NameToLayer("Default");
                                CatchObject controll = parentObj.AddComponent<CatchObject>();
                                GunStateController.AddList(controll);
                                parentObj.transform.position = collision.contacts[i].point;

                                // 그랩한 오브젝트 상위 고정형 오브젝트 종속, HashSet 갱신    
                                transform.parent = parentObj.transform; 
                                controll.AddChild(transform.GetComponent<MeshCollider>());

                            }
                        }
                        else if(contactObj.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
                        {
                            parentObj = new GameObject("test" + test++);
                            parentObj.transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                            //transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                            CatchObject controll = parentObj.AddComponent<CatchObject>();
                            GunStateController.AddList(controll);
                            parentObj.transform.position = collision.contacts[i].point;

                            // 그랩한 오브젝트 상위 오브젝트 종속, HashSet 갱신    
                            transform.parent = parentObj.transform;                                                        
                            controll.AddChild(transform.GetComponent<MeshCollider>());
                            targetNpc = collision.transform.GetComponent<NpcBase>();
                            targetNpc.ChangedState(npcState.objectAttached);
                            GunStateController.AddList(targetNpc);
                        }
                        else
                        {                            
                            // 충돌한 이동형 오브젝트가 고정형에 집합된 형태라면 
                            if(contactObj.transform.parent.GetComponent<CatchObject>() != null &&
                                contactObj.transform.parent.gameObject.layer == LayerMask.NameToLayer("Default"))
                            {
                                //Debug.Log("===");
                                transform.parent = contactObj.transform.parent;
                                checkContact = false;                                
                                ClearState();
                                GrabGun.instance.CancelObj();
                                return;
                            }
                            // 상위 오브젝트 생성
                            parentObj = new GameObject("test" + test++);
                            parentObj.transform.gameObject.layer = LayerMask.NameToLayer("GrabedObject");                        
                            CatchObject controll = parentObj.AddComponent<CatchObject>();                        
                            GunStateController.AddList(controll);
                            parentObj.transform.position = collision.contacts[i].point;

                            // 그랩한 오브젝트와 충돌한 오브젝트 모두 상위 오브젝트 종속, HashSet 갱신    
                            transform.parent = parentObj.transform;
                            collision.transform.parent = parentObj.transform;                            
                            controll.AddChild(transform.GetComponent<MeshCollider>());
                            controll.AddChild(collision.transform.GetComponent<MeshCollider>());                        

                        }
                    }
                    // 아기곰의 경우
                    else if (contactObj.transform.gameObject.layer == LayerMask.NameToLayer("NPC"))
                    {
                        parentObj = new GameObject("test" + test++);
                        parentObj.transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                        //transform.gameObject.layer = LayerMask.NameToLayer("NPC");
                        CatchObject controll = parentObj.AddComponent<CatchObject>();
                        GunStateController.AddList(controll);
                        parentObj.transform.position = collision.contacts[i].point;

                        // 그랩한 오브젝트 상위 오브젝트 종속, HashSet 갱신    
                        transform.parent = parentObj.transform;
                        controll.AddChild(transform.GetComponent<MeshCollider>());
                        targetNpc = collision.transform.GetComponent<NpcBase>();
                        targetNpc.ChangedState(npcState.objectAttached);
                        GunStateController.AddList(targetNpc);
                    }
                    // 엔피씨에 붙은 조합형 일 경우
                    else if(contactObj.transform.gameObject.layer == LayerMask.NameToLayer("MovedObject") &&
                        contactObj.transform.parent?.GetComponent<CatchObject>() != null)
                    {
                        CatchObject controll = contactObj.transform.parent.GetComponent<CatchObject>();

                        // 그랩한 오브젝트 상위 오브젝트 종속, HashSet 갱신    
                        transform.parent = contactObj.transform.parent;
                        controll.AddChild(transform.GetComponent<MeshCollider>());
                    }

                }
                        
                ClearState();
                GrabGun.instance.CancelObj();
                
            }
        }
    }

    public void ChangedState()
    {
        myRigid = GetComponent<Rigidbody>();
        myRigid.mass = 1f;
        myColid.material.dynamicFriction = 0f;
        myColid.material.bounciness = 0f;   
        checkCount = 0;
        checkContact = true;             

        if(transform.childCount != 0)
        {
            triigerCollider = GetComponentsInChildren<SphereCollider>();            
        }        

    }

    void StartContact()
    {
        checkContact = true;
    }


    public void CelarBond()
    {
        if (transform.parent != null)
        {
            myColid.convex = true;

            myRigid = transform.GetComponent<Rigidbody>();
            if (!myRigid) myRigid = transform.AddComponent<Rigidbody>();

            myRigid.velocity = Vector3.down * 3f;
            myRigid.mass = 10f;
            myRigid.useGravity = true;
            myColid.material.dynamicFriction = 1f;            
        }

        checkCount = 0;
        checkContact = false;
        isSleep = false;
    }

    void ClearState()
    {
        if(myRigid != null)
        {
            Destroy(myRigid);
        }

        checkContact = false;
        myColid.convex = false;
    }

    // 강제 슬립?
    void SleepObj()
    {
        if (isSleep) return;

        checkContact = false;
        myRigid.velocity = Vector3.zero;
        Destroy(myRigid);        
        myColid.convex = false;
        isSleep = true;
        //contactTime = 0f;
        ySpeed = 0f;
        gameObject.tag = unContactTag;

        Invoke("ClearTime", 1.5f);
    }

    public void InitOverap()
    {
        if(!myRigid)
        {
            transform.AddComponent<Rigidbody>();
            myRigid = GetComponent<Rigidbody>();
            myRigid.velocity = Vector3.down * 3f;
            myRigid.mass = 10f;

        }
        
        myColid.material.dynamicFriction = 0.8f;
        myColid.material.bounciness = 0.5f;
        myColid.convex = true;      
    }

    public void InitOverap(Vector3 _velocity)
    {
        if (!myRigid)
        {
            transform.AddComponent<Rigidbody>();
            myRigid = GetComponent<Rigidbody>();
            myRigid.velocity = _velocity;            
            myRigid.mass = 10f;

        }

        myColid.material.dynamicFriction = 0.8f;
        myColid.material.bounciness = 0.5f;
        myColid.convex = true;
    }

    public void CancelGrab()
    {
        checkContact = false;
        ySpeed = 0;
    }
    

    // TODO : 오브젝트 풀링으로 오버헤드 없애야 함
    public void ClearTrigger()
    {        
        GameObject[] myChild = new GameObject[transform.childCount];

        // 내 자식만큼 오브젝트 캐싱
        for (int i = 0; i < myChild.Length; i++)
        {
            myChild[i] = transform.GetChild(i).gameObject;
        }

        // 오브젝트 부모 변경 및 해쉬 갱신
        for (int i = 0; i < myChild.Length; i++)
        {
            Destroy(myChild[i].gameObject);
        }
    }

    void ClearTime()
    {        
        isSleep = false;
        contactTime = 0;
    }

}
