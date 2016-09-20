using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResistancesPanel : MonoBehaviour
{
    public Text stun;
    public Text blight;
    public Text disease;
    public Text deathBlow;
    public Text move;
    public Text bleed;
    public Text debuff;
    public Text trap;

    public void UpdateResistances(Character character)
    {
        stun.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.Stun).ModifiedValue);
        blight.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.Poison).ModifiedValue);
        disease.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.Disease).ModifiedValue);
        deathBlow.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.DeathBlow).ModifiedValue);
        move.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.Move).ModifiedValue);
        bleed.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.Bleed).ModifiedValue);
        debuff.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue);
        trap.text = string.Format("{0:#.#%;-#.#%;0%}", character.GetSingleAttribute(AttributeType.Trap).ModifiedValue);
    }
}
