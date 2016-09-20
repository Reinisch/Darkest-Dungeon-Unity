using UnityEngine;
using System.Collections;

public class GameSetup : MonoBehaviour
{
    public Texture2D arrowTexture;

    void Awake()
    {
        Cursor.SetCursor(arrowTexture, Vector2.zero, CursorMode.Auto);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.M))
            Time.timeScale += 1;

        if (Input.GetKey(KeyCode.N))
            Time.timeScale -= 1;

    }
}