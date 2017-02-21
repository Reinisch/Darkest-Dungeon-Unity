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

		this.rendererComponent.material.color = colorPickerCache.Colors[photonView.owner.GetRoomIndex()];
    }
}