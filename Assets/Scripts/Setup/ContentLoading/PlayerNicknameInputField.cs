using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(InputField))]
public class PlayerNicknameInputField : MonoBehaviour
{
    private static string playerNamePrefKey = "PlayerNickname";
    private InputField nicknameInputField;

    private void Awake()
    {
        nicknameInputField = GetComponent<InputField>();
    }

    private void Start()
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

    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerNickName(string value)
    {
        PhotonNetwork.playerName = value + " ";
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
