using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 수집,도감 매니저
/// 231130_배경택
/// </summary>
public class RecordManager : MonoBehaviour
{
    static public RecordManager instance;
    public int endGameCount;

    [Header("도감 캔버스")]
    [SerializeField] private GameObject recordCanvas;

    // 도감 UI를 저장해놓을 Pool
    private GameObject saveRemainRecord;

    private GameObject pagePanel; //페이지 좌측 페이지가 생성될 기준 패널
    private GameObject itemName; //페이지 우측 이름 text
    private GameObject zone; //페이지 우측 지역 text
    private GameObject description; //페이지 우측 설명 text
    private GameObject labelSortGot;
    private TMP_Text pageDisPlayText; // 전체페이지 및 현재페이지 알려주는 텍스트
    //private GameObject labelSortZone;

    private GameObject itemUIObjectPrefab; // 복사하여 생성할 프리펩
    private GameObject pagePrefab; // 복사하여 생성할 프리펩
    private List<Dictionary<string, object>> objectCSV = new List<Dictionary<string, object>>();
    private List<GameObject> pageList;

    private Dictionary<int,RecordObjectInfo> _recordObjectInfos; // 도감 정보 전체가 저장된 원본
    public Dictionary<int, RecordObjectInfo> recordObjectInfos
    {
        get { return _recordObjectInfos;}
    }

    private List<GameObject> recordObjectList; // 도감 오브젝트가 저장된 리스트

    private RecordObject selectedObject;

    private int zoneSortIndex;
    private int gotSortIndex;


    // 우측 도감 설명 초기화 텍스트
    private const string RESET_NAME = "선택정보 없음";
    private const string RESET_Zone = "선택정보 없음";
    private const string RESET_Description = "선택정보 없음";

    private int checkRecordInfoId;
    private const int ID_RESET = -1;
    private const int PAGE_FULL_ITEMCOUNT = 6; // 한페이지에 몇개의 아이템을 표시할 것인지

    // 페이지 번호 및 활성화 비활성화
    private int maxPageIndex;
    private int _pageIndex;
    private int pageIndex
    {
        get
        {            
            return _pageIndex;
        }
        set
        {
            // 페이지 인덱스번호에 맞춰서 페이지전환
            pageList[_pageIndex].SetActive(false);
            _pageIndex = value;
            pageList[_pageIndex].SetActive(true);
            ChangePageDisPlayText();
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;                    
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        // 도감 우측 정보창 게임오브젝트 가져오기
        itemName = Utility.FindChildObj(recordCanvas,"Text(Info_Name)");
        zone = Utility.FindChildObj(recordCanvas, "Text(Info_Zone)");
        description = Utility.FindChildObj(recordCanvas, "Text(Info_Description)");
        pagePanel = Utility.FindChildObj(recordCanvas, "RecordPagePanel");
        labelSortGot = Utility.FindChildObj(recordCanvas, "Sort(Got)");
        pageDisPlayText = Utility.FindChildObj(recordCanvas, "Text(Page)").GetComponent<TMP_Text>();        
        saveRemainRecord = Utility.FindChildObj(recordCanvas, "SavePage");

        itemUIObjectPrefab = Resources.Load<GameObject>("Prefabs/Object_ForRecordObject");
        pagePrefab = Resources.Load<GameObject>("Prefabs/Object_ForPage");

        checkRecordInfoId = ID_RESET;

        objectCSV = CSVReader_KT.Read("CSV/ObjectCSV"); // CSV 파일 읽어들이기 TODO 데이터베이스
        _recordObjectInfos = new Dictionary<int, RecordObjectInfo>();        
        pageList = new List<GameObject>();

        recordObjectList = new List<GameObject>();

        // 정렬 인덱스 초기화
        gotSortIndex = 0;
        zoneSortIndex = 0;

        InputCSVFileToInfo(); // 초기화 레코드 정보 저장
        MakePage(objectCSV.Count); //초기화 페이지 수 계산
        MakeRecordObject(recordObjectList); //초기화 도감 오브젝트 구성
    }

    private void OnEnable()
    {
        GameEventsManager.instance.recordEvents.onGetRecordItem += GetItem;
        GameEventsManager.instance.recordEvents.onSelectRecord += InputRecordInfo;

        ResetRecord();
    }

    private void OnDisable()
    {
        GameEventsManager.instance.recordEvents.onGetRecordItem -= GetItem;
        GameEventsManager.instance.recordEvents.onSelectRecord -= InputRecordInfo;
    }

    /// <summary>
    /// 레코드 아이템을 먹었을때 호출되는 함수
    /// 231206 배경택
    /// </summary>
    /// <param name="_id"></param>
    private void GetItem(int _id)
    {

        if (_recordObjectInfos[_id].id == _id)
        {
            _recordObjectInfos[_id].obtained = true;
            GameEventsManager.instance.recordEvents.ReflectRecord(); // 정보 반영 이벤트 발생
        }   
    }

    private void Start()
    {
        GameEventsManager.instance.recordEvents.ReflectRecord();
        recordCanvas.SetActive(false);
    }

    /// <summary>
    /// CSVFile을 읽은 뒤 레코드 오브젝트 정보에 반영
    /// 231130_배경택
    /// </summary>
    private void InputCSVFileToInfo()
    {
        // CSV 정보를 BuffInfo의 리스트에 읽어들입니다.
        for (int index = 0; index < objectCSV.Count; index++)
        {
            RecordObjectInfo recordInfo = new RecordObjectInfo();

            recordInfo.id = int.Parse(objectCSV[index]["ID"].ToString());
            recordInfo.id_Description = objectCSV[index]["ID_Description"].ToString();
            recordInfo.zone = int.Parse(objectCSV[index]["Zone"].ToString());
            recordInfo.item_Name = objectCSV[index]["Item_Name"].ToString();
            recordInfo.obtained = bool.Parse(objectCSV[index]["Obtained"].ToString());
            recordInfo.description = objectCSV[index]["Description"].ToString();            

            _recordObjectInfos[recordInfo.id] = recordInfo; // 도감 정보를 저장해서 Dictionary로 관리

            // 생성된 오브젝트는 매니저 하위에서 관리
            GameObject recordObject = Instantiate(itemUIObjectPrefab, this.transform);
            recordObject.GetComponent<RecordObject>().recordInfo = recordInfo;
            recordObject.SetActive(false);

            recordObjectList.Add(recordObject);

        }
    }

    /// <summary>
    /// 도감 아이템 개수에 맞춰서 페이지 생성
    /// 231204 배경택
    /// </summary>
    /// <param name="ItemCount"> 도감에 표시될 아이템 개수 </param>
    private void MakePage(int ItemCount)
    {
        while(ItemCount > 0)
        {
            ItemCount -= PAGE_FULL_ITEMCOUNT; // 한 페이지에 들어가는 총 도감아이템 개수만큼 빼주면서 생성

            GameObject page = Instantiate(pagePrefab, pagePanel.transform); // 페이지패널 아래로 생성
            page.SetActive(false); // 페이지 비활성화
            pageList.Add(page); // 리스트에 페이지 추가
        }

        pageList[0].SetActive(true); // 첫 페이지 보이도록 실행
    }


    /// <summary>
    /// 도감 목록 생성
    /// 231204 배경택, 231227 배경택 수정
    /// </summary>
    /// <param name="__recordObjectInfos"> 수집 아이템 정보 List </param>
    private void MakeRecordObject(List<GameObject> _recordObjectList)
    {
        CalMaxPageCount(_recordObjectList);

        for (int i = 0; i < _recordObjectList.Count; i++)
        {
            int tempIndex = i / PAGE_FULL_ITEMCOUNT;

            _recordObjectList[i].SetActive(true);
            _recordObjectList[i].transform.SetParent(pageList[tempIndex].transform, false);
            //_recordObjectList[i].transform.parent = pageList[tempIndex].transform;
        }
    }

    /// <summary>
    /// 최대 페이지 수 계산
    /// 231227 배경택
    /// </summary>
    /// <param name="_recordObjectList"></param>
    private void CalMaxPageCount(List<GameObject> _recordObjectList)
    {
        maxPageIndex = _recordObjectList.Count / PAGE_FULL_ITEMCOUNT;
        if (_recordObjectList.Count % PAGE_FULL_ITEMCOUNT == 0) maxPageIndex--;
    }


    /// <summary>
    /// 도감정보에 선택된 도감아이템 정보 입력
    /// 231205 배경택
    /// </summary>
    public void InputRecordInfo(int selectedId)
    {
        if (checkRecordInfoId == _recordObjectInfos[selectedId].id) // 중복으로 같은 아이디값이 들어올 경우
        {
            DeleteRecordInfo(); // 도감 우측 텍스트정보 지우기
            return;
        }
        checkRecordInfoId = _recordObjectInfos[selectedId].id; // 도감 ID값 저장

        itemName.GetComponent<TMP_Text>().text = _recordObjectInfos[selectedId].item_Name;

        switch (_recordObjectInfos[selectedId].zone)
        {
            case 1:
                zone.GetComponent<TMP_Text>().text = "부엌";
                break;
            case 2:
                zone.GetComponent<TMP_Text>().text = "거실";
                break;
            case 3:
                zone.GetComponent<TMP_Text>().text = "아기방";
                break;
        }

        // 얻은 도감아이템일 경우에만 설명이 출력됨
        if (_recordObjectInfos[selectedId].obtained)
        {
            string temp_text = recordObjectInfos[selectedId].description.Replace("/","\n");                       

            description.GetComponent<TMP_Text>().text = temp_text;
        }
        else description.GetComponent<TMP_Text>().text = "???";
    }


    /// <summary>
    /// 도감정보 내용 지우기
    /// 231205 배경택
    /// </summary>
    private void DeleteRecordInfo()
    {
        checkRecordInfoId = ID_RESET;
        itemName.GetComponent<TMP_Text>().text = RESET_NAME;
        description.GetComponent<TMP_Text>().text = RESET_Description;
        zone.GetComponent<TMP_Text>().text = RESET_Zone;
    }

    /// <summary>
    /// 우측 버튼 누르면 pageIndex가 증가하며 페이지 변경
    /// 231205 배경택
    /// </summary>
    public  void PushRightButton()
    {
        if (pageIndex < maxPageIndex)
        {
            pageIndex++;
            PlayPageSound();
        }
        else PlayCantSound();

    }

    /// <summary>
    /// 좌측 버튼 누르면 pageIndex가 감소하며 페이지 변경
    /// 231205 배경택
    /// </summary>
    public void PushLeftButton()
    {
        if (pageIndex > 0)
        {
            pageIndex--;
            PlayPageSound();
        }
        else PlayCantSound();
    }

    /// <summary>
    /// 획득기준으로 정렬하는 함수
    /// 231205 배경택
    /// </summary>
    /// <param name="optionIndex">정렬목록 옵션 인덱스(0: 전체, 1: 획득, 2: 미획득)</param>
    public void SortGot(int optionIndex) 
    {
        gotSortIndex = optionIndex; //정렬 인덱스 저장        
        MakeRecordObject(SortTotal());
        ChangePageDisPlayText();
        PlayButtonSound();
    }

    /// <summary>
    /// 지역기준으로 정렬하는 함수
    /// 231205 배경택
    /// </summary>
    /// <param name="optionIndex">정렬목록 옵션 인덱스(0: 전체, 1: 주방, 2: 거실, 3: 아기방) </param>
    public void SortZone(int optionIndex)
    {
        zoneSortIndex = optionIndex; // 정렬 인덱스 저장        
        MakeRecordObject(SortTotal());
        ChangePageDisPlayText();
        PlayButtonSound();
    }

    /// <summary>
    /// 현재 페이지 /  전체페이지 텍스트 표시 함수
    /// 231231 배경택
    /// </summary>
    public void ChangePageDisPlayText()
    {        
        if (maxPageIndex < 0) pageDisPlayText.text = "0/0";
        else pageDisPlayText.text = (_pageIndex + 1).ToString() + "/" + (maxPageIndex + 1).ToString();
    }

    /// <summary>
    /// 전체 정렬 함수
    /// 231206 배경택
    /// </summary>
    private List<GameObject> SortTotal()
    {        
        List<GameObject> temp_recordObjectInfos = new List<GameObject>();  

        bool check = false; // 비교를 위한 임시 변수
        pageIndex = 0;        

        if (gotSortIndex == 0 && zoneSortIndex == 0) return recordObjectList; // 정렬 둘다 선택되지 않았을 경우 전체 리스트 반환

        if (gotSortIndex == 1) check = true; // 획득된 상태
        else if (gotSortIndex == 2) check = false; //획득되지 않은 상태

        // 정렬이 하나라도 선택되었다면
        foreach(var recordObject in recordObjectList)
        {
            recordObject.SetActive(false);
            //recordObject.transform.SetParent(transform);
            recordObject.transform.SetParent(saveRemainRecord.transform);

            RecordObjectInfo tempInfo = recordObject.GetComponent<RecordObject>().recordInfo; // 임시 정보에 저장

            if (tempInfo.obtained == check && tempInfo.zone == zoneSortIndex) // 정렬 둘다 선택되었다면
            {
                temp_recordObjectInfos.Add(recordObject);
            }
            else if (tempInfo.obtained == check && zoneSortIndex == 0) // 획득유무만 선택되었다면
            {
                temp_recordObjectInfos.Add(recordObject);
            }
            else if (gotSortIndex == 0 && tempInfo.zone == zoneSortIndex) // 지역만 선택되었다면
            {
                temp_recordObjectInfos.Add(recordObject);
            }
        }

        return temp_recordObjectInfos;
    }

    /// <summary>
    /// 도감을 리셋하는 함수
    /// 231220 배경택
    /// </summary>
    public void ResetRecord()
    {        
        zoneSortIndex = 0; // 정렬 인덱스 초기화
        gotSortIndex = 0; //정렬 인덱스 초기화
        pageIndex = 0; // 페이지 인덱스 초기화
        MakeRecordObject(SortTotal());
        
        labelSortGot.GetComponent<TMP_Dropdown>().value = 0;        
        labelSortGot.GetComponent<TMP_Dropdown>().captionText.text = "획득 여부";

        if(selectedObject != null) selectedObject.InActiveChecking(); // 선택표시 비활성화
        DeleteRecordInfo();
        ChangePageDisPlayText();
    }

    /// <summary>
    /// 선택된 오브젝트를 기억하는 함수
    /// 231220 배경택
    /// </summary>
    /// <param name="_selectedObject"></param>
    public void SelectObject(RecordObject _selectedObject)
    {
        selectedObject = _selectedObject;
        PlayButtonSound();
    }

    private void PlayButtonSound()
    {
        SoundManager.instance.PlaySound(GroupList.UI, (int)UISoundList.ButtonClickSound_Record);
    }

    private void PlayPageSound()
    {
        // 사운드 추가            
        SoundManager.instance.PlaySound(GroupList.UI, (int)UISoundList.ChangePageSound);
    }

    private void PlayCantSound()
    {
        // 사운드 추가        
        SoundManager.instance.PlaySound(GroupList.UI, (int)UISoundList.Cant_BuySound);
    }
}
