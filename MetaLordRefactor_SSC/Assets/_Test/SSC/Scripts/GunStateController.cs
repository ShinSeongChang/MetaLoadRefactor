using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
public enum GunState { READY, EMPTY, RELOADING }

public class GunStateController : MonoBehaviour
{

    // 심볼릭 상수
    #region Symbol

    // 크로스헤어 이미지 교체용 배열 상수
    enum CrossHair { ABLE, UNABLE, LOCK}

    // 도구 UI 이미지 배열 상수
    const int AmmoGaugeIdx = 2;
    const int WarningImgIdx = 4;
    #endregion

    GunBase[] mode = new GunBase[2];
    GunBase currentMode;
    public GunBase CurrentMode { get { return currentMode; } private set { currentMode = value; } }

    public void Shoot(GunMode _mode)
    {
        switch(_mode)
        {
            case GunMode.Paint:
                mode[(int)GunMode.Paint].ShootGun();
                break;
            case GunMode.Grab:                
                mode[(int)GunMode.Grab].ShootGun();
                break;
        }
    }

    public GunBase GetGunMode(int index)
    {
        return mode[index];
    }
    
    public CameraController cameraController;
    [SerializeField] private Controller_Physics player;
    [SerializeField] private Image crossHair;
    [SerializeField] private GameObject GunUi;
    [SerializeField] private Sprite[] crossHairSprite = new Sprite[3];

    public Rigidbody GetConnectObject()
    {
        return player.GetConnectRigidBody();
    }

    public Transform startPos;
    public InputReader reader;    
    public Transform checkPos;
    public Transform pickupPoint;
    public Transform GunHolderHand;
    public Transform grabCorrectPoint;
    public LineRenderer grabLine;
    public Transform AimTarget;
    public LayerMask gunLayer;
    public Transform muzzleStart;

    [Header("도구 UI")]        
    [SerializeField] private AnimationCurve reloadCurve;

    [Header("도구 스텟")]
    [Range(1, 50)]
    public float ClimbeSize = 10f;
    [Range(1, 50)]
    public float BondSize = 20f;
    [Range(0, 500)]
    public float range; 
    [SerializeField, Range(0, 10)]
    float cameraMinRange;
    [SerializeField, Range(0, 10)]
    float playerMinRange;
    [SerializeField, Range(0,500)]
    float grabRange = 20f;
    [Range(10, 1000)]
    public float speed;
    public float GrabRange { get { return grabRange; } private set { grabRange = GrabRange; } }
    [SerializeField, Range(0, 10)]
    float reloadTime = 3f;
    [Range(0.1f, 3f)]
    public float AutoInitTime = 1f;
    [Range(0.01f, 1)]
    public float fireRate = 0.1f;
    [SerializeField, Range(0, 1000)]
    private int maxAmmo = 200;   
    
    public float Range { get { return range; } set { range = value; } }

    [HideInInspector]
    public Vector3 startPoint;
    [HideInInspector]
    public RaycastHit hit;
    [HideInInspector]
    public float lerpTime= .1f;
    [HideInInspector]
    public int checkAmmo;

    [HideInInspector]
    public bool isShootingState = false;
    [HideInInspector]
    public bool checkSuccessRay = false;
    [HideInInspector] 
    public bool onGrab = false;
    [HideInInspector]
    public bool minDistance = false;

    bool usedGrabGun = false;

    public bool UsedGrabGun()
    {
        return usedGrabGun;
    }

    bool usedBondGun = false;
    int maxUpgrade = 470;

    [HideInInspector] public NpcBase targetNpc = null;
    [HideInInspector] public GunState state;
    [HideInInspector] public static HashSet<PaintTarget> paintList;
    [HideInInspector] public static HashSet<MovedObject> bondList;
    [HideInInspector] public static HashSet<NpcBase> npcList;
    [HideInInspector] public static HashSet<CatchObject> catchList;

    Image[] gunImage;
    TextMeshProUGUI gunText;
    RectTransform textRect;
    Coroutine textFadeOut;
    Coroutine crosshairFadeOut;
    Color originColor;

    public bool usedGravity = false;
    public float gravityDecrement = 0.5f;

    // 프로퍼티 모음
    #region Property
    public bool CanFire
    {
        get
        {
            if (state != GunState.READY)
            {
                return false;
            }

            return player.CanFire;
        }
        private set { }
    }

    public bool CanReload
    {
        get
        {            
            if (onGrab)
            {
                return !onGrab;
            }

            return player.CanReload;
        }
        private set { }
    }

    public int MaxAmmo
    {
        get { return maxAmmo; }
        set { maxAmmo = value; }
    }

    private int ammo;
    public int Ammo
    {
        get { return ammo; }
        private set { ammo = value; }
    }

    #endregion

    // CallBack 함수
    #region CallBack
    private void Awake()
    {
        lerpTime = 0.2f;        
        gunImage = GunUi.transform.GetChild(0).GetComponentsInChildren<Image>();
        gunText = GunUi.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        textRect = gunText.GetComponent<RectTransform>();
        //originColor = GunUi.transform.GetChild(0).transform.

        mode[(int)GunMode.Paint] = transform.GetComponent<PaintGun>();
        mode[(int)GunMode.Grab] = transform.GetComponent<GrabGun>();        

        for (int i = 0; i < mode.Length; i++)
        {
            mode[i].hideFlags = HideFlags.HideInInspector;            
        }

        currentMode = mode[(int)GunMode.Paint];
        SwapLayer();

        paintList = new HashSet<PaintTarget>();
        bondList = new HashSet<MovedObject>();
        npcList = new HashSet<NpcBase>();
        catchList = new HashSet<CatchObject>();

        if (!cameraController)
            cameraController = Camera.main.GetComponentInParent<CameraController>();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.coinEvents.onUnlockGunMode += GunModeUnlock;
        GameEventsManager.instance.coinEvents.onUpgradeGun += UpgradeGun;
    }

    void Start()
    {
        UpdateState(MaxAmmo, GunState.READY);        
    }

    private void Update()
    {
        //레이캐스트 업데이트
        UpdateRaycast();
    }

    private void OnDisable()
    {
        GameEventsManager.instance.coinEvents.onUnlockGunMode -= GunModeUnlock;
        GameEventsManager.instance.coinEvents.onUpgradeGun -= UpgradeGun;
    }
    #endregion

    void SwapLayer()
    {
        gunLayer = currentMode.myLayer;
    }
    
    private void UpdateRaycast()
    {
        Vector3 startCameraPos = GetOriginPos();
        Vector3 startPlayerPos = checkPos.position;
        Vector3 direction = CheckDir();

        Vector3 endPos = startCameraPos + Camera.main.transform.forward * range;

        Ray normalRay = new Ray(Camera.main.transform.position, direction);
        Ray checkRay = new Ray(startPlayerPos, direction);
        Ray defaultRay = new Ray(startPlayerPos, (endPos - startPlayerPos).normalized);

        float distance = Vector3.Distance(Camera.main.transform.position, GetOriginPos());
        
        // 일반적인 상황의 사격
        if (Physics.Raycast(normalRay, out hit, range, gunLayer))
        {
            float distanceCameraToHit = Vector3.Distance(startCameraPos, hit.point);
            float distancePlayerToHit = Vector3.Distance(startPlayerPos, hit.point);

            //카메라->끝점 range 이하 경우
            //if (hit.distance <= distance)
            //{
            //    // TODO : 목표지점이 너무 가까운 상태일때 UI 변경을 생각해야함
            //    checkSuccessRay = false;
            //    crossHair.color = Color.red;
            //    player.SetAimPosition(player.transform.position+Vector3.up*100);

            //    if (textFadeOut != null)
            //    {
            //        StopCoroutine(textFadeOut);
            //    }

            //    textFadeOut = StartCoroutine(IEFadeOutText("ㅁㅁㅁㅁㅁㅁㅁㅁ"));
            //    return;
            //}

            if (distanceCameraToHit <= cameraMinRange)
            {
                if (Physics.Raycast(defaultRay, out hit, distancePlayerToHit, gunLayer))
                {
                    player.SetAimPosition(hit.point+defaultRay.direction*10);
                    // Debug.Log("4");
                    // Debug.Log("플레이어->디폴트히트포인트");
                    startPoint = startPlayerPos;
                    checkSuccessRay = true;
                    minDistance = false;                    
                    //crossHair.color = CanFire && currentMode.CanFireAmmoCount() ? Color.green : Color.red;

                    //if (!currentMode.CanFireAmmoCount() && !onGrab && !currentMode.fireStart)
                    //{
                    //    //Debug.Log("1");
                    //    gunImage[WarningImgIdx].GetComponent<UiFadeOut>().InitFadeOut();
                    //    ChangedCrossHair();
                    //}
                   // Debug.Log("?");

                    //Vector3 anchor = Camera.main.WorldToScreenPoint(hit.point);
                    //if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)crossHair.transform.parent, anchor, null, out Vector2 localPoint))
                    {
                        //crossHair.rectTransform.anchoredPosition = localPoint;
                    }

                    if (crossHair.sprite == crossHairSprite[(int)CrossHair.LOCK])
                    {
                        if (crosshairFadeOut != null)
                        {                            
                            //StopCoroutine(crosshairFadeOut);
                            return;
                        }

                        crosshairFadeOut = StartCoroutine(IEFadeOutCrosshair());
                    }
                    else
                    {
                        ChangedCrossHair();
                    }

                    //ChangedCrossHair();
                    //Debug.Log(1);
                    return;
                }
            }


            //카메라->끝점 range 이상 경우
            else
            {
                player.SetAimPosition(hit.point + defaultRay.direction * 10);
                startPoint = startCameraPos;
                checkSuccessRay = true;
                minDistance = false;
                //crossHair.color = CanFire && currentMode.CanFireAmmoCount() ? Color.green : Color.red;

                //if (!currentMode.CanFireAmmoCount() && !onGrab && !currentMode.fireStart)
                //{
                //    //Debug.Log("2");
                //    gunImage[WarningImgIdx].GetComponent<UiFadeOut>().InitFadeOut();
                //    ChangedCrossHair();
                //}

                //if (crossHair.rectTransform.anchoredPosition != Vector2.zero)
                {
                   // crossHair.rectTransform.anchoredPosition = Vector2.zero;
                }

                if (crossHair.sprite == crossHairSprite[(int)CrossHair.LOCK])
                {
                    //if (crosshairFadeOut != null)
                    //{
                    //        Debug.Log(2);
                    //    //StopCoroutine(crosshairFadeOut);
                    //    return;
                    //}

                    //crosshairFadeOut = StartCoroutine(IEFadeOutCrosshair());
                }
                else
                {
                    ChangedCrossHair();
                }

                //ChangedCrossHair();                              
                //Debug.Log(2);
                return;
            }
        }

        player.SetAimPosition(Vector3.zero);
        checkSuccessRay = false;

        // 현재 상태에 맞게 크로스헤어 이미지 업데이트 해주는 조건문
        if (!onGrab && !currentMode.CanFireAmmoCount() && !currentMode.fireStart)
        {
            //Debug.Log(3);
            // 내가 총알 잔량이 부족하면서, 그랩하고있는 상태가 아닐 때 재장전 필요 효과 발생

            if (crossHair.sprite == crossHairSprite[(int)CrossHair.LOCK])
            {
                //if (crosshairFadeOut != null)
                //{
                //            Debug.Log(3);
                //    //StopCoroutine(crosshairFadeOut);
                //    return;
                //}

                //crosshairFadeOut = StartCoroutine(IEFadeOutCrosshair());
            }
            else
            {
                ChangedCrossHair();
            }

            //gunImage[WarningImgIdx].GetComponent<UiFadeOut>().InitFadeOut();
            //ChangedCrossHair();
        }
        else
        {
            //Debug.Log(3);
            if (crossHair.sprite == crossHairSprite[(int)CrossHair.LOCK])
            {
                //if (crosshairFadeOut != null)
                //{
                //            Debug.Log(4);
                //    //StopCoroutine(crosshairFadeOut);
                //    return;
                //}

                //crosshairFadeOut = StartCoroutine(IEFadeOutCrosshair());
            }
            else
            {
                ChangedCrossHair();
            }
            // 디폴트 이미지
            //crossHair.color = Color.green;
            //crossHair.sprite = crossHairSprite[(int)CrossHair.ABLE];
        }

    }

    /// <summary>
    /// 카메라와 플레이어의 축을 동일선상에 놓아주는 메소드
    /// </summary>
    /// <returns>카메라의 정면 방향 + 플레이어의 수직선상 </returns>
    Vector3 GetOriginPos()
    {
        Vector3 origin = Vector3.zero;

        origin = Camera.main.transform.position 
            + Camera.main.transform.forward 
            * Vector3.Distance(Camera.main.transform.position, startPos.position)
            ;

        return origin;
    }
    /// <summary>
    /// GetOriginPos()를 통해 얻은 축에서 카메라의 Ray방향을 얻어낼 메소드
    /// </summary>
    /// <returns></returns>
    Vector3 CheckDir()
    { 
        Vector3 dir = Vector3.zero;
        dir = Camera.main.transform.forward +
            Camera.main.transform.TransformDirection(dir);

        return dir;
    }

    public void UpdateState(int ammoValue, GunState updatedState)
    {
        if (state != GunState.RELOADING)
        {
            Ammo = ammoValue;
        }
        checkAmmo = ammoValue;
        state = updatedState;
        gunImage[AmmoGaugeIdx].fillAmount = (float)ammoValue / (float)maxUpgrade;
    }

    public void UpdateState(int ammoValue)
    {
        if (state != GunState.RELOADING)
        {
            Ammo = ammoValue;        
        }
        gunImage[AmmoGaugeIdx].fillAmount = (float)ammoValue / (float)maxUpgrade;    
        
    }

    public void Reloading()
    {
        if (!CanReload || state == GunState.RELOADING || Ammo == MaxAmmo)
        {
            return;
        }

        player.PlayReloadAnimation();
        int id = (int)GunSoundList.Reload;
        SoundManager.instance.PlaySound(GroupList.Gun, id);

        state = GunState.RELOADING;        
        ClearNpcList();
        ClearAllPaint();
        ClearBondList();
        ClearCatchList();
        currentMode.StopLerpGaguge();
        StopAllCoroutines();
        StartCoroutine(ReloadingAmmo());
    }

    IEnumerator ReloadingAmmo()
    {
        crossHair.gameObject.SetActive(false);
        
        textRect.position = crossHair.rectTransform.position;
        InitColorText();

        float currentAmmo = Ammo;
        Ammo = MaxAmmo;

        float timeCheck = 0f;
        float t = 0f;

        while (timeCheck <= reloadTime)
        {
            timeCheck += Time.deltaTime;
            t = timeCheck / reloadTime;

            int ammoValue = (int)Mathf.Lerp(currentAmmo, maxAmmo, reloadCurve.Evaluate(t));
            UpdateState(ammoValue);
            textRect.position = crossHair.rectTransform.position;
            UpdateText(timeCheck);

            yield return null;

        }

        checkAmmo = maxAmmo;
        UpdateState(maxAmmo, GunState.READY);
        gunText.text = "";
        crossHair.sprite = crossHairSprite[(int)CrossHair.ABLE];
        crossHair.gameObject.SetActive(true);
    }

    // 오버로딩 메소드 모음
    #region 오버로딩 메소드 모음
    public static void AddList(MovedObject obj)
    {
        bondList.Add(obj);        
    }

    public static void AddList(PaintTarget obj)
    {
        paintList.Add(obj);
    }

    public static void AddList(NpcBase obj)
    {
        npcList.Add(obj);
    }

    public static void AddList(CatchObject obj)
    {        
        catchList.Add(obj);        
    }

    void ClearBondList()
    {        

        foreach (var paint in bondList)
        {            
            paint.ClearTrigger();
        }

        bondList.Clear();
    }

    void ClearNpcList()
    {
        foreach (var paint in npcList)
        {
            paint.ChangedState(npcState.normal);
        }

        npcList.Clear();
    }

    void ClearCatchList()
    {
        // 저장한 HashSet 만큼의 상위 오브젝트 탐색
        foreach (var obj in catchList)
        {
            // 탐색한 상위 오브젝트마다 가지고 있는 자식오브젝트 체크
            GameObject[] nullObj = new GameObject[obj.transform.childCount];            


            // 자식 오브젝트들 parent 해제
            for(int i = 0; i < nullObj.Length; i++)
            {
                nullObj[i] = obj.gameObject.transform.GetChild(i).gameObject;

                if (nullObj[i].GetComponent<MovedObject>() == null)
                {
                    Destroy(nullObj[i]);
                    continue;
                }

                nullObj[i].GetComponent<MovedObject>().CelarBond();
                nullObj[i].gameObject.layer = LayerMask.NameToLayer("MovedObject");


            }

            for (int i = 0; i < nullObj.Length; i++)
            {                
                nullObj[i].transform.parent = null;
            }

            // 이후 생성되었던 상위 오브젝트 파괴
            Destroy(obj.gameObject);
        }

        catchList.Clear();        
    }

    #endregion

    public void GunModeUnlock(GunMode gunMode)
    {
        switch (gunMode)
        {
            case GunMode.Paint:
                break;
            case GunMode.Grab:
                usedGrabGun = true;                
                break;
            case GunMode.Bond:
                usedBondGun = true;
                break;
        }
    }

    public void UpgradeGun(UpgradeCategory _category, int _value)
    {
        switch (_category)
        {
            case UpgradeCategory.Range:
                range += _value;
                break;
            case UpgradeCategory.Amount:
                checkAmmo += _value;
                MaxAmmo += _value;                
                UpdateState(Ammo + _value);
                break;
        }
    }

    public void SwapGunMode(GunMode changeMode)
    {
        // 각종 모드 스왑을 막아야 하는 상황 1. 리로딩중, 그랩모드중 그랩일때, 이미 내가 활성화한 모드일때
        if (state == GunState.RELOADING || onGrab || currentMode == mode[(int)changeMode])
        {
            return;
        }

        // 해금모드들 해금전에 스왑 방지
        if((changeMode == GunMode.Grab && !usedGrabGun) ||
            (changeMode == GunMode.Bond && !usedBondGun))
        {
            return;
        }

        int id = (int)GunSoundList.ChangeMod;
        SoundManager.instance.PlaySound(GroupList.Gun, id);

        //SwapTest(changeMode);
        currentMode = mode[(int)changeMode];
        SwapLayer();
    }

    public void ClearAllPaint()
    {
        foreach (PaintTarget target in paintList)
        {
            target.ClearPaint();
            target.splatTexPick = target.originTex;
        }

        paintList.Clear();
    }

    void UpdateText(float time)
    {
        if(time >=  reloadTime * 0.66f)
        {
            gunText.text = "충전중...";
        }
        else if(time >= reloadTime * 0.33f)
        {

            gunText.text = "충전중..";
        }
        else
        {
            gunText.text = "충전중.";
        }
    }

    public void ChangedCrossHair()
    {
        if(crosshairFadeOut != null)
        {
            StopCoroutine(crosshairFadeOut);
        }

        Color tempColor = crossHair.color;
        tempColor.a = 1f;
        crossHair.color = tempColor;

        if (currentMode.CanFireAmmoCount())
        {            
            crossHair.sprite = crossHairSprite[(int)CrossHair.ABLE];
            crossHair.color = Color.green;                                
        }
        else
        {
            if (onGrab) return;

            if(textFadeOut != null)
            {
                StopCoroutine(textFadeOut);
            }

            InitColorText();                        
            Vector3 tempVec = crossHair.rectTransform.position;
            tempVec.y += 120f;
            textRect.position = tempVec;
            gunText.text = "접착제 부족!";
            crossHair.sprite = crossHairSprite[(int)CrossHair.UNABLE];
            crossHair.color = Color.red;
        }
    }

    IEnumerator IEFadeOutText(string _gunText)
    {
        Color tempColor = gunText.color;
        tempColor.a = 1f;
        gunText.color = tempColor;

        Vector3 tempVec = crossHair.rectTransform.position;
        tempVec.y += 80f;
        textRect.position = tempVec;        
        gunText.text = _gunText;

        float timeCheck = 0f;
        float fadeOutTime = 1f;
        float t = 0f;
        
        while(timeCheck < fadeOutTime)
        {
            timeCheck += Time.unscaledDeltaTime;
            t = timeCheck / fadeOutTime;

            tempColor.a = Mathf.Lerp(1, 0, t);
            gunText.color = tempColor;

            yield return null;
        }

    }

    public void FadeOutCrossHair()
    {
        if (crosshairFadeOut != null)
        {
            StopCoroutine(crosshairFadeOut);
            //return;
        }

        crosshairFadeOut = StartCoroutine(IEFadeOutCrosshair());
    }

    IEnumerator IEFadeOutCrosshair()
    {
        Color tempColor = crossHair.color;
        tempColor.a = 1f;
        crossHair.color = tempColor;

        float timeCheck = 0f;
        float fadeOutTime = 1f;
        float t = 0f;

        while (timeCheck < fadeOutTime)
        {
            timeCheck += Time.unscaledDeltaTime;
            t = timeCheck / fadeOutTime;

            tempColor.a = Mathf.Lerp(1, 0, t);
            crossHair.color = tempColor;

            yield return null;
        }

        ChangedCrossHair();
    }

    void InitColorText()
    {
        Color tempColor = gunText.color;
        tempColor.a = 1f;
        gunText.color = tempColor;
    }
    
    public void CheckRangeCrossHair()
    {
        Color tempColor = crossHair.color;
        tempColor.a = 1f;
        crossHair.color = tempColor;

        if (!checkSuccessRay)
        {
            if (textFadeOut != null)
            {
                StopCoroutine(textFadeOut);
            }

            textFadeOut = StartCoroutine(IEFadeOutText("너무 멀어!"));            
            crossHair.color = Color.red;
            crossHair.sprite = crossHairSprite[(int)CrossHair.UNABLE];
        }
        else
        {
            crossHair.color = Color.green;
            crossHair.sprite = crossHairSprite[(int)CrossHair.ABLE];
        }
    }

    public void CheckUnlockUi()
    {
        if (textFadeOut != null)
        {
            StopCoroutine(textFadeOut);
        }

        crossHair.sprite = crossHairSprite[(int)CrossHair.LOCK];
        crossHair.color = Color.white;
        textFadeOut = StartCoroutine(IEFadeOutText("구매 필요!"));
    }

    public void ChangedGravity()
    {
        if(usedGravity)
        {
            usedGravity = false;
        }
        else if(!usedGravity)
        {
            usedGravity = true;
        }        
    }

    public void CancelGrabText(string _text)
    {
        if (textFadeOut != null)
        {
            StopCoroutine(textFadeOut);
        }

        textFadeOut = StartCoroutine(IEFadeOutText(_text));
    }
}