using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PhotonGameManager : Photon.PunBehaviour
{
    #region Public Static Variables

    /// <summary>
    /// Singletone instance for network manager.
    /// </summary>
    public static PhotonGameManager Instanse { get; private set; }

    /// <summary>
    /// Number of players who are combat ready
    /// </summary>
    public static int PlayersPreparedCount { get; set; }

    public static List<BarkMessage> BarkMessages
    {
        get
        {
            return barkMessages;
        }
    }

    public static bool SkipMessagesOnClick { get; set; }
    #endregion

    #region Private variables

    private static List<BarkMessage> barkMessages = new List<BarkMessage>();

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

        RaidSceneManager.Instanse.OnSceneLeave();

        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected

            LoadArena();
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.name); // seen when other disconnects


        RaidSceneManager.Instanse.OnSceneLeave();

        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected

            LoadArena();
        }
    }

    #endregion

    #region Public Methods

    public static void PreparationCheckPassed()
    {
        PlayersPreparedCount = 0;
    }

    public static IEnumerator PreparationCheck()
    {
        while (DarkestSoundManager.NarrationQueue.Count > 0 || DarkestSoundManager.CurrentNarration != null)
            yield return null;

        Instanse.photonView.RPC("PlayerLoaded", PhotonTargets.All);

        while (PlayersPreparedCount < PhotonNetwork.room.playerCount)
            yield return null;
        PreparationCheckPassed();
        yield break;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Remote Calls

    [PunRPC]
    public void ExecuteBarkMessage(int team, string message)
    {
        BarkMessages.Add(new BarkMessage((Team)team, message));
    }

    [PunRPC]
    public void PlayerLoaded()
    {
        PlayersPreparedCount++;
    }

    [PunRPC]
    public void HeroPassButtonClicked()
    {
        if (RaidSceneManager.BattleGround.Round.HeroAction != HeroTurnAction.Waiting)
            return;

        RaidSceneManager.BattleGround.Round.HeroActionSelected(HeroTurnAction.Pass,
            RaidSceneManager.BattleGround.Round.SelectedUnit);
    }

    [PunRPC]
    public void HeroMoveButtonClicked(int primaryTargetId)
    {
        if (RaidSceneManager.BattleGround.Round.HeroAction != HeroTurnAction.Waiting)
            return;

        RaidSceneManager.BattleGround.Round.HeroActionSelected(HeroTurnAction.Move,
            RaidSceneManager.BattleGround.FindById(primaryTargetId));
    }

    [PunRPC]
    public void HeroSkillButtonClicked(int primaryTargetId)
    {
        if (RaidSceneManager.BattleGround.Round.HeroAction != HeroTurnAction.Waiting)
            return;

        RaidSceneManager.BattleGround.Round.HeroActionSelected(HeroTurnAction.Skill,
            RaidSceneManager.BattleGround.FindById(primaryTargetId));
    }

    [PunRPC]
    public void HeroSkillSelected(int skillSlotIndex)
    {
        RaidSceneMultiplayerManager.Instanse.HeroSkillSelected(skillSlotIndex);
    }

    [PunRPC]
    public void HeroMoveSelected()
    {
        RaidSceneMultiplayerManager.Instanse.HeroMoveSelected();
    }

    [PunRPC]
    public void HeroMoveDeselected()
    {
        RaidSceneMultiplayerManager.Instanse.HeroMoveDeselected();
    }

    #endregion
}
