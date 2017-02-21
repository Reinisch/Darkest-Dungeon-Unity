using UnityEngine;
using System.Collections;

[RequireComponent( typeof( PhotonView ) )]
public class OnClickRequestOwnership : Photon.MonoBehaviour
{

    public void OnClick()
    {
        if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
        {
            Vector3 colVector = new Vector3( Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ) );
            this.photonView.RPC( "ColorRpc", PhotonTargets.AllBufferedViaServer, colVector );
        }
        else
        {
            if( this.photonView.ownerId == PhotonNetwork.player.ID )
            {
                Debug.Log( "Not requesting ownership. Already mine." );
                return;
            }

            this.photonView.RequestOwnership();
        }
    }

    [PunRPC]
    public void ColorRpc( Vector3 col )
    {
        Color color = new Color( col.x, col.y, col.z );
        this.gameObject.GetComponent<Renderer>().material.color = color;
    }
}
