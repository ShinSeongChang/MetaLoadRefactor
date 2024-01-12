
using System.Collections.Generic;
using UnityEngine;

public static class ToolFunc<T> where T : MonoBehaviour
{

    public static bool ConatainsCollision(HashSet<T> hs, Collision collision)
    {
        foreach (var item in hs)
        {
            if (EqualsCollision(item, collision))
                return true;
        }
        return false;
    }

    public static bool EqualsCollision(T x, Collision y)
    {
        return x.gameObject.GetInstanceID() == y.gameObject.GetInstanceID();
    }
}

