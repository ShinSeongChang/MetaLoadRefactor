
using UnityEngine;

public class TutorialNpc : NpcBase, IInteractNpc
{
    public void InteractNpc()
    {
        myDialogue.CheckStateTutorialDialogue(state);
    }
}
