using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [SerializeField]
    private Texture2D arrowTexture;

    private void Awake()
    {
        Cursor.SetCursor(arrowTexture, Vector2.zero, CursorMode.Auto);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        //FMODUnity.RuntimeManager.GetBus("bus:/").setFaderLevel(0.2f);
    }

    private void Update()
    {
        //if (Input.GetKey(KeyCode.M))
        //    Time.timeScale += 1;

        //if (Input.GetKey(KeyCode.N))
        //    Time.timeScale -= 1;
    }
}