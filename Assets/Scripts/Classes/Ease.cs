using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ease
{
    public static float EaseIn(float t)
    {
        return t * t;
    }

    public static float EaseOut(float t)
    {
        return Flip(Mathf.Sqrt(Flip(t)));
    }

    public static float EaseInOut(float t)
    {
        return Mathf.Lerp(EaseIn(t), EaseOut(t), t);
    }

    private static float Flip(float x)
    {
        return 1 - x;
    }
}
