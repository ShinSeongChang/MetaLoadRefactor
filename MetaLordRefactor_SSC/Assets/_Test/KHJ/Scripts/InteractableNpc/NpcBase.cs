
using Unity.VisualScripting;
using UnityEngine;

public enum npcState
{
    normal,
    glued,
    objectAttached
}

public class NpcBase : MonoBehaviour
{
    //protected bool isAttached = false;
    protected npcState state;

    [SerializeField] protected DialogueUI myDialogue;

    protected virtual void Awake()
    {
        myDialogue = GameObject.Find("Dialogue_Canvas").GetComponent<DialogueUI>();
        if(!myDialogue)
            myDialogue = GameObject.Find("DialogueCanvas").GetComponent<DialogueUI>();
        state = npcState.normal;                
    }
    
    public virtual void ChangedState(npcState _change)
    {        
        if(_change == npcState.objectAttached)
        {
            state = _change;
        }
        else if (_change == npcState.glued)
        {
            if (state == npcState.objectAttached)
            {
                state = npcState.objectAttached;
            }
            else
            {
                state = _change;
            }
        }
        else
        {
            state = _change;
        }
    }

    public void PrintState()
    {
        //Debug.LogFormat("{0} <== {1} 상태",state, transform.name);
    }
}
