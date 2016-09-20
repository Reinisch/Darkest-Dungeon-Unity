using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharStatsPanel : MonoBehaviour
{
    public Text maxHealth;
    public Text dodge;
    public Text prot;
    public Text speed;
    public Text accuracyMod;
    public Text crit;
    public Text damage;

    public void UpdateStats(Character character)
    {
        maxHealth.text = string.Format("{0}", character.GetPairedAttribute(AttributeType.HitPoints).ModifiedValue);
        dodge.text = string.Format("{0:#.#%;-#.#%;0%}", System.Math.Round((double)character.GetSingleAttribute(AttributeType.DefenseRating).ModifiedValue, 3));
        prot.text = string.Format("{0:#.#%;-#.#%;0%}", System.Math.Round((double)character.GetSingleAttribute(AttributeType.ProtectionRating).ModifiedValue, 3));
        speed.text = string.Format("{0}", character.GetSingleAttribute(AttributeType.SpeedRating).ModifiedValue);
        accuracyMod.text = string.Format("{0:+#;-#;+0}", character.GetSingleAttribute(AttributeType.AttackRating).ModifiedValue * 100);
        crit.text = string.Format("{0:#.#%;-#.#%;0%}", System.Math.Round((double)character.GetSingleAttribute(AttributeType.CritChance).ModifiedValue, 3));
        damage.text = string.Format("{0:#.#}-{1:#.#}",
            character.GetSingleAttribute(AttributeType.DamageLow).ModifiedValue,
            character.GetSingleAttribute(AttributeType.DamageHigh).ModifiedValue);
    }

}
