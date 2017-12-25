using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FMODUnity
{
    [CustomPropertyDrawer(typeof(BankRefAttribute))]
    class BankRefDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Texture browseIcon = EditorGUIUtility.Load("FMOD/SearchIconBlack.png") as Texture;
            
            SerializedProperty pathProperty = property;

            EditorGUI.BeginProperty(position, label, property);

            Event e = Event.current;
            if (e.type == EventType.DragPerform && position.Contains(e.mousePosition))
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                    DragAndDrop.objectReferences[0].GetType() == typeof(EditorBankRef))
                {
                    pathProperty.stringValue = ((EditorBankRef)DragAndDrop.objectReferences[0]).Name;

                    e.Use();
                }
            }
            if (e.type == EventType.DragUpdated && position.Contains(e.mousePosition))
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                    DragAndDrop.objectReferences[0].GetType() == typeof(EditorBankRef))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    DragAndDrop.AcceptDrag();
                    e.Use();
                }
            }

            float baseHeight = GUI.skin.textField.CalcSize(new GUIContent()).y;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding.top = buttonStyle.padding.bottom = 1;

            Rect searchRect = new Rect(position.x + position.width - browseIcon.width - 15, position.y, browseIcon.width + 10, baseHeight);
            Rect pathRect = new Rect(position.x, position.y, searchRect.x - position.x - 5, baseHeight);

            EditorGUI.PropertyField(pathRect, pathProperty, GUIContent.none);
            if (GUI.Button(searchRect, new GUIContent(browseIcon, "Select FMOD Bank"), buttonStyle))
            {
                var eventBrowser = EventBrowser.CreateInstance<EventBrowser>();

                eventBrowser.SelectBank(property);
                var windowRect = position;
                windowRect.position = GUIUtility.GUIToScreenPoint(windowRect.position);
                windowRect.height = searchRect.height + 1;
                eventBrowser.ShowAsDropDown(windowRect, new Vector2(windowRect.width, 400));
            }

            EditorGUI.EndProperty();
        }
    }    
}
