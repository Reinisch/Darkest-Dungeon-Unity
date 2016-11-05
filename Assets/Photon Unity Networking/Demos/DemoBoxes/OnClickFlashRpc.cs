using System.Collections;
using UnityEngine;

public class OnClickFlashRpc : Photon.PunBehaviour
{
    private Material originalMaterial;
    private Color originalColor;
    private bool isFlashing;

    // called by InputToEvent.
    // we use this GameObject's PhotonView to call an RPC on all clients in this room.
    private void OnClick()
    {
        photonView.RPC("Flash", PhotonTargets.All);
    }


    // A PUN RPC.
    // RPCs are only executed on the same GameObject that was used to call it.
    // RPCs can be implemented as Coroutine, which is here used to flash the emissive color.
    [PunRPC]
    private IEnumerator Flash()
    {
        if (isFlashing)
        {
            yield break;
        }
        isFlashing = true;

		this.originalMaterial = GetComponent<Renderer>().material;
        if (!this.originalMaterial.HasProperty("_Emission"))
        {
            Debug.LogWarning("Doesnt have emission, can't flash " + gameObject);
            yield break;
        }

        this.originalColor = this.originalMaterial.GetColor("_Emission");
        this.originalMaterial.SetColor("_Emission", Color.white);

        for (float f = 0.0f; f <= 1.0f; f += 0.08f)
        {
            Color lerped = Color.Lerp(Color.white, this.originalColor, f);
            this.originalMaterial.SetColor("_Emission", lerped);
            yield return null;
        }

        this.originalMaterial.SetColor("_Emission", this.originalColor);
        isFlashing = false;
    }
}