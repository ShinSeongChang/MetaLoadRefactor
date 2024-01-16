using Unity.VisualScripting;
using UnityEngine;

public class MovedObject_Refactor : MonoBehaviour
{
    #region PrivateComponent
    MeshCollider myColid;

    protected CatchObject_Refactor combineObj = null;
    protected GameObject contactObj = null;
    protected LayerMask layerMask;
    protected Rigidbody myRigid = null;
    protected NpcBase targetNpc = null;
    
    #endregion

    #region PrivateValue

    protected int checkCount = 0;
    protected float ySpeed = default;
    protected float contactTime = 0f;
    protected float decrementGravity = 0.5f;
    protected float decrement = 0.5f;
    protected float maxGravity = 30f;

    protected bool isSleep = false;
    protected bool checkContact = false;    

    protected string contactTag = "ContactObject";
    protected string unContactTag = "Untagged";
    protected string catchObjectName = "CatchObject";
    
    protected const string catchLayer = "GrabedObject";
    protected const string defaultLayer = "Default";
    protected const string movedLayer = "MovedObject";
    protected const string npcLayer = "NPC";
        
    Coroutine sleepCoroutine;

    #endregion

    #region Property

    // 충돌을 감지하면 안되는 상황 묶기    
    protected bool CanContact  => CheckLayer(contactObj) && transform.parent == null && checkContact;
    protected bool CanOverap => CheckPaint(contactObj) && transform.childCount != 0 && isSleep && CheckTag(contactObj);

    #endregion

    void Awake()
    {
        StartCaching();      
    }

    void Update()
    {        
        FallingObject();
    }

    protected virtual void OnTriggerEnter(Collider collision)
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
            combineObj.SetHash(combineObj, myColid);
        }
        // 충돌한 오브젝트에 CatchObject가 없다면 부모 오브젝트의 Combine 상태를 체크한다.
        else
        {
            // Combine 된 상태라면
            if (collisionObj.GetComponentInParent<CatchObject_Refactor>())
            {
                // 해당 부모 오브젝트에 나 자신 종속,
                combineObj = collisionObj.GetComponentInParent<CatchObject_Refactor>();
                combineObj.SetHash(combineObj, myColid);
            }
            // Combine 안된 상태라면 => 고정형 오브젝트, NPC, 그랩이후 해제된 MovedObject들
            else
            {
                // 고정형과 부딪혔을 때
                if (collisionObj.layer == LayerMask.NameToLayer(defaultLayer))
                {
                    CreateCatchObject(GetComponent<Collider>(), defaultLayer);
                    combineObj.SetHash(combineObj, myColid);
                }
                // NPC와 부딪혔을 때
                else if (collisionObj.layer == LayerMask.NameToLayer(npcLayer))
                {
                    CreateCatchObject(GetComponent<Collider>(), npcLayer);
                    combineObj.SetHash(combineObj, myColid);
                    targetNpc = collisionObj.GetComponent<NpcBase>();
                    targetNpc.ChangedState(npcState.objectAttached);
                }
                // 이동형과 부딪혔을 때
                else if (collisionObj.layer == LayerMask.NameToLayer(movedLayer))
                {
                    CreateCatchObject(GetComponent<Collider>(), catchLayer);
                    combineObj.SetHash(combineObj, myColid, collisionObj.GetComponent<MeshCollider>());
                }
            }

            ClearState();
            GrabGun_Refactor.instance.CancelObj();
        }

    }

    //// 그랩한 물건이 이동형 오브젝트와 부딪힐때마다 물리력 행사 콜백
    protected virtual void OnCollisionEnter(Collision collision)
    {            
        contactObj = collision.gameObject;

        if (collision.gameObject.CompareTag(contactTag))
        {
            ySpeed = 0;

            if (myRigid)
            {
                Vector3 temp = myRigid.velocity;
                temp.y = 0;
                myRigid.velocity = temp;
            }

        }

        if(!CanContact)
        {
            return;
        }

        if (checkContact)
        {
            gameObject.tag = contactTag;
        }

        // 충돌한 오브젝트가 이동형 오브젝트라면
        if (collision.gameObject.layer == LayerMask.NameToLayer("MovedObject"))
        {
            // PaintTaget에 bool값 체크 존재, 페인팅된 대상에는 물리력을 부여 x Or 내가 페인팅된 상태면 X Or 특정 불값을 통해 연쇄적으로 서로에게 물리부여 상황 벗어나기
            if (!CanOverap)
            {
                return;
            }

            // Combine된 오브젝트면 Combine에 물리력 부여           
            if (collision.transform.GetComponentInParent<CatchObject_Refactor>())
            {
                if (checkContact)
                {
                    Vector3 force = -(collision.contacts[0].normal * 2f);
                    collision.gameObject.GetComponent<CatchObject_Refactor>().InitOverap(force);
                }
                else
                {
                    collision.gameObject.GetComponent<CatchObject_Refactor>().InitOverap();
                }
            }
            // 단일 오브젝트면 단일 오브젝트에 물리력 부여           
            else
            {
                if (checkContact)
                {
                    Vector3 force = -(collision.contacts[0].normal * 2f);
                    collision.gameObject.GetComponent<MovedObject_Refactor>().InitOverap(force);
                }
                else
                {
                    Vector3 force = -(collision.contacts[0].normal * 2f);
                    collision.gameObject.GetComponent<MovedObject_Refactor>().InitOverap(force);
                }
            }
        }

    }

    // 충돌지점 본드 체크
    protected virtual void OnCollisionStay(Collision collision)
    {
        contactObj = collision.gameObject;

        // 그랩한 오브젝트가 플레이어 닿을시 임시 캔슬처리
        if (checkContact && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GrabGun_Refactor.instance.CancelObj();
        }

        //그랩한 MovedObject가 아니면 충돌 포인트 체크
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

        if (collision.gameObject.CompareTag(contactTag))
        {
            ySpeed = 0;

            if (myRigid)
            {
                Vector3 temp = myRigid.velocity;
                temp.y = 0;
                myRigid.velocity = temp;
            }
        }
        
        if (!CanContact)
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
                GameObject collisionObj = collision.gameObject;
                Collider collider = collisionObj.GetComponent<Collider>();

                if (collisionObj.GetComponent<CatchObject_Refactor>())
                {                    
                    combineObj = collisionObj.GetComponent<CatchObject_Refactor>();
                    SetHash(combineObj, myColid);
                }
                else
                {                            
                    // 부모 오브젝트의 Combine 상태를 체크한다.
                    if (collisionObj.GetComponentInParent<CatchObject_Refactor>())
                    {                       
                        combineObj = collisionObj.GetComponentInParent<CatchObject_Refactor>();
                        SetHash(combineObj, myColid);
                    }
                    // Combine 안된 상태라면 => 고정형 오브젝트, NPC, 그랩이후 해제된 MovedObject들
                    else
                    {                                                                        
                        if(collisionObj.layer == LayerMask.NameToLayer(defaultLayer))
                        {
                            CreateCatchObject(collider, defaultLayer);
                            SetHash(combineObj, myColid);                            
                        }
                        else if(collisionObj.layer == LayerMask.NameToLayer(npcLayer))
                        {
                            CreateCatchObject(collider, npcLayer);
                            SetHash(combineObj, myColid);
                            targetNpc = collisionObj.GetComponent<NpcBase>();
                            targetNpc.ChangedState(npcState.objectAttached);
                        }
                        else if(collisionObj.layer == LayerMask.NameToLayer(movedLayer))
                        {                            
                            CreateCatchObject(collider, catchLayer);
                            SetHash(combineObj, myColid, collisionObj.GetComponent<MeshCollider>());                            
                        }
                    }

                }
                
                ClearState();
                GrabGun_Refactor.instance.CancelObj();
            }
        }
    }

    public virtual void ChangedState()
    {
        myRigid = GetComponent<Rigidbody>();
        myRigid.mass = 1f;
        myColid.material.dynamicFriction = 0f;
        myColid.material.bounciness = 0f;
        checkCount = 0;
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

    protected virtual void ClearState()
    {
        if (myRigid != null)
        {
            Destroy(myRigid);
        }

        checkContact = false;
        myColid.convex = false;
    }

    // 강제 슬립?
    protected virtual void SleepObj()
    {
        if (isSleep) return;

        checkContact = false;
        myRigid.velocity = Vector3.zero;
        Destroy(myRigid);
        myColid.convex = false;
        isSleep = true;        
        ySpeed = 0f;
        gameObject.tag = unContactTag;

        Invoke("ClearTime", 1.5f);
    }

    public virtual void InitOverap()
    {
        if (!myRigid)
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

    public virtual void InitOverap(Vector3 _velocity)
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

    /// <summary>
    /// Rigidbody가 존재하는 MovedObject 강제 중력값 넣는 메소드
    /// </summary>
    protected void FallingObject()
    {
        // 내 리지드바디가 존재하고, 그랩한 대상이 아닐 때 (그랩한 대상은 낙하속도 X)
        if (myRigid && !checkContact)
        {
            if (myRigid.velocity.magnitude <= 0.5f && !isSleep)//(contactTime >= 3)
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

    /// <summary>
    /// MovedObject의 충돌 이벤트 발생시 새로 만들어낼 상위 객체 정의하는 메소드
    /// </summary>
    /// <param name="collision">충돌한 오브젝트 Collider</param>    
    /// <param name="parentLayer">상위 오브젝트 설정 레이어</param>
    protected void CreateCatchObject(Collider collision, string parentLayer)
    {
        // 상위 오브젝트 생성 및 Component Setting
        GameObject parentObj = new GameObject(catchObjectName);         // 상위 오브젝트 생성
        parentObj.layer = LayerMask.NameToLayer(parentLayer);           // 상위 오브젝트 Layer 설정
        combineObj = parentObj.AddComponent<CatchObject_Refactor>();             // 상위 오브젝트 스크립트 추가
        GunStateController.AddList(combineObj);                         // Controller에서 재장전시 목록 순회를 위해 HashSet 갱신
        parentObj.transform.position = collision.ClosestPoint(transform.position);  // 상위 오브젝트 위치 정렬  
    }
        
    /// <summary>
    /// 생성된 상위 오브젝트에서 관리할 자식 Collider 갱신해주기 위한 메소드
    /// <para>
    /// 전달받은 Collider 객체들을 상위 오브젝트에 종속 시킨다.
    /// </para>
    /// </summary>    
    /// <param name="colliders">부모 오브젝트에 종속되는 오브젝트들의 MeshCollider</param>
    protected void SetHash(CatchObject_Refactor parent, params MeshCollider[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            // HashSet 갱신
            parent.AddChild(colliders[i]);

            // 상위 오브젝트에 전달받은오브젝트들 종속
            colliders[i].transform.SetParent(parent.transform);
        }

        combineObj = null;
    }

    #region BindingMethod

    /// <summary>
    /// 오브젝트 인스턴스시점에 캐싱할 목록을 묶어두는 메소드
    /// </summary>
    protected virtual void StartCaching()
    {
        // 충돌감지 Bool 값 false 설정으로 그랩한 오브젝트만 변환
        checkContact = false;

        // 내 메쉬 콜라이더 저장
        myColid = GetComponent<MeshCollider>();

        // 레이어 마스크 설정
        layerMask = LayerMask.GetMask(defaultLayer, npcLayer, "StaticObject", movedLayer, catchLayer);
    }    

    bool CheckLayer(GameObject contactObj)
    {
        if (contactObj.layer == LayerMask.NameToLayer("CatchObject") ||
            contactObj.layer == LayerMask.NameToLayer("Wall") ||
            contactObj.layer == LayerMask.NameToLayer("InteractZone") ||
            contactObj.layer == LayerMask.NameToLayer("TriggerObject"))        
        {            
            return false;
        }
        
        return true;
    }

    bool CheckPaint(GameObject contactObj)
    {
        if(!contactObj.GetComponent<PaintTarget>().CheckPainted())
        {
            return false;
        }

        return true;
    }

    bool CheckTag(GameObject contactObj)
    {
        if (contactObj.CompareTag(contactTag))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region InvokeMethod
    void StartContact()
    {
        checkContact = true;
    }
    void ClearTime()
    {
        isSleep = false;
        contactTime = 0;
    }

    #endregion

   
}
