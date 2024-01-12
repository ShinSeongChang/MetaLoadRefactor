using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

abstract public class GunBase : MonoBehaviour
{
    protected Rigidbody conectedBody;
    protected GunStateController state;
    protected Brush brush = null;
    protected Transform AimTarget = null;
    [SerializeField]
    protected int ammo;
    public bool fireStart = false;
    public LayerMask myLayer;

    LayerMask bondLayer;
    Coroutine shootCoroutine;


    public GunMode mode { get; protected set; }

    protected virtual void Awake()
    {        
        brush = new Brush();
        state = FindAnyObjectByType<GunStateController>();
        bondLayer = 1 << LayerMask.NameToLayer("MovedObject");

        if (brush.splatTexture == null)
        {
            brush.splatTexture = Resources.Load<Texture2D>("splats");
            brush.splatsX = 4;
            brush.splatsY = 4;
        }
    }
    abstract public bool ShootGun();
    
    protected void UsedAmmo(Ray _ray, int _ammo)
    {

        if (EffectManager.instance)
            EffectManager.instance.PlayEffect(EffectList.GunMuzzle, state.muzzleStart.position, state.muzzleStart.forward);

        int id = (int)GunSoundList.FireSound;
        SoundManager.instance.PlaySound(GroupList.Gun, id);
        
        // 코루틴 돌고있는지 체크
        StopLerpGaguge();
        shootCoroutine = StartCoroutine(LerpGauge(_ammo));        
        PaintTarget.PaintRay(_ray, brush, myLayer, state.range);

        // TODO : RaycastHit 지점에 페인트 체크용 트리거 오브젝트 생성?
        RaycastHit hit;

        if (Physics.Raycast(_ray, out hit, state.Range, bondLayer))
        {
            // TODO : 개인 리팩토링
            #region 좌표지점의 Triangle 참조하여 만들어보기

            //    // hit된 오브젝트의 MeshCollider 참조, 이후에 Mesh 컴포넌트를 참조한다.
            //    MeshCollider targetMesh = hit.collider as MeshCollider;
            //    Mesh checkMesh = targetMesh.sharedMesh;

            //    // 정점 좌표
            //    Vector3[] vertices = checkMesh.vertices;

            //    // 해당 메쉬를 이루는 삼각형의 idx
            //    // 정렬을 해주려면?
            //    int[] triangles = checkMesh.triangles;

            //    for (int i = 0; i < triangles.Length; i += 3)
            //    {
            //        int temp = triangles[i];
            //        triangles[i] = triangles[i + 1];
            //        triangles[i + 1] = triangles[i + 2];
            //        triangles[i + 2] = temp;
            //    }

            //    //Debug.Log("삼각형 인덱스 : " + hit.triangleIndex);
            //    //Debug.Log("P0 : " + hit.triangleIndex * 3 + 0);
            //    //Debug.Log("P1 : " + hit.triangleIndex * 3 + 1);
            //    //Debug.Log("P2 : " + hit.triangleIndex * 3 + 2);

            //    // 삼각형의 세 꼭지점 얻기 (1개의 트라이앵글마다 3개의 꼭지점을 얻어내는 방식? => (idx * 3) + 0, 1, 2 순서로 얻는다.)
            //    Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
            //    Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            //    Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

            //    // 얻어낸 꼭지점의 좌표는 Local 좌표이기에 Wolrd 좌표로 변환해준다.
            //    p0 = hit.collider.transform.TransformPoint(p0);
            //    p1 = hit.collider.transform.TransformPoint(p1);
            //    p2 = hit.collider.transform.TransformPoint(p2);

            //    // { 직접적인 확인을 위한 오브젝트 생성 
            //    GameObject objP0 = new GameObject("P0");
            //    GameObject objP1 = new GameObject("P1");
            //    GameObject objP2 = new GameObject("P2");

            //    objP0.transform.position = p0;
            //    objP1.transform.position = p1;
            //    objP2.transform.position = p2;
            //// } 직접적인 확인을 위한 오브젝트 생성

            //// 삼각형의 변마다의 길이를 구한다면
            ////TODO: 각 Triangle마다 꼭지점의 정렬 기준을 알아내거나 새로이 정렬을?
            //    float side1 = Vector3.Distance(p0, p1);
            //    float side2 = Vector3.Distance(p0, p2);

            //float width = default;
            //float height = default;


            //Vector3 tempSize = new Vector3(side1, side2, 0.1f);

            //Mesh mesh = hit.collider.GetComponent<MeshFilter>().mesh;
            //Vector3[] vertices = mesh.vertices;
            //int[] triangles = mesh.triangles;

            //// 충돌 지점의 로컬 좌표를 구합니다.
            //Vector3 localHitPoint = hit.collider.transform.InverseTransformPoint(hit.point);

            //// 충돌 지점이 속한 삼각형을 찾습니다.
            //for (int i = hit.triangleIndex; i < hit.triangleIndex + 3; i += 3)
            //{
            //    Debug.Log("몇번 도는지 : " + i);
            //    Vector3 v1 = vertices[triangles[i]];
            //    Vector3 v2 = vertices[triangles[i + 1]];
            //    Vector3 v3 = vertices[triangles[i + 2]];

            //    // 삼각형 내부에 있는지 확인
            //    if (PointInTriangle(localHitPoint, v1, v2, v3))
            //    {
            //        // 삼각형의 면적을 계산
            //        float area = Vector3.Cross(v2 - v1, v3 - v1).magnitude / 2f;

            //        // 면적에 맞는 Collider 생성
            //        GameObject colliderObject = new GameObject("CustomCollider");
            //        colliderObject.transform.position = hit.point;
            //        colliderObject.transform.SetParent(hit.collider.transform);
            //        colliderObject.AddComponent<BoxCollider>().size = new Vector3(area, 0.1f, area);

            //        break;
            //    }
            //}



            #endregion
            // TODO : 개인 리팩토링

            if(hit.transform.GetComponent<MovedObject>() != null)
            {
                GunStateController.AddList(hit.transform.GetComponent<MovedObject>());
            }

            // TODO : 오브젝트 풀링으로 오버헤드 줄여야 함
            // Trigger 체크할 게임오브젝트 생성
            GameObject triggerObj = new GameObject("Trigger");
            triggerObj.layer = LayerMask.NameToLayer("TriggerObject");
            triggerObj.transform.position = hit.point;  // - (hit.normal * 0.1f);
            triggerObj.transform.rotation = Quaternion.LookRotation(hit.normal);
            triggerObj.transform.SetParent(hit.transform);  

            // Trigger 오브젝트 Collider 세팅
            SphereCollider objTrigger = triggerObj.AddComponent<SphereCollider>();
            objTrigger.isTrigger = true;
            objTrigger.radius = 0.5f;

        }

    }

    // 그랩건일때 레이지점 페인팅 없이 소모값만 적용하고 싶을 때
    protected void UsedAmmo(int _ammo)
    {
        if (EffectManager.instance)
            EffectManager.instance.PlayEffect(EffectList.GunExplosion, state.muzzleStart.position, Quaternion.identity);

        int id = (int)GunSoundList.FireSound;
        SoundManager.instance.PlaySound(GroupList.Gun, id);

        if (shootCoroutine != null) { StopCoroutine(shootCoroutine); }
        shootCoroutine = StartCoroutine(LerpGauge(_ammo));
    }

    protected virtual bool CheckCanFire()
    {
        if(!state.CanFire || !CanFireAmmoCount())
        {
            return false;
        }

        return true;
    }


    public virtual bool CanFireAmmoCount()
    {
        return state.Ammo >= -ammo;
    }

    public void StopLerpGaguge()
    {
        if (shootCoroutine != null) { StopCoroutine(shootCoroutine); }
    }


    IEnumerator LerpGauge(int usingAmmo)
    {
        int currentAmmo = state.Ammo;
        state.checkAmmo += usingAmmo;
        float timeCheck = 0;        

        //if (state.checkAmmo <= 0)
        //{            
        //    state.checkAmmo = 0;
        //    state.UpdateState(state.checkAmmo, GunState.EMPTY);
        //}

        //while (timeCheck < state.lerpTime)
        //{            
        //    timeCheck += Time.deltaTime;
        //    float t = timeCheck / state.lerpTime;

        //    int value = (int)Mathf.Lerp(currentAmmo, state.checkAmmo, t);
        //    state.UpdateState(value);
        //    yield return Time.deltaTime;
        //}

        if (state.checkAmmo <= 0)
        {
            state.checkAmmo = 0;
            state.UpdateState(state.checkAmmo, GunState.EMPTY);
            
            while (timeCheck < state.lerpTime)
            {
                timeCheck += Time.deltaTime;
                float t = timeCheck / state.lerpTime;

                int value = (int)Mathf.Lerp(currentAmmo, state.checkAmmo, t);
                state.UpdateState(value);
                yield return Time.deltaTime;
            }

            yield break;
        }
        else
        {
            while (timeCheck < state.lerpTime)
            {
                timeCheck += Time.deltaTime;
                float t = timeCheck / state.lerpTime;

                int value = (int)Mathf.Lerp(currentAmmo, state.checkAmmo, t);
                state.UpdateState(value);
                yield return Time.deltaTime;
            }
        }
    }

    // 점이 삼각형 내부에 있는지 확인하는 함수
    //bool PointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
    //{
    //    float dX = p.x - p2.x;
    //    float dY = p.y - p2.y;
    //    float dX21 = p2.x - p1.x;
    //    float dY12 = p1.y - p2.y;
    //    float D = dY12 * (p0.x - p2.x) + dX21 * (p0.y - p2.y);
    //    float s = dY12 * dX + dX21 * dY;
    //    float t = (p2.y - p0.y) * dX + (p0.x - p2.x) * dY;
    //    if (D < 0)
    //        return s <= 0 && t <= 0 && s + t >= D;
    //    return s >= 0 && t >= 0 && s + t <= D;
    //}
}
