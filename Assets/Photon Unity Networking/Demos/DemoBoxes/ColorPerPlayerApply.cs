using Photon;
using UnityEngine;

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


    /// <summary>ColorPerPlayer stores colors in Custom Player Properties. When they change, we check and re-apply the color of objects.</summary>
    /// <param name="playerAndUpdatedProps">Info about which properties changed.</param>
    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        // we could easily check if properties change for the owner of this photonView
        // for simplicity of code, we just call ApplyColor()
        this.ApplyColor();
    }


    public void ApplyColor()
    {
        if (photonView.owner == null)
        {
            return;
        }

        if (photonView.owner.customProperties.ContainsKey(ColorPerPlayer.ColorProp))
        {
            int playersColorIndex = (int)photonView.owner.customProperties[ColorPerPlayer.ColorProp];
            this.rendererComponent.material.color = colorPickerCache.Colors[playersColorIndex];
        }
    }
}