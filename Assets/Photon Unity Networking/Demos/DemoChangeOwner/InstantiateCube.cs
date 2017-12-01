using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;

public class InstantiateCube : MonoBehaviour
{
    public GameObject Prefab;
    public int InstantiateType;
    //private string[] InstantiateTypeNames = {"Mine", "Scene"};

    public bool showGui;

    void OnClick()
    {
        if (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
        {
            // only use PhotonNetwork.Instantiate while in a room.
            return;
        }

        switch (InstantiateType)
        {
            case 0:
                PhotonNetwork.Instantiate(Prefab.name, this.transform.position + 3*Vector3.up, Quaternion.identity, 0);
                break;
            case 1:
                PhotonNetwork.InstantiateSceneObject(Prefab.name, InputToEvent.inputHitPos + new Vector3(0, 5f, 0), Quaternion.identity, 0, null);
                break;
        }
    }

}
