using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteractNpc : MonoBehaviour
{
    //npc 상호작용관련
    public bool isInteract = false;
    IInteractNpc playerInteract = null;
    [SerializeField]
    InputReader reader;

    //대화중인지 판별하기 위한 bool값
    public static bool isTalking = false; //TODO static 인거 참조로 변경예정 아마?

    void Update()
    {
        if (isInteract)
        {
            PushE();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("InteractZone"))
        {
            playerInteract = other.transform.parent.GetComponent<IInteractNpc>();
                //other.GetComponent<IInteractNpc>();
            isInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
       
        if (other.gameObject.layer == LayerMask.NameToLayer("InteractZone"))
        {
            playerInteract = null;
            isInteract = false;
        }
    }
    public void PushE()
    {
        if (playerInteract != null && reader.InteractKey)
        {
            reader.CancelInteract();
            isTalking = true;
            isInteract = false;
            playerInteract.InteractNpc();
            Controller_Physics.SwitchCameraLock(true); //플레이어 움직임 및 카메라 정지
        }
    }
}
