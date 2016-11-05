using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

/// <summary>
/// Basic script to assign a color per player in a PUN room.
/// </summary>
/// <remarks>
/// This script is but one possible implementation to have players select a color in a room.
/// It uses a Custom Property per player to store currently selected colors.
/// When a player joins and someone else didn't pick a color yet, this script waits.
/// When a color is selected or a player leaves, this scripts selects a color if it didn't do that before.
///
/// This could be extended to provide easy access to each player's color. Alternatively, you could write
/// extension methods for the PhotonPlayer class to access the Custom Property for colors in a seamless way.
/// See TeamExtensions for an example.
/// </remarks>
public class ColorPerPlayer : PunBehaviour
{
    /// <summary>
    /// Defines the available colors per room. There should be at least one color per available player spot.
    /// </summary>
    public Color[] Colors = new Color[] { Color.red, Color.blue, Color.yellow, Color.green };

    /// <summary>
    /// Property-key for Player Color. the value will be the index of the player's color in array Colors (0...)
    /// </summary>
    public const string ColorProp = "pc";

    public bool ShowColorLabel;
    public Rect ColorLabelArea = new Rect(0, 50, 100, 200);
    public Texture2D img;

    /// <summary>
    /// Color this player selected. Defaults to grey.
    /// </summary>
    public Color MyColor = Color.grey;

    public bool ColorPicked { get; set; }

    private bool afterOnJoinedRoom;	// enables this script to skip all property updates until the client joined a room

    public override void OnJoinedRoom()
    {
        this.afterOnJoinedRoom = true;
        this.SelectColor();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        this.SelectColor();
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        // when we join a room, OnPhotonPlayerPropertiesChanged usually gets called for all players. 
        // we can skip this and check those in OnJoined and when props change later on.
        if (this.afterOnJoinedRoom)
        {
            // important: SelectColor() might cause a call to OnPhotonPlayerPropertiesChanged().
            // to avoid endless recursion (and a crash), we skip calling SelectColor() if this player changed props.
            // we could also check which props changed and skip all changes, aside from color-selection.
            PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
            if (player != null && player.isLocal)
            {
                return;
            }

            this.SelectColor();
        }
    }

    public override void OnLeftRoom()
    {
        // colors are select per room.
        this.Reset();
    }

    /// <summary>
    /// Resets the color locally. In this class and the PhotonNetwork.player instance.
    /// </summary>
    public void Reset()
    {
        this.MyColor = Color.grey;
        this.ColorPicked = false;
        this.afterOnJoinedRoom = false;

        // colors are select per room. to reset, we have to clean the locally cached property in PhotonPlayer, too
        Hashtable colorProp = new Hashtable();
        colorProp.Add(ColorProp, null);
        PhotonNetwork.player.SetCustomProperties(colorProp);
    }


    // simple UI to show color
    private void OnGUI()
    {
        if (!this.ColorPicked || !this.ShowColorLabel)
        {
            return;
        }
        GUILayout.BeginArea(this.ColorLabelArea);


        GUILayout.BeginHorizontal();
        Color c = GUI.color;
        GUI.color = this.MyColor;
        GUILayout.Label(this.img);
        GUI.color = c;

        string playerNote = (PhotonNetwork.isMasterClient) ? "is your color\nyou are the Master Client" : "is your color";
        GUILayout.Label(playerNote);
        GUILayout.EndHorizontal();


        GUILayout.EndArea();
    }


    /// <summary>
    /// Attempts to select a color out of the existing, not-yet-taken ones.
    /// </summary>
    /// <remarks>
    /// Available colors are defined in Colors.
    /// Colors are taken, if their Colors index is in a player's Custom Property with the key ColorProp.
    ///
    /// </remarks>
    public void SelectColor()
    {
        if (this.ColorPicked)
        {
            return;
        }

        HashSet<int> takenColors = new HashSet<int>();

        // check which colors the OTHERS picked. we pick one of the remaining colors.
        foreach (PhotonPlayer player in PhotonNetwork.otherPlayers)
        {
            if (player.customProperties.ContainsKey(ColorProp))
            {
                int picked = (int)player.customProperties[ColorProp];
                //Debug.Log("Taken color index: " + picked);
                takenColors.Add(picked);
            }
            else
            {
                // a player joined earlier but didn't set a color yet. as that player has a lower ID, it should select a color before we do.
                // we will wait to avoid clashes when 2 players join soon after another. we don't want a color picked twice!
                if (player.ID < PhotonNetwork.player.ID)
                {
                    //Debug.Log("Can't select a color yet. This player has to pick one first: " + player);
                    return;
                }
            }
        }

        ////Debug.Log("Taken colors: " + takenColors.Count);

        if (takenColors.Count == this.Colors.Length)
        {
            Debug.LogWarning("No color available! All picked. Colors length should match MaxPlayers of the room.");
            return;
        }

        // go through the list of available colors and check each if it's taken or not
        // pick the first color that's not taken
        for (int index = 0; index < this.Colors.Length; index++)
        {
            if (!takenColors.Contains(index))
            {
                Color color = this.Colors[index];
                this.MyColor = color;

                // this stores the picked color in the server and makes it known to the others (network sync)
                Hashtable colorProp = new Hashtable();
                colorProp.Add(ColorProp, index);
                PhotonNetwork.player.SetCustomProperties(colorProp); // this goes to the server asap.

                //Debug.Log("Selected my color: " + this.MyColor);
                this.ColorPicked = true;
                break; // one color selected. break this loop.
            }
        }
    }
}
