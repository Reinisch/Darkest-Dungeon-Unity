using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class DeathRecordSlot : MonoBehaviour
{
    public Image resolveIcon;
    public Image heroPortrait;
    public Text heroName;
    public Text deathDescription;

    public void UpgdateRecord(DeathRecord newRecord, GraveyardWindow window)
    {
        resolveIcon.sprite = window.resolveIcons[newRecord.ResolveLevel];
        heroPortrait.sprite = DarkestDungeonManager.HeroSprites[DarkestDungeonManager.Data.HeroClasses.Values.ToList().
            Find(item => item.IndexId == newRecord.HeroClassIndex).StringId]["A"].Portrait;
        heroName.text = newRecord.HeroName;
        string deathFact = LocalizationManager.GetString("str_resolve_" + newRecord.ResolveLevel.ToString());
        deathFact += " " + newRecord.HeroName + " ";
        switch(newRecord.Factor)
        {
            case DeathFactor.Hunger:
                deathFact += LocalizationManager.GetString("str_death_hunger_hunger");
                break;
            case DeathFactor.Trap:
                deathFact += LocalizationManager.GetString("str_death_trap_trap");
                break;
            case DeathFactor.Obstacle:
                deathFact += LocalizationManager.GetString("str_death_trap_obstacle");
                break;
            case DeathFactor.AttackMonster:
                deathFact += LocalizationManager.GetString("str_death_attack_monster");
                deathFact += " " + LocalizationManager.GetString("str_monstername_" + newRecord.KillerName);
                break;
            case DeathFactor.BleedMonster:
                deathFact += LocalizationManager.GetString("str_death_bleed_monster");
                deathFact += " " + LocalizationManager.GetString("str_monstername_" + newRecord.KillerName);
                break;
            case DeathFactor.PoisonMonster:
                deathFact += LocalizationManager.GetString("str_death_poisoned_monster");
                deathFact += " " + LocalizationManager.GetString("str_monstername_" + newRecord.KillerName);
                break;
            case DeathFactor.AttackFriend:
                deathFact += LocalizationManager.GetString("str_death_attack_friendly");
                break;
            case DeathFactor.BleedFriend:
                deathFact += LocalizationManager.GetString("str_death_bleed_friendly");
                break;
            case DeathFactor.PoisonFriend:
                deathFact += LocalizationManager.GetString("str_death_poisoned_friendly");
                break;
            case DeathFactor.PoisonUnknown:
                deathFact += LocalizationManager.GetString("str_death_poisoned_unknown");
                break;
            case DeathFactor.BleedUnknown:
                deathFact += LocalizationManager.GetString("str_death_bleed_unknown");
                break;
            case DeathFactor.Unknown:
                deathFact += LocalizationManager.GetString("str_death_unknown_unknown");
                break;
            case DeathFactor.CaptorMonster:
                deathFact += LocalizationManager.GetString("str_death_captor_monster");
                deathFact += " " + LocalizationManager.GetString("str_monstername_" + newRecord.KillerName);
                break;
            case DeathFactor.HeartAttack:
                deathFact += LocalizationManager.GetString("str_death_bleed_heart_attack");
                break;
        }
        deathDescription.text = deathFact;
    }
}
