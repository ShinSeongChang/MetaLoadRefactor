using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventsController : MonoBehaviour
{
    [SerializeField]
    Controller_Physics player;

    UnityAction reloadEnd;
    UnityAction unequipEnd;
    UnityAction equipEnd;
    UnityAction walkSound;
    UnityAction climbSound;
    UnityAction jumpSound;

    UnityAction[] animationEvents;

    private void Start()
    {
        animationEvents = new UnityAction[(int)AnimationList.last];
        animationEvents[0] = reloadEnd;
        animationEvents[1] = unequipEnd;
        animationEvents[2] = equipEnd;
        animationEvents[3] = walkSound;
        animationEvents[4] = walkSound;
        animationEvents[5] = climbSound;
    }

    private void OnEnable()
    {

        reloadEnd += player.EndReloadAnimation;
        unequipEnd += player.EndUnEquipAnimation;
        equipEnd += player.EndEquipAnimation;
        walkSound += player.PlayWalkSound;
        climbSound += player.PlayClimbSound;
    }

    private void OnDisable()
    {
        reloadEnd -= player.EndReloadAnimation;
        unequipEnd -= player.EndUnEquipAnimation;
        equipEnd -= player.EndEquipAnimation;
        walkSound -= player.PlayWalkSound;
        climbSound -= player.PlayClimbSound;
    }

    public void PlayAnimation(AnimationList listNum)
    {
        animationEvents[(int)listNum].Invoke();
    }

    public void PlayMoveAnimation(AnimationList listNum)
    {
        if(player.GetMoveDirection().x % 1 != 0 && player.GetMoveDirection().y % 1 != 0 && listNum==AnimationList.walkSideSound)
        {
            return;
        }
        animationEvents[(int)listNum].Invoke();
    }

    public enum AnimationList
    {
        reloadEnd=0, unequipEnd = 1, equipEnd = 2, walkFrontSound = 3, walkSideSound =4, climbSound=5, last
    }
}
