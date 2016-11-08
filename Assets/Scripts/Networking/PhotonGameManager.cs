using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PhotonGameManager : Photon.PunBehaviour
{
    #region Public Static Variables

    /// <summary>
    /// Singletone instance for network manager.
    /// </summary>
    public static PhotonGameManager Instanse { get; private set; }

    #endregion

    #region Private Methods

    void LoadArena()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            return;
        }
        Debug.Log("PhotonNetwork : Loading Level : Dungeon Multiplayer");
        PhotonNetwork.LoadLevel("DungeonMultiplayer");
    }

    #endregion

    #region MonoBehavior callbacks

    /// <summary>
    /// MonoBehaviour method called during early initialization phase.
    /// </summary>
    void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;
        }
        else
            Destroy(gameObject);
    }

    #endregion

    #region Photon Messages

    /// <summary>
    /// Called when the local player left the room. We need to load the selector scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        //SceneManager.LoadScene("CampaignSelection");
        RaidSceneManager.Instanse.OnSceneLeave();
        PhotonNetwork.LoadLevel("CampaignSelection");
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected() " + other.name); // not seen if you're the player connecting


        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


            LoadArena();
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.name); // seen when other disconnects


        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


            LoadArena();
        }
    }

    #endregion

    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion
}
