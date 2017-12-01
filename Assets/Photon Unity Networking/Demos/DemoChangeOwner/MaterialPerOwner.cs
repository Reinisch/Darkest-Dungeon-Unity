using UnityEngine;
using System.Collections;


[RequireComponent( typeof( PhotonView ) )]
public class MaterialPerOwner : Photon.MonoBehaviour
{
    private int assignedColorForUserId;

    Renderer m_Renderer;

    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if( this.photonView.ownerId != assignedColorForUserId )
        {
            int index = System.Array.IndexOf(ExitGames.UtilityScripts.PlayerRoomIndexing.instance.PlayerIds, this.photonView.ownerId);
            m_Renderer.material.color = FindObjectOfType<ColorPerPlayer>().Colors[index];
            this.assignedColorForUserId = this.photonView.ownerId;
            //Debug.Log("Switched Material to: " + this.assignedColorForUserId + " " + this.renderer.material.GetInstanceID());
        }
    }
}
