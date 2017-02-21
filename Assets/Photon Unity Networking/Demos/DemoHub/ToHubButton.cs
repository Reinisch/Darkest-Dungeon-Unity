using UnityEngine;
using UnityEngine.SceneManagement;

public class ToHubButton : MonoBehaviour
{
    public Texture2D ButtonTexture;
    private Rect ButtonRect;

    private static ToHubButton instance;

    public static ToHubButton Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof (ToHubButton)) as ToHubButton;
            }

            return instance;
        }
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    public void Start()
    {
        if (this.ButtonTexture == null)
        {
            gameObject.SetActive(false);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void OnGUI()
    {
        bool sceneZeroLoaded = false;

        #if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
        sceneZeroLoaded = SceneManager.GetActiveScene().buildIndex == 0;
        #else
        sceneZeroLoaded = Application.loadedLevel == 0;
        #endif

        if (!sceneZeroLoaded)
        {
            int w = this.ButtonTexture.width + 4;
            int h = this.ButtonTexture.height + 4;

            this.ButtonRect = new Rect(Screen.width - w, Screen.height - h, w, h);
            if (GUI.Button(this.ButtonRect, this.ButtonTexture, GUIStyle.none))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene(0);
            }
        }
    }
}