using UnityEngine;

public class CaptureEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Capture; } }
    public bool RemoveFromParty { private get; set; }
    public bool VirtueBlockable { private get; set; }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character.IsVirtued && VirtueBlockable)
            return false;

        var emptyCaptorUnit = performer.Party.Units.Find(unit => unit.Character.IsMonster
            && ((Monster)unit.Character).Data.EmptyCaptor != null
            && ((Monster)unit.Character).Data.EmptyCaptor.PerformerBaseClass == performer.Character.Class);

        if (emptyCaptorUnit == null)
            return false;

        MonsterData fullCaptorData = DarkestDungeonManager.Data.
            Monsters[((Monster)emptyCaptorUnit.Character).Data.EmptyCaptor.FullMonsterClass];
        GameObject unitObject = Resources.Load("Prefabs/Monsters/" + fullCaptorData.TypeId) as GameObject;
        FormationUnit fullCaptorUnit = RaidSceneManager.BattleGround.ReplaceUnit(fullCaptorData, emptyCaptorUnit, unitObject);

        RaidSceneManager.BattleGround.CaptureUnit(target, fullCaptorUnit, RemoveFromParty);

        if (RemoveFromParty == false)
        {
            var emptyCaptorMonster = (Monster)emptyCaptorUnit.Character;
            if (emptyCaptorMonster.Data.EmptyCaptor.CaptureEffects.Count > 0)
            {
                foreach (var captorEffectString in emptyCaptorMonster.Data.EmptyCaptor.CaptureEffects)
                {
                    var captorEffect = DarkestDungeonManager.Data.Effects[captorEffectString];
                    foreach (var subEffect in captorEffect.SubEffects)
                        subEffect.ApplyInstant(emptyCaptorUnit, target, captorEffect);
                }
            }
            target.SetCaptureEffect(fullCaptorUnit);
        }

        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            return true;
        }

        if (target.Character.IsVirtued && VirtueBlockable)
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
        return false;
    }
}