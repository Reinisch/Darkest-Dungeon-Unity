using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FMODUnity
{
    class FindAndReplace : EditorWindow
    {
        [MenuItem("FMOD/Find and Replace", priority = 2)]
        static void ShowFindAndReplace()
        {
            var window = CreateInstance<FindAndReplace>();
            #if UNITY_4_6 || UNITY_4_7
            window.title = "FMOD Find and Replace";
            #else
            window.titleContent = new GUIContent("FMOD Find and Replace");
            #endif
            window.OnHierarchyChange();
            var position = window.position;
            window.maxSize = window.minSize = position.size = new Vector2(400, 170);
            window.position = position;
            window.ShowUtility();
        }

        bool levelScope = true;
        bool prefabScope;
        string findText;
        string replaceText;
        string message = "";
        MessageType messageType = MessageType.None;
        int lastMatch = -1;
        List<StudioEventEmitter> emitters;
        
        void OnHierarchyChange()
        {
            emitters = new List<StudioEventEmitter>(Resources.FindObjectsOfTypeAll<StudioEventEmitter>());

            if (!levelScope)
            {
                emitters.RemoveAll(x => PrefabUtility.GetPrefabType(x) != PrefabType.Prefab);
            }

            if (!prefabScope)
            {
                emitters.RemoveAll(x => PrefabUtility.GetPrefabType(x) == PrefabType.Prefab);
            }            
        }

        bool first = true;

        void OnGUI()
        {
            bool doFind = false;
            if ((Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                Event.current.Use();
                doFind = true;
            }
                        
            GUI.SetNextControlName("find");
            EditorGUILayout.PrefixLabel("Find:");
            EditorGUI.BeginChangeCheck();
            findText = EditorGUILayout.TextField(findText);
            if (EditorGUI.EndChangeCheck())
            {
                lastMatch = -1;
                message = null;
            }
            EditorGUILayout.PrefixLabel("Replace:");
            replaceText = EditorGUILayout.TextField(replaceText);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            levelScope = EditorGUILayout.ToggleLeft("Current Level", levelScope, GUILayout.ExpandWidth(false));
            prefabScope = EditorGUILayout.ToggleLeft("Prefabs", prefabScope, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                OnHierarchyChange();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find") || doFind)
            {
                message = "";
                {
                    FindNext();
                }
                if (lastMatch == -1)
                {
                    message = "Finished Search";
                    messageType = MessageType.Warning;
                }
            }
            if (GUILayout.Button("Replace"))
            {
                message = "";
                if (lastMatch == -1)
                {
                    FindNext();
                }
                else
                {
                    Replace();
                }
                if (lastMatch == -1)
                {
                    message = "Finished Search";
                    messageType = MessageType.Warning;
                }
            }
            if (GUILayout.Button("Replace All"))
            {
                if (EditorUtility.DisplayDialog("Replace All", "Are you sure you wish to replace all in the current hierachy?", "yes", "no"))
                {
                    ReplaceAll();
                }
            }
            GUILayout.EndHorizontal();
            if (!String.IsNullOrEmpty(message))
            {
                EditorGUILayout.HelpBox(message, messageType);
            }
            else
            {
                EditorGUILayout.HelpBox("\n\n", MessageType.None);
            }

            if (first)
            {
                first = false;
                EditorGUI.FocusTextInControl("find");
            }
        }
        
        void FindNext()
        {
            for (int i = lastMatch + 1; i < emitters.Count; i++)
            {
                if (emitters[i].Event.IndexOf(findText, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    lastMatch = i;
                    EditorGUIUtility.PingObject(emitters[i]);
                    Selection.activeGameObject = emitters[i].gameObject;
                    message = "Found object";
                    messageType = MessageType.Info;
                    return;
                }
            }
            lastMatch = -1;
        }

        void ReplaceAll()
        {
            int replaced = 0;
            for (int i = 0; i < emitters.Count; i++)
            {
                if (ReplaceText(emitters[i]))
                {
                    replaced++;
                }
            }

            message = String.Format("{0} replaced", replaced);
            messageType = MessageType.Info;
        }

        bool ReplaceText(StudioEventEmitter emitter)
        {
            int findLength = findText.Length;
            int replaceLength = replaceText.Length;
            int position = 0;
            var serializedObject = new SerializedObject(emitter);
            var pathProperty = serializedObject.FindProperty("Event");
            string path = pathProperty.stringValue;
            position = path.IndexOf(findText, position, StringComparison.CurrentCultureIgnoreCase);
            while (position >= 0)
            {
                path = path.Remove(position, findLength).Insert(position, replaceText);
                position += replaceLength;
                position = path.IndexOf(findText, position, StringComparison.CurrentCultureIgnoreCase);
            }
            pathProperty.stringValue = path;
            return serializedObject.ApplyModifiedProperties();
        }

        void Replace()
        {
            ReplaceText(emitters[lastMatch]);
            FindNext();
        }
        
    }
}
