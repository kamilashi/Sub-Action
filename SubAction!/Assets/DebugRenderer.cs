using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class DebugRenderer 
{
    public static bool depthTest = false; 

    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.0f)
    {
        Debug.DrawLine(start, end, color, duration, depthTest);
    }

    public static void DrawLine(Vector2 start, Vector2 end, float zPos, Color color, float duration = 0.0f)
    {
        Debug.DrawLine(new Vector3(start.x, start.y, zPos), new Vector3(end.x, end.y, zPos), color, duration, depthTest);
    }
}

#endif