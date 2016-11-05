using UnityEngine;
using System.Collections;

public class DemoRPGMovement : MonoBehaviour 
{
    public RPGCamera Camera;

    void OnJoinedRoom()
    {
        CreatePlayerObject();
    }

    void CreatePlayerObject()
    {
        Vector3 position = new Vector3( 33.5f, 1.5f, 20.5f );

        GameObject newPlayerObject = PhotonNetwork.Instantiate( "Robot Kyle RPG", position, Quaternion.identity, 0 );

        Camera.Target = newPlayerObject.transform;
    }
}
