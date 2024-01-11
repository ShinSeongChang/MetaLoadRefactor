using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

enum PlayerActions
{
    Move,
    Jump,
    Fire,
    Grab,
    Run,
    Interact,
    Store,
    Record,
    Jump1,
}

enum BindingKeyword
{
    Common = 0,
    Forward = 1,
    Back = 2,
    Left = 3,
    Right = 4,
}

/// <summary>
/// 런타임에 Key를 리바인딩 하기위한 클래스
/// 231214 배경택
/// </summary>
public class RebindKeyUI : MonoBehaviour
{
    public InputReader inputReader; // 플레이어 인풋 리더 스크립터블 오브젝트를 가져오기위한 변수

    [SerializeField] private PlayerActions playerActions; // 제어하고자 하는 액션
    [SerializeField] private BindingKeyword bindingKeyword; // 제어하고자하는 세부 액션 _ bindingIndex가 반영되어있습니다.
    [SerializeField] private GameObject input_Text; // 키 입력이 들어와있는 게임오브젝트
    [SerializeField] private GameObject wait_Text; // 키 입력받는동안 표시될 게임오브젝트

    private InputAction gameInputAction; // 액션을 저장해 놓을 변수명

    private InputActionRebindingExtensions.RebindingOperation RebindOperation; // 다시 바인딩 작업을 담당하는 변수

    private void Awake()
    {
        // 인스펙터창에서 Enum을 통해 제어할 플레이어 액션을 선택
        switch (playerActions)
        {
            case PlayerActions.Move:
                gameInputAction = inputReader.inputActions.Player.Move;
                break;
            case PlayerActions.Jump:
                gameInputAction = inputReader.inputActions.Player.Jump;
                break;
            case PlayerActions.Fire:
                gameInputAction = inputReader.inputActions.Player.Fire;
                break;
            case PlayerActions.Grab:
                gameInputAction = inputReader.inputActions.Player.Grab;
                break;
            case PlayerActions.Run:
                gameInputAction = inputReader.inputActions.Player.Run;
                break;
            case PlayerActions.Interact:
                gameInputAction = inputReader.inputActions.Player.Interact;
                break;
            case PlayerActions.Store:
                gameInputAction = inputReader.inputActions.Player.Store;
                break;
            case PlayerActions.Record:
                gameInputAction = inputReader.inputActions.Player.Record;
                break;
            case PlayerActions.Jump1:
                gameInputAction = inputReader.inputActions.Player.Jump1;

                break;
            
            default:
                break;
        }

    }

    private void OnEnable()
    {
        GameEventsManager.instance.resetEvents.onResetAllBindings += ChangeDisplayText; // 전체 리셋이벤트에 화면텍스트 변경함수 할당
    }

    private void OnDisable()
    {
        RebindOperation?.Dispose(); // Rebind 작업이 존재할 경우 해제
        RebindOperation = null; //  Rebind 작업을 null로 초기화
        GameEventsManager.instance.resetEvents.onResetAllBindings -= ChangeDisplayText; // 전체 리셋이벤트에 화면텍스트 변경함수 해제
    }

    // 바인딩 버튼을 누를때 호출되는 버튼
    public void OnClickBindingButton()
    {
        RebindOperation?.Cancel(); // RebindOperation을 null로 설정

        void CleanUp()
        {
            RebindOperation?.Dispose();
            RebindOperation = null;
        }

        gameInputAction.Disable(); // Action이 활성화 되어있을경우 리바인딩을 할 수 없어서 Disable해줌

        input_Text.SetActive(false);
        wait_Text.SetActive(true);

        int bindingIndex = (int)bindingKeyword;
               
        RebindOperation = gameInputAction.PerformInteractiveRebinding() // 제어할 액션을 유저 상호작용형으로 리바인딩
                    .WithTargetBinding(bindingIndex) // 바인딩 인덱스 적용
                    .WithCancelingThrough("<Keyboard>/escape") // esc 누르면 취소
                    .OnCancel(operation =>
                    {
                        gameInputAction.Enable();
                        input_Text.SetActive(true);
                        wait_Text.SetActive(false);
                        CleanUp(); // 메모리 낭비를 방지
                    }) // 취소시 Action Enable
                    .OnComplete(operation =>
                    {
                        gameInputAction.Enable();

                        string keyToBind = gameInputAction.GetBindingDisplayString((int)bindingKeyword);
                        
                        // 중복 키 바인딩 체크
                        if (IsKeyAlreadyBound(keyToBind))
                        {                            
                            input_Text.SetActive(false);
                            wait_Text.SetActive(false);                   

                            gameInputAction.RemoveBindingOverride(bindingIndex); // 바인딩을 제거
                            CleanUp(); // 메모리 낭비를 방지
                            OnClickBindingButton(); // 제대로된 값이 입력될때까지 재입력
                            operation.Dispose();
                            return;
                        }

                        ChangeDisplayText();
                        input_Text.SetActive(true);
                        wait_Text.SetActive(false);

                        operation.Dispose();
                    }); // 완료시 Action Enable

        RebindOperation.Start();
    }

    // 표시되는 텍스트 변경
    public void ChangeDisplayText()
    {
        string displayString = string.Empty; // 표시할 문자열 초기화

        // 액션에서 디스플레이 문자열 가져오기.
        InputAction action = gameInputAction; // 액션 가져오기

        if (action != null) // 액션이 있는 경우
        {
            int bindingIndex = (int)bindingKeyword; // 인덱스 설정
            if (bindingIndex != -1) // 인덱스가 있을 경우
                displayString = action.GetBindingDisplayString(bindingIndex); // 디스플레이 문자열 가져오기

            if(displayString == "LMB") // 만약 왼쪽마우스라서 LMB가 스트링값으로 들어가 있다면
            {
                displayString = "Left Mouse";
            }
            else if(displayString == "RMB")  // 만약 오른쪽마우스라서 RMB가 스트링값으로 들어가 있다면
            {
                displayString = "Right Mouse";
            }
            else if(displayString == "Left Shift")
            {
                displayString = "Shift";
            }
        }

        if (input_Text != null) // 텍스트 오브젝트가 있는경우
            input_Text.GetComponent<TMP_Text>().text = displayString; // 텍스트 설정
    }


    // 중복된 키값을 확인하는 함수
    private bool IsKeyAlreadyBound(string keyToBind)
    {
        int bindingIndex = (int)bindingKeyword;

        foreach (var inputAction in inputReader.inputActions) // 인풋 리더의 InputAction 여러개가 있다면 전부 체크
        {
            for (int i = 0; i < inputAction.bindings.Count; i++) // 액션 하위에 바인드된 키값이 여러개라면 전부 체크
            {
                if (i == bindingIndex && inputAction == gameInputAction) continue; // 현재 내 bindingIndex와 같으면 Continue, 내 값을 바로 체크하게되면 중복으로 체크되기 때문                

                if (keyToBind == inputAction.GetBindingDisplayString(i))
                {
                    return true; // 중복된 키가 있을 경우 True 반환
                }
            }
        }
        return false; // 중복된 키가 없을 경우 false를 반환합니다.
    }
}
