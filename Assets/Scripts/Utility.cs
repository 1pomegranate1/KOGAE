using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilty
{
    public static float ToDegree(this Vector2 vec)
    {
        return Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg + 90;
    }
}
