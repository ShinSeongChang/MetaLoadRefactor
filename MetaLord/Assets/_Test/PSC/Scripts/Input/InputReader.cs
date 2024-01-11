using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName ="InputReader", menuName = "ScriptableObject/Input/InputReader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<Vector2, bool> Look = delegate { };

    public event UnityAction EnableMouseControlCamera = delegate { };
    public event UnityAction DisableMouseControlCamera = delegate { };
    public event UnityAction<float> Reload = delegate { };
    public event UnityAction<bool> Interact = delegate { };
    public event UnityAction<float> ModeChange = delegate { };

    public event UnityAction<float> Jump1 = delegate { };

    public event UnityAction<float> Jump = delegate { };
    public event UnityAction Run = delegate { };
    public event UnityAction<float> Fire = delegate { };
    public event UnityAction<float> Grab = delegate { };

    public event UnityAction<float> Store = delegate { }; // 231219 배경택
    public event UnityAction<float> Record = delegate { }; // 231219 배경택
    public event UnityAction<float> ReadyMenu = delegate { }; // 231219 배경택

    public PlayerInputActions inputActions;

    public Vector3 mouseMovement => inputActions.Player.Look.ReadValue<Vector2>();
    public Vector3 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    public bool JumpKey => inputActions.Player.Jump1.ReadValue<float>()==1f;
    public bool ShootKey => inputActions.Player.Fire.ReadValue<float>() == 1f 
        && inputActions.Player.MouseControlCamera.phase==InputActionPhase.Waiting;
    public bool GrabKey => inputActions.Player.Grab.ReadValue<float>() == 1f
        && inputActions.Player.MouseControlCamera.phase == InputActionPhase.Waiting;
    public bool ReloadKey => inputActions.Player.Reload.ReadValue<float>() == 1f;

    public bool StoreKey => inputActions.Player.Store.ReadValue<float>() == 1f; // 231219 배경택
    public bool RecordKey => inputActions.Player.Record.ReadValue<float>() == 1f; //231219 배경택
    public bool ReadyMenuKey => inputActions.Player.ReadyMenu.ReadValue<float>() == 1f; //231219 배경택


    private bool interactKey = false;
    public bool InteractKey => interactKey;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
        EnablePlayerActions();
        Interact += CheckInteract;
        interactKey = false;
    }

    public void CheckInteract(bool check)
    {
        interactKey = check;
    }
    public void CancelInteract()
    {
        interactKey = false;
    }

    public void EnablePlayerActions()
    {
        inputActions.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }
    bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";

    public void OnMouseControlCamera(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                EnableMouseControlCamera.Invoke();
                break;
            case InputActionPhase.Canceled:
                DisableMouseControlCamera.Invoke();
                break;
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Fire.Invoke(1);
                break;
            case InputActionPhase.Canceled:
                Fire.Invoke(0);
                break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump.Invoke(context.ReadValue<float>());
    }


    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Run.Invoke();
                break;
        }
    }

    public void OnPull(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        Reload.Invoke(context.ReadValue<float>());
    }

    public void OnModeChange(InputAction.CallbackContext context)
    {
       // throw new System.NotImplementedException();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Interact.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Interact.Invoke(false);
                break;
        }
    }

    public void OnStore(InputAction.CallbackContext context) // 231219 배경택
    {
        Store.Invoke(context.ReadValue<float>());
    }

    public void OnRecord(InputAction.CallbackContext context) // 231219 배경택
    {
        Record.Invoke(context.ReadValue<float>());
    }

    public void OnReadyMenu(InputAction.CallbackContext context) // 231231 배경택
    {
        ReadyMenu.Invoke(context.ReadValue<float>());
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Grab.Invoke(1);
                break;
            case InputActionPhase.Canceled:
                Grab.Invoke(0);
                break;
        }
    }

    public void OnJump1(InputAction.CallbackContext context)
    {
        Jump1.Invoke(context.ReadValue<float>());

    }
}
