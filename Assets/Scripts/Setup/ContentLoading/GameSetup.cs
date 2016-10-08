using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public Texture2D arrowTexture;

    void Awake()
    {
        Cursor.SetCursor(arrowTexture, Vector2.zero, CursorMode.Auto);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        //FMODUnity.RuntimeManager.GetBus("bus:/").setFaderLevel(0.2f);
    }

    void Update()
    {
        //if (Input.GetKey(KeyCode.M))
        //    Time.timeScale += 1;

        //if (Input.GetKey(KeyCode.N))
        //    Time.timeScale -= 1;
    }
}