using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;
using ExitGames.Client.Photon;


[InitializeOnLoad]
public class PunStartup : MonoBehaviour
{ 

    static PunStartup()
    {
        bool doneBefore = EditorPrefs.GetBool("PunDemosOpenedBefore");
        if (!doneBefore)
        {
            EditorApplication.update += OnUpdate;
        }
    }

    static void OnUpdate()
    {
        if (EditorApplication.isUpdating)
        {
            return;
        }

        bool doneBefore = EditorPrefs.GetBool("PunDemosOpenedBefore");
        if (doneBefore)
        {
            EditorApplication.update -= OnUpdate;
            return;
        }

        if (string.IsNullOrEmpty(SceneManagerHelper.EditorActiveSceneName) && EditorBuildSettings.scenes.Length == 0)
        {
            LoadPunDemoHub();
            SetPunDemoBuildSettings();
            EditorPrefs.SetBool("PunDemosOpenedBefore", true);
            Debug.Log("No scene was open. Loaded PUN Demo Hub Scene and added demos to build settings. Ready to go! This auto-setup is now disabled in this Editor.");
        }
        else
        {
            EditorApplication.update -= OnUpdate;
        }
    }

    [MenuItem("Window/Photon Unity Networking/Configure Demos (build setup)", false, 5)]
    public static void SetupDemo()
    {
        SetPunDemoBuildSettings();
    }

    //[MenuItem("Window/Photon Unity Networking/PUN Demo Loader Reset")]
    //protected static void ResetDemoLoader()
    //{
    //    EditorPrefs.DeleteKey("PunDemosOpenedBefore");
    //}

    public static void LoadPunDemoHub()
    {
		string _hubScene = PhotonNetwork.FindAssetPath("DemoHub-Scene-V2 t:scene");
		EditorSceneManager.OpenScene(_hubScene);
		Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(_hubScene);
    }




    /// <summary>
    /// Finds scenes in "Assets/Photon Unity Networking/Demos/", excludes those in folder "PUNGuide_M2H" and applies remaining scenes to build settings. The one with "Hub" in it first.
    /// </summary>
    public static void SetPunDemoBuildSettings()
    {

		string _PunPath = string.Empty;
		
		string _thisPath = PhotonNetwork.FindAssetPath ("PunStartUp");
		
		_thisPath = Application.dataPath + _thisPath.Substring (6); // remove "Assets/"

		_PunPath = PhotonEditorUtils.GetParent(_thisPath,"Photon Unity Networking");

		if (_PunPath == null)
		{
			_PunPath = Application.dataPath+"Photon Unity Networking";
		}

        // find path of pun guide
		string[] tempPaths = Directory.GetDirectories(_PunPath, "Demos", SearchOption.AllDirectories);
        if (tempPaths == null || tempPaths.Length != 1)
        {
            return;
        }

        // find scenes of guide
        string guidePath = tempPaths[0];
        tempPaths = Directory.GetFiles(guidePath, "*.unity", SearchOption.AllDirectories);

        if (tempPaths == null || tempPaths.Length == 0)
        {
            return;
        }

        // add found guide scenes to build settings
        List<EditorBuildSettingsScene> sceneAr = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < tempPaths.Length; i++)
        {
            //Debug.Log(tempPaths[i]);
            string path = tempPaths[i].Substring(Application.dataPath.Length - "Assets".Length);
            path = path.Replace('\\', '/');
            //Debug.Log(path);

            if (path.Contains("PUNGuide_M2H"))
            {
                continue;
            }

			// edited to avoid old scene to be included.
			if (path.Contains("DemoHub-Scene-V2"))
            {
                sceneAr.Insert(0, new EditorBuildSettingsScene(path, true));
                continue;
            }

            sceneAr.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = sceneAr.ToArray();
        EditorSceneManager.OpenScene(sceneAr[0].path);
    }
}