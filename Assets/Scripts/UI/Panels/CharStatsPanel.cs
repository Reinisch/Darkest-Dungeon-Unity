using UnityEngine;
using UnityEngine.UI;

public class CharStatsPanel : MonoBehaviour
{
    [SerializeField]
    private Text maxHealth;
    [SerializeField]
    private Text dodge;
    [SerializeField]
    private Text prot;
    [SerializeField]
    private Text speed;
    [SerializeField]
    private Text accuracyMod;
    [SerializeField]
    private Text crit;
    [SerializeField]
    private Text damage;

    public void UpdateStats(Character character)
    {
        maxHealth.text = string.Format("{0}", Mathf.CeilToInt(character.MaxHealth));
        dodge.text = string.Format("{0:#.#%;-#.#%;0%}", System.Math.Round(character.Dodge, 3));
        prot.text = string.Format("{0:#.#%;-#.#%;0%}", System.Math.Round(character.Protection, 3));
        speed.text = string.Format("{0}", character.Speed);
        accuracyMod.text = string.Format("{0:+#;-#;+0}", character.Accuracy * 100);
        crit.text = string.Format("{0:#.#%;-#.#%;0%}", System.Math.Round(character.Crit, 3));
        damage.text = string.Format("{0}-{1}", Mathf.CeilToInt(character.MinDamage), Mathf.CeilToInt(character.MaxDamage));
    }
}