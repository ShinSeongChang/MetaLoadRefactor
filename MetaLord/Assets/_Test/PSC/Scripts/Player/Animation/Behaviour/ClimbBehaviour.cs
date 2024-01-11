using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClimbBehaviour : StateMachineBehaviour
{
    Controller_Physics player;
    CameraController cameraManager;
    float enterTime = 0;
    bool check = false;

    private void OnEnable()
    {
        cameraManager = FindObjectOfType<CameraController>();
        player = FindObjectOfType<Controller_Physics>();
    }


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        check = false;
        enterTime = 0;
        player.StartClimbAnimation();
        player.PlayUnEquipAnimation();
        cameraManager.ChangePriorityCamera(CameraType.Climb, 20);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!check)
        {
            enterTime += Time.deltaTime;
            if (enterTime > .2f)
            {
                check = true;
                player.EndClimbAnimation();
            }
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player.EndClimbAnimation();
        player.PlayEquipAnimation();
        cameraManager.ChangePriorityCamera(CameraType.Climb, 1);

    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
