using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

// Disable all the deprecated function warnings. Can't change anything because we have to be backwards compatible.
#pragma warning disable 0618


#pragma warning disable 0649

namespace FMODUnity
{

    [InitializeOnLoad]
    public class MigrationUtil : MonoBehaviour
    {

        /*
            If you have a custom script that uses the FMODAsset class, it will need to me migrated so it can reference events in the new integration

            For Example, if you have the existing script

                public class MyBehaviour : MonoBehaviour 
                {
                    public FMODAsset MyEvent;
                    private FMOD.Studio.EventInstance myEventInstance;

                    void Start()
                    {
                        myEventInstance = FMOD_StudioSystem.instance.GetEvent(MyEvent)    
                    }
                }

            You can migrate it by first adding a new property for each existing property

                public class MyBehaviour : MonoBehaviour 
                {
                    public FMODAsset MyEvent;
                    [FMODUnity.EventRef]
                    public String MyEventMigrated;
                    private FMOD.Studio.EventInstance myEventInstance;

                    void Start()
                    {
                        myEventInstance = FMODUnity.RuntimeManager.CreateInstance(MyEventMigrated)    
                    }
                }

            Then add a custom migration entry before running the migration command

                static CustomMigrationEntry[] CustomMigrationEntries = new CustomMigrationEntry[]
                {
                    new CustomMigrationEntry { ScriptClass = typeof(MyBehaviour), OldProperty = "MyEvent", NewProperty = "MyEventMigrated" },
                };

            You can add multiple custom migration entries
            
                static CustomMigrationEntry[] CustomMigrationEntries = new CustomMigrationEntry[]
                {
                    new CustomMigrationEntry { ScriptClass = typeof(MyBehaviour), OldProperty = "MyEvent", NewProperty = "MyEventMigrated" },
                    new CustomMigrationEntry { ScriptClass = typeof(MyOtherBehaviour), OldProperty = "MyEvent", NewProperty = "MyEventMigrated" },
                };

            After you've run the migration command you can remove the FMODAsset property from your script and rename the new properties

                public class MyBehaviour : MonoBehaviour 
                {
                    [FormerlySerializedAs("MyEventMigrated")]
                    [FMODUnity.EventRef]
                    public String MyEvent;
                    private FMOD.Studio.EventInstance myEventInstance;

                    void Start()
                    {
                        myEventInstance = FMODUnity.RuntimeManager.CreateInstance(MyEvent)    
                    }
                }

            If you want to remove the FormerlySerializedAs you can do so after you've loaded and saved every scene in your project.
        */

        static CustomMigrationEntry[] CustomMigrationEntries = new CustomMigrationEntry[]
        {
             //new CustomMigrationEntry { ScriptClass = typeof(MyBehaviour), OldProperty = "MyEvent", NewProperty = "MyEventMigrated" },
        };

        struct CustomMigrationEntry
        {
            public Type ScriptClass;
            public String OldProperty;
            public String NewProperty;
        }

        [MenuItem("FMOD/Migration From Legacy Integration")]
        public static void ShowMigrationDialog()
        {
            if (EditorUtility.DisplayDialog("FMOD Studio Integration Migration", "Are you sure you wish to migrate.\nPlease backup your Unity project first.", "OK", "Cancel"))
            {
                Migrate();
            }
        }

        public static void Migrate()
        {
            int oldUndoIndex = Undo.GetCurrentGroup();
            Undo.IncrementCurrentGroup();
            #if !UNITY_4_6 || UNITY_4_7
            Undo.SetCurrentGroupName("FMOD Studio Integration Migration");
            #endif

            Settings settings = Settings.Instance;

            var prefKey = "FMODStudioProjectPath_" + Application.dataPath;
            var prefValue = EditorPrefs.GetString(prefKey);
            if (prefValue != null)
            {
                settings.SourceBankPath = prefValue as string;
                settings.SourceBankPath += "/Build";
                settings.HasSourceProject = false;
            }

            // for each level
            EditorApplication.SaveCurrentSceneIfUserWantsTo();

            var scenes = AssetDatabase.FindAssets("*.scene");
            foreach (string scene in scenes)
            {
                if (!EditorUtility.DisplayDialog("FMOD Studio Integration Migration", String.Format("Migrate scene {0}", AssetDatabase.GUIDToAssetPath(scene)), "OK", "Cancel"))
                {
                    continue;
                }

                EditorApplication.OpenScene(AssetDatabase.GUIDToAssetPath(scene));

                var emitters = FindObjectsOfType<FMOD_StudioEventEmitter>();
                foreach (var emitter in emitters)
                {
                    GameObject parent = emitter.gameObject;
                    bool startOnAwake = emitter.startEventOnAwake;
                    string path = null;
                    if (emitter.asset != null)
                    {
                        path = emitter.asset.path;
                    }
                    else if (!String.IsNullOrEmpty(emitter.path))
                    {
                        path = emitter.path;
                    }
                    else
                    {
                        continue;
                    }

                    Undo.DestroyObjectImmediate(emitter);

                    var newEmitter = Undo.AddComponent<StudioEventEmitter>(parent);
                    newEmitter.Event = path;
                    newEmitter.PlayEvent = startOnAwake ? EmitterGameEvent.LevelStart : EmitterGameEvent.None;
                    newEmitter.PlayEvent = startOnAwake ? EmitterGameEvent.LevelEnd : EmitterGameEvent.None;
                }


                var listeners = FindObjectsOfType<FMOD_Listener>();

                foreach (var listener in listeners)
                {
                    GameObject parent = listener.gameObject;

                    foreach (var plugin in listener.pluginPaths)
                    {
                        if (!settings.Plugins.Contains(plugin))
                        {
                            settings.Plugins.Add(plugin);
                        }
                    }

                    Undo.DestroyObjectImmediate(listener);
                    Undo.AddComponent<StudioListener>(parent);
                }

                foreach (var custom in CustomMigrationEntries)
                {
                    var objects = FindObjectsOfType(custom.ScriptClass);
                    foreach (var obj in objects)
                    {
                        var serializedObj = new SerializedObject(obj);
                        var oldProp = serializedObj.FindProperty(custom.OldProperty);
                        if (oldProp == null || oldProp.propertyType != SerializedPropertyType.ObjectReference || (oldProp.objectReferenceValue != null && oldProp.objectReferenceValue.GetType() != typeof(FMODAsset)))
                        {
                            UnityEngine.Debug.LogWarning(String.Format("FMOD Studio Migration: Cannot find property {0} of type FMODAsset on script {1}", custom.OldProperty, custom.ScriptClass.ToString()));
                            continue;
                        }
                        var newProp = serializedObj.FindProperty(custom.NewProperty);
                        if (newProp == null || newProp.propertyType != SerializedPropertyType.String)
                        {
                            UnityEngine.Debug.LogWarning(String.Format("FMOD Studio Migration: Cannot find property {0} of type string on script {1}", custom.NewProperty, custom.ScriptClass.ToString()));
                            continue;
                        }
                        var oldAsset = oldProp.objectReferenceValue as FMODAsset;
                        if (oldAsset != null)
                        {
                            newProp.stringValue = oldAsset.path;
                        }
                        oldProp.objectReferenceValue = null;
                        serializedObj.ApplyModifiedProperties();
                    }
                }

                EditorApplication.SaveCurrentSceneIfUserWantsTo();
            }

            EditorUtility.SetDirty(settings);

            AssetDatabase.DeleteAsset("Assets/FMODAssets");
            AssetDatabase.Refresh();
            Undo.CollapseUndoOperations(oldUndoIndex);
        }
    }
}
