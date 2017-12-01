using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

using ExitGames.UtilityScripts;

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

	// we need to reach the PlayerRoomindexing Component. So for safe initialization, we avoid having to mess with script execution order
	bool isInitialized;

	void OnEnable()
	{
		if (!isInitialized)
		{
			Init();
		}
	}

	void Start()
	{
		if (!isInitialized)
		{
			Init();
		}
	}

	void Init()
	{
		if (!isInitialized && PlayerRoomIndexing.instance!=null)
		{
			PlayerRoomIndexing.instance.OnRoomIndexingChanged += Refresh;
			isInitialized = true;
		}
	}


	void OnDisable()
	{
		PlayerRoomIndexing.instance.OnRoomIndexingChanged -= Refresh;
	}

	void Refresh()
	{
		int _index = PhotonNetwork.player.GetRoomIndex();
		if (_index == -1)
		{
			this.Reset();
		}else{
			this.MyColor = this.Colors[_index];
			this.ColorPicked = true;
		}

	}

	public override void OnJoinedRoom()
	{
		if (!isInitialized)
		{
			Init();
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
}