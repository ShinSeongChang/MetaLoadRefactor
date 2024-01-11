using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Controller_Physics : MonoBehaviour
{
    [Header("LayerMask")]
    [SerializeField]
    LayerMask groundMask = -1;
    [SerializeField]
    LayerMask objectMask = -1;
    [SerializeField]
    LayerMask climbMask = -1;
    [SerializeField] 
    LayerMask colorCheckLayer = -1;
    [SerializeField] 
    LayerMask aimLayer = -1;

    [Header("Component")]
    [SerializeField]
    Rigidbody rb;
    [SerializeField]
    Transform playerInputSpace = default;
    [SerializeField]
    Transform playerCenter = default;
    //[SerializeField]
    //TrailRenderer trailRenderer;
    [SerializeField]
    Animator animator;
    [SerializeField]
    GunStateController gunController;
    [SerializeField]
    MeshRenderer frontGunRender;
    [SerializeField]
    MeshRenderer handGunRender;
    [SerializeField]
    MeshRenderer backGunRender;
    [SerializeField]
    CameraController cameraManager;

    #region Private Reference

    RaycastHit groundHit;

    Rigidbody connectedBody;
    Rigidbody previousConnectedBody;

    Vector3 input = Vector3.zero;
    Vector3 inputMouse = Vector3.zero;

    Vector3 velocity;
    Vector3 connectionVelocity;

    Vector3 upAxis;
    Vector3 rightAxis;
    Vector3 forwardAxis;
    Vector3 gravity;

    Vector3 connectionWorldPosition;
    Vector3 connectionLocalPosition;

    Vector3 contactNormal;
    Vector3 steepNormal;
    Vector3 climbNormal;
    Vector3 lastClimbNormal = Vector3.up;

    Vector3 beforePosition;

    Coroutine fireDelay = null;

    Queue<GameObject> climbObject;
    #endregion

    #region Private Value
    float minGroundDotProduct;
    float minObjectDotProduct;
    float minClimbDotProduct;
    //float currMouseSpeed = 0;
    float moveMultiple = default;

    float idleTime = 0;

    bool desireClimb = false;
    //bool desireOutClimb = false;
    bool desireJump = false;
    bool desireRun = false;
    bool multipleState;

    bool isJump = false;
    bool canJump = true;
    bool canFire = false;

    bool playingReloadAnimation = false;
    //bool playingEquipAnimation = false;

    int jumpPhase = 0;
    int stepsSinceLastGrounded = 0;
    int stepsSinceLastJump = 0;
    int stepsSinceLastClimb = 0;
    int stepsSinceLastMultiple = 0;

    int allContactCount = 0;
    int groundContactCount = 0;
    int steepContactCount = 0;
    int climbContactCount = 0;

    bool beforeColored = false;
    byte checkedFrame = 0;
    //int catchObject;

    #endregion
    public static bool stopState { get; private set; }

    public bool IsMove => (input.magnitude != 0);
    public bool CanFire => canFire && CanReload;

    // 12.21 SSC : NPC 대화중(stopState) 사격, 재장전 방지 위해 CanReload => !stopState 추가
    // 12.21 SSC : 상점창 사격, 재장전 방지 위해 CanReload => !storeUI.activeSelf 추가
    public bool CanReload => !playingReloadAnimation && !OnClimb && !stopState && !controller_UI.CheckAllUISetActiveTrue();
    public bool OnMultipleState => multipleState;
    public bool OnGround => groundContactCount > 0;
    public bool OnSteep => steepContactCount > 0;
    public bool OnClimb => climbContactCount > 0;
    public bool OnClimbAnimation => animator.GetCurrentAnimatorStateInfo(0).IsName("Climb");

    [SerializeField, Range(0, 360)]
    float viewAngle = 90;

    [SerializeField, Range(1, 5)]
    int climbHelpTime = 2;

    [Header("Player Setting")]
    [SerializeField, Range(0, 100f)]
    float jumpHeight = default;
    [SerializeField, Range(0, 10)]
    float jumpDelayTime = 2f;
    [SerializeField, Range(0, 10)]
    float fireDelayTime = 1f;

    [SerializeField, Min(0f)]
    float probeDistance = default;
    [SerializeField, Range(1, 10)]
    float runMultiple = default;
    [SerializeField, Range(0, 1)]
    float walkMultiple = default;
    [SerializeField, Range(0, 100f)]
    float gravityMultiple = 1;

    [SerializeField, Range(0, 100)]
    float maxMoveSpeed = default;
    [SerializeField, Range(0, 100)]
    float maxAirMoveSpeed = default;
    [SerializeField, Range(0,100f)]
    float maxClimbSpeed = default;

    [SerializeField, Range(0, 100)]
    float maxAcceleration = default;
    [SerializeField, Range(0, 100)]
    float maxAirAcceleration = default;
    [SerializeField, Range(0, 100)]
    float maxClimbAcceleration = default;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = default;
    [SerializeField, Range(0, 5)]
    int maxAirJumps = default;

    [SerializeField, Range(0, 90f)]
    float maxGroundAngle = default;
    [SerializeField, Range(0, 90f)]
    float maxObjectAngle = default;
    [SerializeField, Range(90, 180f)]
    float maxClimbAngle = default;

    [SerializeField, Range(1, 10)]
    byte checkFrameRate = 2;

    [Header("RigController")]
    [SerializeField] Transform startPoint;
    [SerializeField] Transform cameraPoint;

    [SerializeField] Transform aimTarget;
    [SerializeField] Transform rotateTarget;

    [SerializeField] float aimRange = 50;

    [SerializeField] Rig aimRig;
    [SerializeField] Rig rotateRig;

    RaycastHit aimHit;

    LineRenderer l;
    // UI 컨트롤러 _ 240102배경택
    private Controller_UI controller_UI;

    #region Animator Hash
    private readonly int IdleTimeHash = Animator.StringToHash("IdleTime");
    private readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    private readonly int VelocityYHash = Animator.StringToHash("VelocityY");
    private readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    private readonly int ClimbHash = Animator.StringToHash("OnClimb");
    private readonly int GroundHash = Animator.StringToHash("OnGround");
    private readonly int ReloadTriggerHash = Animator.StringToHash("Reload");
    private readonly int EquipStateHash = Animator.StringToHash("EquipState");
    private readonly int EquipTriggerHash = Animator.StringToHash("EquipTrigger");
    private readonly int ClimbWaitHash = Animator.StringToHash("ClimbWait");
    #endregion

    private void OnDisable()
    {
        stopState = false;
    }

    //에디터에서 처리
    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minObjectDotProduct = Mathf.Cos(maxObjectAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);        
    }

    private void Awake()
    {
        //l = gameObject.AddComponent<LineRenderer>();
        //l.positionCount = 2;
        Application.targetFrameRate = 60;
        climbObject = new Queue<GameObject>();

        cameraPoint = Camera.main.transform;
        gravity = CustomGravity.GetGravity(rb.position, out upAxis);

        OnValidate();

        BindHandler();
        rb.useGravity = false;

        canFire = false;
        fireDelay = StartCoroutine(fireDelayRoutine(fireDelayTime));
        controller_UI = GetComponent<Controller_UI>();

        stepsSinceLastClimb = climbHelpTime + 1;
        groundContactCount = 1;
        contactNormal = Vector3.up;
    }

    void Update()
    {
        //l.SetPosition(0, rb.position);
        //l.SetPosition(1, rb.position - upAxis * probeDistance);

        // Debug.Log("업데이트" + transform.position);
        //대화나 메뉴에서 stop시킴
        if (stopState)
        {
            return;
        }

        TrailRenderer t = GetComponent<TrailRenderer>();
        if (t)
        {

            Color color = new Color(0, 0, 0, 1);
            color.r = OnGround ? 1 : 0;
            color.g = OnSteep ? 1 : 0;
            color.b = OnClimb ? 1 : 0;
            t.material.color = color;
        }

        UpdateAxis();
        UpdateFrameState();
        UpdateInputState();
        UpdateAnimationParameter();


        if (gunController.CurrentMode.mode == GunMode.Paint)
        {
            if (gunController.CurrentMode.ShootGun())
            {
                gunController.Shoot(GunMode.Paint);
                idleTime = 0;
            }
        }

        else if (desireFire)
        {
            desireFire = false;
            if (gunController.CurrentMode.ShootGun())
            {
                gunController.Shoot(GunMode.Paint);
                idleTime = 0;
            }
        }

        if (desireGrab)
        {
            desireGrab = false;
            gunController.Shoot(GunMode.Grab);
        }

        // 장전        
        if (reader.ReloadKey)
        {
            gunController.Reloading();
        }

        

    }

    private void UpdateFrameState()
    {
        if (checkedFrame >= checkFrameRate)
            checkedFrame = 0;
        checkedFrame += 1;
    }

    private void FixedUpdate()
    {
        //Debug.Log("픽스드" + transform.position);
        //Debug.Log(connectedBody?.name);

        //대화나 메뉴에서 stop시킴
        if (stopState)
        {
            /*rb.velocity += new Vector3(0, gravity.y*Time.deltaTime, 0);
            if (rb.velocity.y > 10)
            {
                velocity = Vector3.zero;
            }*/
            return;
        }
        beforePosition = transform.position;

        /*Debug.Log("FixedUpdate");
        Debug.Log(allContactCount);
        Debug.Log(groundContactCount);
        Debug.Log(climbContactCount);
        Debug.Log(steepContactCount);
        Debug.Log(desireClimb);
        Debug.Log(multipleState);*/

        //if (climbContactCount > 0) 
        //Debug.Break();
        //Debug.Log();


        //상태 업데이트
        //OnGround, OnClimb 결정
        UpdateState();

        /*Debug.Log("UpdateState");
        Debug.Log(OnGround);
        Debug.Log(OnClimb);
        Debug.Log(OnSteep);
        Debug.Break();*/

        //속도 계산
        AdjustVelocity();
        //점프 계산
        AdjustJump();

        //Debug.Log("1 "+velocity);

        //점프 상태일 경우 2배의 그래비티 적용
        if (!isJump && !OnGround && !OnClimb)
        {
           // Debug.Log("점프상태");
            velocity += gravity * gravityMultiple * Time.deltaTime;
        }


        //중력우선순위
        //점프 > 빠져나가기 > 등산 보정 > 등산 > 지상 끈끈이 > 지상 정지  > 기타(경사)

        if (isJump)
        {
          //  Debug.Log("점프");
            //일반 중력만 적용
            velocity += gravity * Time.deltaTime;
        }
        else if ((stepsSinceLastMultiple <= 2) && input.z < 0)
        {
          // Debug.Log("빠져나가려는 상태일 경우");
            //등산중에 접촉면으로 끌어당김
            velocity.y = 0;
        }

        else if (OnClimb && !desireClimb)
        {
         //   Debug.Log("등산 벗어나기 직전");
            velocity *= 0.3f;
            velocity -= lastClimbNormal.normalized * maxClimbAcceleration * Time.deltaTime;
        }

        else if (OnClimb )
        {
          //  Debug.Log("등산만");
            //등산중에 접촉면으로 끌어당김
            velocity -= contactNormal.normalized * maxClimbAcceleration * 0.9f * Time.deltaTime;
        }

        else if (OnGround && desireClimb)
        {
           // Debug.Log("지상, 끈끈이");
            //땅에 있을 경우 + 이동 상태 일 경우 중력+접촉면으로 끌어당김 동시에 적용
            velocity += (gravity - contactNormal.normalized * maxClimbAcceleration * 0.9f) * Time.deltaTime;
        }
        else if (OnGround && velocity.sqrMagnitude < 0.01f)
        {
            //Debug.Log("지상, 정지");
            //땅에 있을 경우 + 정지상태일 경우 밀려나지않는 상태
            velocity += contactNormal.normalized * (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
        }
        else
        {
           /// Debug.Log("그외");
            //일반 중력 적용
            velocity += gravity * gravityMultiple * Time.deltaTime;
        }
        Vector3 limit = velocity;

       /* if (limit.y < -40)
        {
            limit.y = -40;
        }
        else if(limit.y > 40)
        {
            limit.y = 40;
        }

        if (limit.z<-30)
        {
            limit.z = -30;
        }
        else if (limit.z > 30)
        {
            limit.z = 30;
        }

        if(limit.x < -30)
        {
            limit.x = -30;
        }
        else if (limit.x > 30)
        {
            limit.x = 30;
        }*/

        //속도 적용
        rb.velocity = limit;

        //상태 초기화
        ClearState();
    }


    Quaternion curr;
    void LateUpdate()
    {
        //대화나 메뉴에서 stop시킴
        if (stopState)
        {
            /*rb.velocity += new Vector3(0, gravity.y*Time.deltaTime, 0);
            if (rb.velocity.y > 10)
            {
                velocity = Vector3.zero;
            }*/
            return;
        }


        if (playingReloadAnimation)
        {
            frontGunRender.enabled = true;
            backGunRender.enabled = (false);
        }


        aimRig.weight = stepsSinceLastClimb< climbHelpTime && !isJump ? 0 : 1;
        rotateRig.weight = stepsSinceLastClimb < climbHelpTime && !isJump ? 1 : 0;


        if (!OnGround && OnClimb)
        {
            rotateTarget.rotation = Quaternion.LookRotation(-climbNormal);
        }
        else if(multipleState)
        {
            rotateTarget.rotation = Quaternion.LookRotation(playerInputSpace.forward);
        }
        else if (stepsSinceLastClimb<climbHelpTime && !isJump)
        {
            rotateTarget.rotation = Quaternion.LookRotation(-lastClimbNormal);
        }

    }

    public void SetAimPosition(Vector3 position)
    {
        //aimTarget.position = position;
        aimTarget.position = Vector3.Lerp(aimTarget.position, position, Time.deltaTime * 5);

        if (position == Vector3.zero)
        {
            aimTarget.position = cameraPoint.position + cameraPoint.forward * aimRange;
            if (Physics.Raycast(startPoint.position, aimTarget.position - startPoint.position, out aimHit, aimRange, aimLayer))
            {
                aimTarget.position = Vector3.Lerp(aimTarget.position, aimHit.point, Time.deltaTime * 5);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stopState) return;
        //Debug.Log("엔터" + transform.position);
        EvaluateCollision(collision);
    }


    private void OnCollisionStay(Collision collision)
    {
        if (stopState) return;
        //Debug.Log("스테이" + transform.position);
        EvaluateCollision(collision);
    }
/*
    private void OnCollisionExit(Collision collision)
    {
        collision.gameObject.tag = "Untagged";
    }*/

    void AdjustVelocity()
    {
        //땅이 아닐 경우 + 인풋이 없을 경우
        if (!OnGround && input.magnitude == 0)
        {
            //해당 함수를 실행하지않는다.
            return;
        }
        //땅일 경우 + 인풋이 없을 경우
        else if (OnGround && input.magnitude == 0)
        {
            //정지 상태를 유지한다.
            velocity = Vector3.zero;
            return;
        }

        float acceleration;
        float speed;

        Vector3 xAxis;
        Vector3 zAxis;


        //상태에 따라 가속도, 최고속도, 좌우/앞뒤 축을 재 조정한다.
        if (allContactCount == 0)
        {
          //  Debug.Log("좌표-점프");
            acceleration = maxAirAcceleration;
            speed = maxMoveSpeed;
            //점프 상태일 경우 플레이어의 좌/우 측을 따른다.
            xAxis = rightAxis;
            //점프 상태일 경우 플레이어의 앞/뒤 축을 따른다.
            zAxis = forwardAxis;
            moveMultiple = 1;
        }
        /*else if (multipleState)
        {
            // Debug.Log("좌표-멀티");
            acceleration = maxAcceleration;
            speed = maxMoveSpeed;
            //등산 상태일 경우 접촉표면과 upaxis의 법선 벡터가 좌/우 축이 된다.
            xAxis = Vector3.Cross(contactNormal, upAxis);
            //등산 상태일 경우 앞/뒤 -> 위/아래로 구현한다.
            zAxis = upAxis;
        }*/
        else if (OnClimb && desireClimb)
        {
           // Debug.Log("좌표-등산");
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            //등산 상태일 경우 접촉표면과 upaxis의 법선 벡터가 좌/우 축이 된다.
            xAxis = Vector3.Cross(contactNormal, upAxis);
            //등산 상태일 경우 앞/뒤 -> 위/아래로 구현한다.
            zAxis = upAxis;
            moveMultiple = 1;
        }
        else if (OnClimb && !desireClimb)
        {
          //  Debug.Log("좌표-등산보정");
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            //등산 상태일 경우 접촉표면과 upaxis의 법선 벡터가 좌/우 축이 된다.
            xAxis = Vector3.Cross(lastClimbNormal, upAxis);
            //등산 상태일 경우 앞/뒤 -> 위/아래로 구현한다.
            zAxis = upAxis;
            moveMultiple = walkMultiple;
        }
        else
        {
           // Debug.Log(OnGround);
           // Debug.Log("좌표-땅/공중");
            acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            speed = OnGround && desireClimb? maxClimbSpeed: maxMoveSpeed;
            //speed = maxMoveSpeed;
            //기본 상태일 경우 플레이어의 좌/우 측을 따른다.
            xAxis = rightAxis;
            //기본 상태일 경우 플레이어의 앞/뒤 축을 따른다.
            zAxis = forwardAxis;
        }

        //x축(좌우방향)과 접촉표면의 사영 벡터를 사용한다.
        xAxis = ProjectDirectionOnPlane(xAxis, contactNormal);

        //멀티상태(땅 상태+등산 상태)일 경우 + 뒤로 갈 경우 
        if (multipleState && input.z < 0)
        {
            //z축을 앞/뒤로 변경하여 벗어나기 쉬운 상태로 만들어준다.
            //desireOutClimb = true;
            acceleration = maxAcceleration;
            speed = maxMoveSpeed;
            zAxis = forwardAxis;
        }

        //그외의 경우, Z축(전진방향)과 접촉표면의 사영 벡터를 사용한다.
        else
        {
            zAxis = ProjectDirectionOnPlane(zAxis, contactNormal);
        }

        //만약 connection 플랫폼이 존재할 경우 해당 속도만큼 속도에 영향을 받는다.
        //Vector3 relativeVelocity = velocity - connectionVelocity;


        Vector3 relativeVelocity = velocity;

        //이동 방향을 현재 기울기에 따라 힘 조절한다.
        float currX = Vector3.Dot(relativeVelocity, xAxis);
        float currZ = Vector3.Dot(relativeVelocity, zAxis);

        //가속
        float maxSpeedChange = acceleration * Time.deltaTime;

        //원하는 속도로 가속/감속한다.
        float newX = Mathf.MoveTowards(currX, input.x * speed * moveMultiple, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currZ, input.z * speed * moveMultiple, maxSpeedChange);

       // Debug.Log(xAxis +" "+zAxis);
       // Debug.Log((newX + " " + currX) + " " + (newZ + " " + currZ));

        //새로운 속도와 현재 속도의 차이만큼 가속 시킨다.
        velocity += xAxis * (newX - currX) + zAxis * (newZ - currZ);

    }

    private void AdjustJump()
    {
        if (OnClimb && !animator.GetCurrentAnimatorStateInfo(0).IsName("Climb"))
        {
            return;
        }

        //점프 키 눌렀을 경우만
        if (desireJump)
        {
            desireJump = false;
            Jump(gravity);
        }
    }

    //버그
    //수정해줬으면 좋겠는거
    //추가하고싶은거(복잡한거) <- 목 ~ 금요일

        

    //디벨롭모드

    //소리

    //바라보는 각도

    //그림자

    //추가낙하속도
    //팅기는 횟수
    //팅기고 난 뒤에 fixed

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;

        //등산 상태일 경우
        if (OnClimb)
        {
            //인풋없거나 뒤로 점프할 경우
            if (input.magnitude == 0 || input.z < 0)
            {
                //점프 방향을 접촉 표면과 같게 한다.
                jumpDirection = contactNormal;
                cameraManager.UpdateFixedAngle();
                cameraManager.PlayBlendCameraRoutine();
                cameraManager.ChangePriorityCamera(CameraType.Climb, 1);
            }

            //인풋이 있을 경우
            else
            {
                //해당 입력 방향으로 좌우/상하를 바꾼다.
                Vector3 xAxis = ProjectDirectionOnPlane(Vector3.Cross(contactNormal, upAxis), contactNormal);
                Vector3 zAxis = ProjectDirectionOnPlane(upAxis, contactNormal);
                jumpDirection = (input.x * xAxis * 2 + input.z * zAxis).normalized;

                //이전 속도를 초기화한다.
                velocity = Vector3.zero;
            }
        }

        //땅에 있을 경우
        else if (OnGround)
        {
            //위쪽으로 점프
            jumpDirection = upAxis;
        }

        //등산상태x 경사상태o
        else if (OnSteep && steepContactCount == allContactCount)
        {
            jumpDirection = steepNormal;
            //jumpPhase = 0;
            //return;
        }

        //연속 점프 가능할경우(현재 사용 안함)
        else if (maxAirJumps > 0 &&jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        //그외
        else
        {
            //점프 못함
            return;
        }

        if (jumpPhase == 0)
        {
            int id = (int)PlayerSoundList.Jump;
            SoundManager.instance.PlaySound(GroupList.Player, id);

            //점프 상태
            isJump = true;
            //등산 딜레이
            StartCoroutine(climbDelayRoutine(OnClimb ? jumpDelayTime : 0f));
            //점프 애니메이션
            animator.SetTrigger(JumpTriggerHash);
            animator.SetBool(ClimbWaitHash, true);


        }
       

        //점프 방향에 추가적인 upAxis 추가
        jumpDirection = (jumpDirection + upAxis).normalized;

        //점프 프레임 초기화
        stepsSinceLastJump = 0;
        //점프 횟수 추가
        jumpPhase += 1;

        //Root(-2*g*h) = 점프 속도
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

        //이전 속도
        float alignSpeed = Vector3.Dot(velocity, jumpDirection);

        //계산전에 항상 이전 속도를 뺀다.
        //만약 이전 속도가 점프속도보다 빠를경우를 대비하여 0으로 빠르게 떨어질 경우 잠깐 멈추게 만든다.
        if (alignSpeed > 0f)
        {   
            jumpSpeed = Mathf.Max(jumpSpeed - alignSpeed, 0f);
        }

        //점프 적용
        velocity += jumpDirection * jumpSpeed;
    }

    IEnumerator fireDelayRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        if(stepsSinceLastClimb>climbHelpTime) canFire = true;
    }
    IEnumerator climbDelayRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        isJump = false;
        animator.SetBool(ClimbWaitHash, false);
    }


    void EvaluateCollision(Collision collision)
    {
        //여기서 현재 평면이 이동 가능한 절벽인지 체크

        int layer = collision.gameObject.layer;

        //현재 충돌한 오브젝트의 최소 각도를 가져온다.
        float minDot = GetMinDot(layer);

        //모든 contact를 검사하여 일정 각도 이상의 평면을 모두 저장한다.
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 target = collision.GetContact(i).point;
            Vector3 dirToTarget = (target - beforePosition);
            dirToTarget.y = 0f;
            dirToTarget.Normalize();

            float angle = Vector3.Angle(animator.transform.forward, dirToTarget);

            //접촉 표면을 가져온다.
            Vector3 normal = collision.GetContact(i).normal;
            //접촉 표면의 각도를 가져온다.(내적)
            float upDot = Vector3.Dot(upAxis, normal);

            //색칠된 벽을 확인
            bool isColoredWall = false;

            /*  Debug.Log((stepsSinceLastGrounded <= 1 && angle <= viewAngle * 0.5f));
              Debug.Log(stepsSinceLastGrounded + "/" + angle);
              Debug.Log(stepsSinceLastClimb);*/

            /*Debug.Log(angle <= viewAngle * 0.5f);
            Debug.Log(angle);
            Debug.Log(viewAngle * 0.5f);
            Debug.Log((stepsSinceLastClimb <= climbHelpTime));
            Debug.Log(stepsSinceLastClimb);
            Debug.Log(climbHelpTime);*/
            //색칠 리스트에 추가 되어있을 경우만 검사
            if (((angle <= viewAngle * 0.5f) || (stepsSinceLastClimb <= climbHelpTime)) && ToolFunc<PaintTarget>.ConatainsCollision(GunStateController.paintList, collision))
            {
                isColoredWall = CheckPaintedWall(collision.contacts[i], normal);
                //Debug.Log(isColoredWall);
            }
            
            //cos에서 y값은 1->-1로 가므로 높을수록 각도는 낮은 각도
            //만약 접촉 표면의 각도가 최소 각도를 만족할 경우
            if (upDot >= minDot)
            {
                //땅에 있다고 판단
                groundContactCount += 1;
                allContactCount += 1;
                contactNormal += normal;

                //collision의 rigidbody를 연결
                connectedBody = collision.rigidbody;
            }
            else
            {
                /*Debug.Log(isColoredWall);
                Debug.Log(upDot);
                Debug.Log((climbMask & (1 << layer)) != 0);
                Debug.Log(upDot >= minClimbDotProduct);
                Debug.Log(!isJump);*/

                //색칠된 곳이고 등산 가능한 각도+등산 가능한 레이어 일 경우
                if (isColoredWall && upDot >= minClimbDotProduct && (climbMask & (1 << layer)) != 0 && !isJump)
                {
                    climbContactCount += 1;
                    allContactCount += 1;
                    climbNormal += normal;
                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
                else
                {
                    steepContactCount += 1;
                    allContactCount += 1;
                    steepNormal += normal;
                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }

                }
            }

        }

        if (OnClimb)
        {
            climbObject.Enqueue(collision.gameObject);
            if (climbObject.Count > 4)
            {
                climbObject.Dequeue().tag = "Untagged";
            }
            collision.gameObject.tag = "ClimbObj";
        }

        /*Debug.Log(collision.gameObject.name);
        Debug.Log(allContactCount);
        Debug.Log(groundContactCount);
        Debug.Log(climbContactCount);
        Debug.Log(steepContactCount);
        if(allContactCount>2)
            Debug.Break();*/
    }

    private bool CheckPaintedWall(ContactPoint point, Vector3 normal)
    {
        //접촉 표면의 색을 가져와서 판단한다.
        //하나라도 색이 다를 경우 접착제 붙인 상태
        Ray ray = new Ray(point.point + normal, -normal);
        Ray centerRay = new Ray(transform.position, -normal);
        int channel = PaintTarget.RayChannel(ray, centerRay, 1.5f, colorCheckLayer);

        bool isColoredWall = channel == 0;
        desireClimb |= isColoredWall;
        return isColoredWall;
    }
/*    private bool CheckPaintedWall(ContactPoint point, Vector3 normal, out int channel)
    {
        //접촉 표면의 색을 가져와서 판단한다.
        //하나라도 색이 다를 경우 접착제 붙인 상태
        Ray ray = new Ray(point.point + normal, -normal);
        channel = PaintTarget.RayChannel(ray, 1.5f, colorCheckLayer);
        bool isColoredWall = channel == 0;
        desireClimb |= isColoredWall;
        return isColoredWall;
    }*/

    private void ClearState()
    {
        //매 프레임 마다 초기화한다.(FixedUpdate의 마지막)
        //[프레임 시작]->행동(fixedUpdate)->초기화(fixedUpdate)->충돌처리(oncollision)->입력(update)->[프레임 시작]->행동(fixedUpdate)->초기화(fixedUpdate)->...
        //땅
        groundContactCount = 0;
        contactNormal = Vector3.zero;
        //가파른 경사
        steepContactCount = 0;
        steepNormal = Vector3.zero;
        //등산가능
        climbContactCount = 0;
        previousClimbNormal = climbNormal;
        climbNormal = Vector3.zero;

        allContactCount = 0;

        connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;

        desireClimb = false;
    }

    public enum PlayerState
    {
        ground, climb ,air
    }

    private void UpdateState()
    {

        //마지막 그라운드에서 몇 프레임이 지났는지 저장하기 위한 변수
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        stepsSinceLastClimb += 1;
        stepsSinceLastMultiple += 1;

        //플레이어 속도 초기화
        velocity = rb.velocity;

        //확실히 땅이거나, 땅에 붙어있을 경우, 등산 상태인 경우, 경사면이 중복되어 있을 경우
        //이 경우 이후 자동적으로  OnGround가 true가 된다.
        if (CheckClimbing() || OnGround || SnapToGround() || CheckSteepContacts())
        {
            //마지막 그라운드 프레임 초기화
            stepsSinceLastGrounded = 0;

            if (stepsSinceLastJump > 2)
            {
                //점프 횟수 초기화
                jumpPhase = 0;
            }

            //만약 땅에 1개 이상 밟고 잇을 경우 단위 벡터로 contact 위치 일반화
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }

        //공중에 있을 경우
        //이후 자동적으로 OnGround가 false가 된다.
        else
        {
            groundContactCount = 0;
            //접촉 방향 Vector3.up(평지)
            contactNormal = upAxis;
        }

        //만약 움직이는 플랫폼을 쓸경우
        if (connectedBody)
        {
            //키네마틱이나, 플레이어의 mass보다 단위가 큰 플랫폼일 경우
            if (connectedBody.isKinematic || connectedBody.mass >= rb.mass)
            {
                //플랫폼의 물리를 플레이어에 적용
                //이번에는 사용하지않는다.
                //UpdateConnectionState();
            }
        }
        
        //마지막 climb에서 2프레임 지나기전일 경우
        if (stepsSinceLastClimb == 1)
        {
            if (fireDelay != null) StopCoroutine(fireDelay);
            canFire = false;
            fireDelay = StartCoroutine(fireDelayRoutine(fireDelayTime));

        }

        //온전히 땅에 붙을 경우
        else if (!OnClimb && OnGround)
        {
            canJump = true;
            canFire = true;
            isJump = false;
        }

        if (stepsSinceLastClimb <= climbHelpTime && !canJump)
        {
            climbContactCount += 1;
            contactNormal = lastClimbNormal;
        }

        //등산 상태, 점프 상태, 뒤로 이동을 제외한 경우 달리기 가능
        if (desireRun && !OnClimb && stepsSinceLastGrounded == 0 && input.z >= 0)
        {
            moveMultiple = runMultiple;
        }
        else
        {
            moveMultiple = walkMultiple;
        }

        if (multipleState)
        {
            stepsSinceLastMultiple = 0;
        }

        // 지상<->등산 애니메이션 결정
        //animator.SetBool(ClimbHash, !multipleState && OnClimb);
        // bool condition =  stepsSinceLastClimb <= 1;

        // frame = condition ? frame+1 : 0;

        /*Debug.Log(stepsSinceLastClimb);
        Debug.Log(multipleState);
        Debug.Log(OnClimb);
        Debug.Log(desireClimb);*/

        animator.SetBool(ClimbHash, OnClimb);
        animator.SetBool(GroundHash, !OnClimb && stepsSinceLastGrounded == 0);
    }

    /*//연결된 플랫폼이 있을 경우
    void UpdateConnectionState()
    {
        //해당 플랫폼의 속도를 플레이어의 속도에 적용하기 위해 이를 가져온다.
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = rb.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);

    }*/

    private void UpdateInputState()
    {
        input.x = reader.Direction.x;
        input.z = reader.Direction.y;
        input.y = 0;

        //입력값 변환
        input = Vector3.ClampMagnitude(input, 1);

        desireJump |= reader.JumpKey && (OnClimb || OnGround);
        desireJump &= canJump;

        multipleState = OnGround && OnClimb;

        if (input.magnitude == 0 && !multipleState && OnGround && CanReload)
        {
            idleTime += Time.deltaTime;
        }
        else
        {
            idleTime = 0;
        }
    }

    //플레이어의 축을 변경한다(앞뒤/좌우)
    void UpdateAxis()
    {
        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }
    }

    //애니메이션의 파라미터를 추가한다.
    private void UpdateAnimationParameter()
    {
        animator.SetFloat(IdleTimeHash, idleTime);
        animator.SetFloat(VelocityXHash, input.x * (desireRun ? 2 : 1));
        animator.SetFloat(VelocityYHash, input.z * (desireRun ? 2 : 1));
    }

    //땅을 벗어날 경우 특정상황에서 땅에 붙이기 위한 기능
    bool SnapToGround()
    {
        //다음의 경우 그냥 땅을 벗어난다.
        //1. 만약 땅에서 벗어나고 2프레임 이상 진행 됐을경우(1프레임까지는 땅에 있다고 가정)
        //2. 점프 실행한지 2프레임이하일 경우(점프 보장)
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            //Debug.Log(stepsSinceLastGrounded+"/"+ stepsSinceLastJump);
            return false;
        }

        Vector3 velocityNonY = velocity;
        velocityNonY.y = 0;
        float speed = velocityNonY.magnitude;
        //2. 만약 현재 속도가 일정 속도 이상이라면
        if (speed > maxSnapSpeed)
        {
            //Debug.Log(speed);
            return false;
        }

        //3. 땅으로 레이를 쐈을 때 hit되지않을 경우
        if (!Physics.Raycast(rb.position, -upAxis, out groundHit, probeDistance, groundMask))
        {
            //Debug.Log("레이캐스트");
            return false;
        }

        float upDot = Vector3.Dot(upAxis, groundHit.normal);
        //4. hit한 곳이 유효하지 않은 경사일경우(maxSlopeAngle을 넘길경우)
        if (upDot < GetMinDot(groundHit.collider.gameObject.layer))
        {
            return false;
        }

        //Debug.Log("통과");
        //그 외에는 땅에 붙어있다고 가정한다.
        groundContactCount = 1;
        contactNormal = groundHit.normal;
        float dot = Vector3.Dot(velocity, groundHit.normal);
        //만약 velocity가 바닥을 향하고 있다면 더 느려지는 경우가 있기 때문에 이 경우를 제외한다.
        if (dot > 0f)
        {
            //velocity를 사영하여 각도를 바꾸고 magnitude를 곱해서 동일한 힘을 준다.
            velocity = (velocity - groundHit.normal * dot).normalized * speed;
        }

        //연결 플랫폼을 변경
        connectedBody = groundHit.rigidbody;
        return true;
    }


    bool CheckSteepContacts()
    {
        //만약 가파른 경사를 2개 이상 얻고있다면
        //가상의 경사를 만든다.
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;

                return true;
            }
        }
        return false;
    }


    bool CheckClimbing()
    {
        if (OnClimb)
        {
            //만약 등산 경사를 2개 이상 얻고있다면
            //가상의 경사를 만든다.
            if (climbContactCount > 1)
            {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                if(upDot >= minGroundDotProduct)
                {
                    climbNormal = lastClimbNormal;
                }
            }
            groundContactCount = 1;
            stepsSinceLastClimb = 0;
            lastClimbNormal = climbNormal;
            contactNormal = climbNormal;
            return true;
        }

        return false;
    }

    /*public void ConnectCameraAndGrabObj(Transform grabObj)
    {
        cameraManager.SetGrabObject(grabObj);
    }
    public void DisconnectCameraAndGrabObj()
    {
        cameraManager.ClearGrabObject();
    }*/

    public Rigidbody GetConnectRigidBody()
    {
        return connectedBody;
    }

    public Vector3 GetClimbNormal()
    {
        return climbNormal;
    }

    Vector3 previousClimbNormal;
    public Vector3 GetPreviousClimbNormal()
    {
        return previousClimbNormal;
    }

    public Vector2 GetMoveDirection()
    {
        return reader.Direction.normalized;
    }

    float GetMinDot(int layer)
    {
        //n만큼 비트 이동한 것과 비교하여 1이 검출되지않는다면 지형 / 그외 오브젝트
        return (objectMask & (1 << layer)) == 0 ? minGroundDotProduct : minObjectDotProduct;
    }

    /*Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        //vector를 normal의 각도 만큼 Projection 한다.
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }*/


    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        //방향 - 법선*내적 -> 방향으로 적용되는 가감된 힘의 크기(사영벡터)
        return (direction - normal*Vector3.Dot(direction, normal)).normalized;
    }



    public static void SwitchCameraLock(bool check)
    {
        stopState = check;
        if (stopState)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;

        }
    }

    #region 관리자모드
    public void SetValue(SliderType type, float value)
    {
        switch (type)
        {
            case SliderType.Move: maxMoveSpeed = value; break;
            case SliderType.Jump: jumpHeight = value; break;
            case SliderType.Gravity: gravityMultiple = value; break;
            case SliderType.OneShot: (gunController.GetGunMode((int)GunMode.Paint) as PaintGun).FirstShot = (int)value; break;
            case SliderType.repeatShot: (gunController.GetGunMode((int)GunMode.Paint) as PaintGun).AutoShot = (int)value; break;
            case SliderType.Grab: (gunController.GetGunMode((int)GunMode.Grab) as GrabGun).GrabShot = (int)value; break;
            case SliderType.Range: gunController.Range = value; break;
            case SliderType.Capacity: gunController.MaxAmmo = (int)value; break;
            case SliderType.ObjGravity: gunController.gravityDecrement = value; break;
            case SliderType.Speed: gunController.speed = value; break;
        }
    }
    public float GetValue(SliderType type)
    {
        switch (type)
        {
            case SliderType.Move: return maxMoveSpeed;
            case SliderType.Jump: return jumpHeight;
            case SliderType.Gravity: return gravityMultiple;
            case SliderType.OneShot: return (float)(gunController.GetGunMode((int)GunMode.Paint) as PaintGun).FirstShot;
            case SliderType.repeatShot: return (float)(gunController.GetGunMode((int)GunMode.Paint) as PaintGun).AutoShot;
            case SliderType.Grab: return (float)(gunController.GetGunMode((int)GunMode.Grab) as GrabGun).GrabShot;
            case SliderType.Range: return gunController.Range;
            case SliderType.Capacity: return gunController.MaxAmmo;
            case SliderType.ObjGravity: return (float)gunController.gravityDecrement;
            case SliderType.Speed: return (float)gunController.speed;

        }
        return -1;
    }
    #endregion


    #region 애니메이션 이벤트


    public void PlayWalkSound()
    {
        if (!OnGround)
        {
            return;
        }
        int id = desireClimb ? (int)PlayerSoundList.GlueWalk : (int)PlayerSoundList.DefaultWalk;
        SoundManager.instance.PlaySound(GroupList.Player, id);
    }
    public void PlayClimbSound()
    {
        if (!OnClimb)
        {
            return;
        }

        int id =  (int)PlayerSoundList.GlueWalk;
        SoundManager.instance.PlaySound(GroupList.Player, id);
    }
    public void PlayReloadAnimation()
    {
        playingReloadAnimation = true;
        aimRig.weight = 0;
        animator.SetTrigger(ReloadTriggerHash);
    }
    public void EndReloadAnimation()
    {
        playingReloadAnimation = false;
        aimRig.weight = 1;
    }

    public bool onUnequip = false;

    public void PlayUnEquipAnimation()
    {
        handGunRender.enabled = true;
        frontGunRender.enabled = false;
        backGunRender.enabled = (false);

       // playingEquipAnimation = true;
        aimRig.weight = 0;
        animator.SetTrigger(EquipTriggerHash);
    }
    public void EndUnEquipAnimation()
    {
        if (!OnClimb) return;
        handGunRender.enabled = false;
        frontGunRender.enabled = false;
        backGunRender.enabled = (true);

       // playingEquipAnimation = false;
        aimRig.weight = 1;

    }
    public void PlayEquipAnimation()
    {
        /*Debug.LogWarning(stepsSinceLastGrounded + " " + stepsSinceLastClimb);

        Debug.LogWarning(multipleState + " " + allContactCount);
        Debug.LogWarning(OnGround+" "+groundContactCount);
        Debug.LogWarning(OnClimb + " " + climbContactCount);
        Debug.LogWarning(OnSteep + " " + steepContactCount);*/
        //Debug.Break();
        handGunRender.enabled = false;
        frontGunRender.enabled = true;
        backGunRender.enabled = (false);

        aimRig.weight = 1;
        //handGunRender.enabled = true;
        //frontGunRender.enabled = false;
        //backGunRender.enabled = (false);

        //playingEquipAnimation = true;
        //aimRig.weight = 0;
        //animator.SetTrigger(EquipTriggerHash);
    }
    public void EndEquipAnimation()
    {
        handGunRender.enabled = false;
        frontGunRender.enabled = true;
        backGunRender.enabled = (false);

       // playingEquipAnimation = false;
        aimRig.weight = 1;
    }
    public void StartClimbAnimation()
    {
        /* Debug.LogAssertion(stepsSinceLastGrounded + " " + stepsSinceLastClimb);

         Debug.LogAssertion(multipleState + " " + allContactCount);
         Debug.LogAssertion(OnGround + " " + groundContactCount);
         Debug.LogAssertion(OnClimb + " " + climbContactCount);
         Debug.LogAssertion(OnSteep + " " + steepContactCount);*/

        animator.SetBool(EquipStateHash, true);
        canJump = false;
    }
    public void EndClimbAnimation()
    {
        canJump = true;
    }

    #endregion

    #region 바인딩 함수
    [Header("인풋 시스템 리더")] //인풋 시스템 리더
    [SerializeField]
    InputReader reader;

    private void BindHandler()
    {
        reader.Fire += PressFire;
        reader.Grab += PressGrab;
        reader.Run += ToggleRun;
    }

    bool desireFire;
    bool desireGrab;

    public void PressFire(float input)
    {
        if (input == 1)
        {
            desireFire = true;
        }
        else
        {
            desireFire = false;
        }
    }
    public void PressGrab(float input)
    {
        if (input == 1)
        {
            desireGrab = true;
        }
        else
        {
            desireGrab = false;
        }
    }

    void ToggleRun()
    {
        desireRun = !desireRun;
    }


    #endregion

}
