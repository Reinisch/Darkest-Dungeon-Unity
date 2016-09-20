using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FMODUnity
{
    [CustomEditor(typeof(StudioBankLoader))]
    [CanEditMultipleObjects]
    class StudioBankLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var load = serializedObject.FindProperty("LoadEvent");
            var unload = serializedObject.FindProperty("UnloadEvent");
            var tag = serializedObject.FindProperty("CollisionTag");
            var banks = serializedObject.FindProperty("Banks");
            var preload = serializedObject.FindProperty("PreloadSamples");

            EditorGUILayout.PropertyField(load, new GUIContent("Load"));
            EditorGUILayout.PropertyField(unload, new GUIContent("Unload"));

            if (load.enumValueIndex == 3 || load.enumValueIndex == 4 ||
                unload.enumValueIndex == 3 || unload.enumValueIndex == 4)
            {
                tag.stringValue = EditorGUILayout.TagField("Collision Tag", tag.stringValue);
            }

            EditorGUILayout.PropertyField(preload, new GUIContent("Preload Sample Data"));

            //EditorGUILayout.PropertyField(banks);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Banks");
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Add Bank", GUILayout.ExpandWidth(false)))
            {
                banks.InsertArrayElementAtIndex(banks.arraySize);
                SerializedProperty newBank = banks.GetArrayElementAtIndex(banks.arraySize - 1);
                newBank.stringValue = "";

                var browser = EventBrowser.CreateInstance<EventBrowser>();

                #if UNITY_4_6 || UNITY_4_7
				browser.title  = "Select FMOD Bank";
                #else
                browser.titleContent = new GUIContent("Select FMOD Bank");
                #endif

                browser.SelectBank(newBank);
                browser.ShowUtility();
            }

            Texture deleteTexture = EditorGUIUtility.Load("FMOD/Delete.png") as Texture;
            GUIContent deleteContent = new GUIContent(deleteTexture, "Delete Bank");

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding.top = buttonStyle.padding.bottom = 1;
            buttonStyle.margin.top = 2;
            buttonStyle.padding.left = buttonStyle.padding.right = 4;
            buttonStyle.fixedHeight = GUI.skin.textField.CalcSize(new GUIContent()).y;

            for (int i = 0; i < banks.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(banks.GetArrayElementAtIndex(i), GUIContent.none);

                if (GUILayout.Button(deleteContent, buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    banks.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            Event e = Event.current;
            if (e.type == EventType.dragPerform)
            {
                if (DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                    DragAndDrop.objectReferences[0].GetType() == typeof(EditorBankRef))
                {
                    int pos = banks.arraySize;
                    banks.InsertArrayElementAtIndex(pos);
                    var pathProperty = banks.GetArrayElementAtIndex(pos);

                    pathProperty.stringValue = ((EditorBankRef)DragAndDrop.objectReferences[0]).Name;

                    e.Use();
                }
            }
            if (e.type == EventType.DragUpdated)
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
