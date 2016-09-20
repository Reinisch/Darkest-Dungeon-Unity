using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace FMODUnity
{

    public class StudioEventEmitterGizoDrawer
    {
        #if UNITY_4_6 || UNITY_4_7
        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NotSelected)]
        #else
        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        #endif
        static void DrawGizmo(StudioEventEmitter studioEmitter, GizmoType gizmoType)
        {
            Gizmos.DrawIcon(studioEmitter.transform.position, "FMODEmitter.tiff", true);
            if ((int)(gizmoType & GizmoType.Selected) != 0 && studioEmitter.Event != null)
            {
                EditorEventRef editorEvent = EventManager.EventFromPath(studioEmitter.Event);
                if (editorEvent != null && editorEvent.Is3D)
                {
                    Gizmos.DrawWireSphere(studioEmitter.transform.position, editorEvent.MinDistance);
                    Gizmos.DrawWireSphere(studioEmitter.transform.position, editorEvent.MaxDistance);
                }
            }
        }
    }
}
