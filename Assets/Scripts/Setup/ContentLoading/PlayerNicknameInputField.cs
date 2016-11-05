using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Player name input field.
/// </summary>
[RequireComponent(typeof(InputField))]
public class PlayerNicknameInputField : MonoBehaviour
{
    #region Private Variables

    private static string playerNamePrefKey = "PlayerNickname";

    private InputField nicknameInputField;

    #endregion

    #region MonoBehaviour Callbacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        nicknameInputField = GetComponent<InputField>();
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        string defaultNickname = "";
        if(PlayerPrefs.HasKey(playerNamePrefKey))
            defaultNickname = PlayerPrefs.GetString(playerNamePrefKey);
        else
        {
            var availableClasses = DarkestDungeonManager.Data.HeroClasses.Values.ToList();
            var defaultHeroClass = availableClasses[Random.Range(0, availableClasses.Count)];
            defaultNickname += LocalizationManager.GetString("hero_class_name_" + defaultHeroClass.StringId);
            defaultNickname += "#" + Random.Range(1, 10000).ToString().PadLeft(4, '0');
            defaultNickname = defaultNickname.Replace(" ", "");
        }

        PhotonNetwork.playerName = defaultNickname;
        nicknameInputField.text = defaultNickname;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerNickName(string value)
    {
        PhotonNetwork.playerName = value + " ";
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }

    #endregion
}
