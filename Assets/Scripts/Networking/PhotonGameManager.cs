using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhotonGameManager : Photon.PunBehaviour
{
    public static PhotonGameManager Instanse { get; private set; }
    public static int PlayersPreparedCount { private get; set; }
    public static List<BarkMessage> BarkMessages { get { return barkMessages; } }
    public static bool SkipMessagesOnClick { get; set; }

    private static List<BarkMessage> barkMessages = new List<BarkMessage>();

    private void Awake()
    {
        if (Instanse == null)
            Instanse = this;
        else
            Destroy(gameObject);
    }

    public static IEnumerator PreparationCheck()
    {
        while (DarkestSoundManager.NarrationQueue.Count > 0 || DarkestSoundManager.CurrentNarration != null)
            yield return null;

        Instanse.photonView.RPC("PlayerLoaded", PhotonTargets.All);

        while (PlayersPreparedCount < PhotonNetwork.room.PlayerCount)
            yield return null;
        PreparationCheckPassed();
    }

    private static void PreparationCheckPassed()
    {
        PlayersPreparedCount = 0;
    }

    public override void OnLeftRoom()
    {
        //SceneManager.LoadScene("CampaignSelection");
        RaidSceneManager.Instanse.OnSceneLeave();
        PhotonNetwork.LoadLevel("CampaignSelection");
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting

        RaidSceneManager.Instanse.OnSceneLeave();

        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected

            LoadArena();
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects


        RaidSceneManager.Instanse.OnSceneLeave();

        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected

            LoadArena();
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void LoadArena()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            return;
        }
        Debug.Log("PhotonNetwork : Loading Level : Dungeon Multiplayer");
        PhotonNetwork.LoadLevel("DungeonMultiplayer");
    }

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
}
