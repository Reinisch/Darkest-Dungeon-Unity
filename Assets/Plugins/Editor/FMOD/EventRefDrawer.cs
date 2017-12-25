using System;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace FMODUnity
{    
    [CustomPropertyDrawer(typeof(EventRefAttribute))]
    class EventRefDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Texture browseIcon = EditorGUIUtility.Load("FMOD/SearchIconBlack.png") as Texture;
            Texture openIcon = EditorGUIUtility.Load("FMOD/BrowserIcon.png") as Texture;
            Texture addIcon = EditorGUIUtility.Load("FMOD/AddIcon.png") as Texture;
                        
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty pathProperty = property;

            Event e = Event.current;
            if (e.type == EventType.DragPerform && position.Contains(e.mousePosition))
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                    DragAndDrop.objectReferences[0].GetType() == typeof(EditorEventRef))
                {
                    pathProperty.stringValue = ((EditorEventRef)DragAndDrop.objectReferences[0]).Path;
                    GUI.changed = true;
                    e.Use();
                }
            }
            if (e.type == EventType.DragUpdated && position.Contains(e.mousePosition))
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                    DragAndDrop.objectReferences[0].GetType() == typeof(EditorEventRef))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    DragAndDrop.AcceptDrag();
                    e.Use();
                }
            }

            float baseHeight = GUI.skin.textField.CalcSize(new GUIContent()).y;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding.top = 1;
            buttonStyle.padding.bottom = 1;

            Rect addRect = new Rect(position.x + position.width - addIcon.width - 7, position.y, addIcon.width + 7, baseHeight);
            Rect openRect = new Rect(addRect.x - openIcon.width - 7, position.y, openIcon.width + 6, baseHeight);
            Rect searchRect = new Rect(openRect.x - browseIcon.width - 9, position.y, browseIcon.width + 8, baseHeight);
            Rect pathRect = new Rect(position.x, position.y, searchRect.x - position.x - 3, baseHeight);

            EditorGUI.PropertyField(pathRect, pathProperty, GUIContent.none);                       

            if (GUI.Button(searchRect, new GUIContent(browseIcon, "Search"), buttonStyle))
            {
                var eventBrowser = EventBrowser.CreateInstance<EventBrowser>();
                
                eventBrowser.SelectEvent(property);
                var windowRect = position;
                windowRect.position = GUIUtility.GUIToScreenPoint(windowRect.position);
                windowRect.height = openRect.height + 1;
                eventBrowser.ShowAsDropDown(windowRect, new Vector2(windowRect.width, 400));

            }
            if (GUI.Button(addRect, new GUIContent(addIcon, "Create New Event in Studio"), buttonStyle))
            {
                var addDropdown= EditorWindow.CreateInstance<CreateEventPopup>();

                addDropdown.SelectEvent(property);
                var windowRect = position;
                windowRect.position = GUIUtility.GUIToScreenPoint(windowRect.position);
                windowRect.height = openRect.height + 1;
                addDropdown.ShowAsDropDown(windowRect, new Vector2(windowRect.width, 500));

            }
            if (GUI.Button(openRect, new GUIContent(openIcon, "Open In Browser"), buttonStyle) &&
                !String.IsNullOrEmpty(pathProperty.stringValue) && 
                EventManager.EventFromPath(pathProperty.stringValue) != null
                )
            {
                EventBrowser.ShowEventBrowser();
                var eventBrowser = EditorWindow.GetWindow<EventBrowser>();
                eventBrowser.JumpToEvent(pathProperty.stringValue);
            }
            
            if (!String.IsNullOrEmpty(pathProperty.stringValue) && EventManager.EventFromPath(pathProperty.stringValue) != null)
            {
                Rect foldoutRect = new Rect(position.x + 10, position.y + baseHeight, position.width, baseHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "Event Properties");
                if (property.isExpanded)
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.richText = true;
                    EditorEventRef eventRef = EventManager.EventFromPath(pathProperty.stringValue);
                    float width = style.CalcSize(new GUIContent("<b>Oneshot</b>")).x;
                    Rect labelRect = new Rect(position.x, position.y + baseHeight * 2, width, baseHeight);
                    Rect valueRect = new Rect(position.x + width + 10, position.y + baseHeight * 2, pathRect.width, baseHeight);

                    GUI.Label(labelRect, new GUIContent("<b>GUID</b>"), style);
                    EditorGUI.SelectableLabel(valueRect, eventRef.Guid.ToString("b"));
                    labelRect.y += baseHeight;
                    valueRect.y += baseHeight;

                    GUI.Label(labelRect, new GUIContent("<b>Banks</b>"), style);
                    StringBuilder builder = new StringBuilder();
                    eventRef.Banks.ForEach((x) => { builder.Append(Path.GetFileNameWithoutExtension(x.Path)); builder.Append(", "); });
                    GUI.Label(valueRect, builder.ToString(0, builder.Length - 2));
                    labelRect.y += baseHeight;
                    valueRect.y += baseHeight;

                    GUI.Label(labelRect, new GUIContent("<b>Panning</b>"), style);
                    GUI.Label(valueRect, eventRef.Is3D ? "3D" : "2D");
                    labelRect.y += baseHeight;
                    valueRect.y += baseHeight;

                    GUI.Label(labelRect, new GUIContent("<b>Stream</b>"), style);
                    GUI.Label(valueRect, eventRef.IsStream.ToString());
                    labelRect.y += baseHeight;
                    valueRect.y += baseHeight;

                    GUI.Label(labelRect, new GUIContent("<b>Oneshot</b>"), style);
                    GUI.Label(valueRect, eventRef.IsOneShot.ToString());
                    labelRect.y += baseHeight;
                    valueRect.y += baseHeight;
                }
            }
            else
            {
                Rect labelRect = new Rect(position.x, position.y + baseHeight, position.width, baseHeight);
                GUI.Label(labelRect, new GUIContent("Event Not Found", EditorGUIUtility.Load("FMOD/NotFound.png") as Texture2D));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool expanded = property.isExpanded && !String.IsNullOrEmpty(property.stringValue) && EventManager.EventFromPath(property.stringValue) != null;
            float baseHeight = GUI.skin.textField.CalcSize(new GUIContent()).y;            
            return baseHeight * (expanded ? 7 : 2); // 6 lines of info
        }
    }
}
