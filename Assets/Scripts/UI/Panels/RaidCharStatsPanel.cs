using UnityEngine;
using UnityEngine.UI;

public class RaidCharStatsPanel : MonoBehaviour
{
    public Text maxHealth;
    public Text stressLevel;
    public Text dodge;
    public Text prot;
    public Text speed;
    public Text accuracyMod;
    public Text crit;
    public Text damage;

    public void UpdateStats(Character character)
    {
        if(Mathf.Approximately(character.CurrentHealth, character.MaxHealth))
            maxHealth.text = string.Format("{0}/{1}",
                Mathf.CeilToInt(character.MaxHealth),
                Mathf.CeilToInt(character.MaxHealth));
        else
            maxHealth.text = string.Format("{0}/{1}",
                Mathf.RoundToInt(character.CurrentHealth),
                Mathf.CeilToInt(character.MaxHealth));

        stressLevel.text = string.Format("{0:0}/{1:0}", character.Stress.CurrentValue, character.Stress.ModifiedValue);

        dodge.text = string.Format("{0:+0.#%;-0.#%;+0%}", System.Math.Round(character.Dodge, 3));
        prot.text = string.Format("{0:+0.#%;-0.#%;+0%}", System.Math.Round(character.Protection, 3));
        speed.text = string.Format("{0}", character.Speed);
        accuracyMod.text = string.Format("{0:+#;-#;+0}", character.Accuracy * 100);
        crit.text = string.Format("{0:+0.#%;-0.#%;+0%}", System.Math.Round(character.Crit, 3));
        damage.text = string.Format("{0}-{1}", Mathf.CeilToInt(character.MinDamage), Mathf.CeilToInt(character.MaxDamage));
    }
}
