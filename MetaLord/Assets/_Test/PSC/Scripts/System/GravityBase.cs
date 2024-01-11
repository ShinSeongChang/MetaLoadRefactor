using UnityEngine;

public class GravityBase : MonoBehaviour
{
    private void OnEnable()
    {
        CustomGravity.Register(this);
    }

    private void OnDisable()
    {
        CustomGravity.UnRegister(this);
    }
    public virtual Vector3 GetGravity(Vector3 position)
    {
        return Physics.gravity;
    }
}
