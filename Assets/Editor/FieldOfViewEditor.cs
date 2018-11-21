using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (FieldOfView))]
public class FieldOfViewEditor : Editor {

    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        // Draw circle around player:
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
        Vector3 viewAngleA = fov.directionFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.directionFromAngle(fov.viewAngle / 2, false);

        // Draw lines that show angle of view:
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        Handles.color = Color.red;
        // Draw lines from player to enemy if they are within field of view.
        foreach(Transform visibleTargets in fov.visibleTargets)
        {
            Handles.DrawLine(fov.transform.position, visibleTargets.position);
        }
    }
}
