using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtils
{
    public static bool ThreeRaycasts(Vector3 origin, Vector3 dir, float spacing, Transform transform
       , out List<RaycastHit> hits, float distance, LayerMask layer, bool drawDebug = false)
    {
        bool centerHit = Physics.Raycast(origin, dir, out RaycastHit centerHitInfo, distance, layer);
        bool leftHit = Physics.Raycast(origin - transform.right * spacing, dir, out RaycastHit leftHitInfo, distance, layer);
        bool rightHit = Physics.Raycast(origin + transform.right * spacing, dir, out RaycastHit rightHitInfo, distance, layer);

        hits = new List<RaycastHit> { centerHitInfo, leftHitInfo, rightHitInfo };

        var hitFound = centerHit || leftHit || rightHit;

        if (drawDebug)
        {
            Debug.DrawRay(origin, dir * distance, Color.green);
            Debug.DrawRay(origin - transform.right * spacing, dir * distance, Color.green);
            Debug.DrawRay(origin + transform.right * spacing, dir * distance, Color.green);
        }

        return hitFound;
    }
}