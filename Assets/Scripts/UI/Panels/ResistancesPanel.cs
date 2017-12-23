using UnityEngine;
using UnityEngine.UI;

public class ResistancesPanel : MonoBehaviour
{
    [SerializeField]
    private Text stun;
    [SerializeField]
    private Text blight;
    [SerializeField]
    private Text disease;
    [SerializeField]
    private Text deathBlow;
    [SerializeField]
    private Text move;
    [SerializeField]
    private Text bleed;
    [SerializeField]
    private Text debuff;
    [SerializeField]
    private Text trap;

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
