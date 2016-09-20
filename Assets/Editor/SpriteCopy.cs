using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpriteCopy : EditorWindow
{
    Object copyFrom;
    Object copyTo;

    // Creates a new option in "Windows"
    [MenuItem("Window/Spritesheet Transfer")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SpriteCopy window = (SpriteCopy)EditorWindow.GetWindow(typeof(SpriteCopy));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Copy from:", EditorStyles.boldLabel);
        copyFrom = EditorGUILayout.ObjectField(copyFrom, typeof(Texture2D), false, GUILayout.Width(220));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Copy to:", EditorStyles.boldLabel);
        copyTo = EditorGUILayout.ObjectField(copyTo, typeof(Texture2D), false, GUILayout.Width(220));
        GUILayout.EndHorizontal();

        GUILayout.Space(25f);
        if (GUILayout.Button("Copy pivots and slices"))
        {
            CopyPivotsAndSlices();
        }
    }

    void CopyPivotsAndSlices()
    {
        if (!copyFrom || !copyTo)
        {
            Debug.Log("Missing one object");
            return;
        }

        if (copyFrom.GetType() != typeof(Texture2D) || copyTo.GetType() != typeof(Texture2D))
        {
            Debug.Log("Cant convert from: " + copyFrom.GetType() + "to: " + copyTo.GetType() + ". Needs two Texture2D objects!");
            return;
        }

        string copyFromPath = AssetDatabase.GetAssetPath(copyFrom);
        TextureImporter ti1 = AssetImporter.GetAtPath(copyFromPath) as TextureImporter;
        ti1.isReadable = true;

        string copyToPath = AssetDatabase.GetAssetPath(copyTo);
        TextureImporter ti2 = AssetImporter.GetAtPath(copyToPath) as TextureImporter;
        ti2.isReadable = true;

        ti2.spriteImportMode = SpriteImportMode.Multiple;

        List<SpriteMetaData> newData = new List<SpriteMetaData>();

        Debug.Log("Amount of slices found: " + ti1.spritesheet.Length);

        for (int i = 0; i < ti1.spritesheet.Length; i++)
        {
            SpriteMetaData d = ti1.spritesheet[i];
            newData.Add(d);
        }
        ti2.spritesheet = newData.ToArray();

        AssetDatabase.ImportAsset(copyToPath, ImportAssetOptions.ForceUpdate);

    }
}