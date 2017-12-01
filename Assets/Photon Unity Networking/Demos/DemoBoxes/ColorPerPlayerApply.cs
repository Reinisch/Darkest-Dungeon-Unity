using Photon;
using UnityEngine;

using ExitGames.UtilityScripts;

/// <summary>Sample script that uses ColorPerPlayer to apply it to an object's material color.</summary>
public class ColorPerPlayerApply : PunBehaviour
{
    // ColorPerPlayer should be a singleton. As it's not, we cache the instance for all ColorPerPlayerApply
    private static ColorPerPlayer colorPickerCache;

    // Cached, so we can apply color changes
    private Renderer rendererComponent;

	// we need to reach the PlayerRoomindexing Component. So for safe initialization, we avoid having to mess with script execution order
	bool isInitialized;
	
	void OnEnable()
	{
		if (!isInitialized)
		{
			Init();
		}
	}
	
	void Start()
	{
		if (!isInitialized)
		{
			Init();
		}
	}
	
	void Init()
	{
		if (!isInitialized && PlayerRoomIndexing.instance!=null)
		{
			PlayerRoomIndexing.instance.OnRoomIndexingChanged += ApplyColor;
			isInitialized = true;
		}
	}
	
	
	void OnDisable()
	{
		isInitialized = false;
		if (PlayerRoomIndexing.instance!=null)
		{
			PlayerRoomIndexing.instance.OnRoomIndexingChanged -= ApplyColor;
		}
	}


    public void Awake()
    {
        if (colorPickerCache == null)
        {
            colorPickerCache = FindObjectOfType<ColorPerPlayer>() as ColorPerPlayer;
        }

        if (colorPickerCache == null)
        {
            enabled = false;
        }
        if (photonView.isSceneView)
        {
            enabled = false;
        }

        this.rendererComponent = GetComponent<Renderer>();
    }


    /// <summary>Called by PUN on all components of network-instantiated GameObjects.</summary>
    /// <param name="info">Details about this instantiation.</param>
    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        this.ApplyColor(); // this applies a color, even before the initial Update() call is done
    }


    public void ApplyColor()
    {
        if (photonView.owner == null)
        {
            return;
        }

		int _index = photonView.owner.GetRoomIndex();

		if (_index>=0 && _index<=colorPickerCache.Colors.Length)
		{
			this.rendererComponent.material.color = colorPickerCache.Colors[_index];
		}

    }
}