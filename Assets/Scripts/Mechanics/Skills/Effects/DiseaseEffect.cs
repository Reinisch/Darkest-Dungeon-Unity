public class DiseaseEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Disease; } }
    private bool IsRandom { get; set; }
    private Quirk Disease { get; set; }

    public DiseaseEffect(Quirk disease, bool isRandom)
    {
        Disease = disease;
        IsRandom = isRandom;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null || target.Character is Hero == false)
            return false;

        float diseaseTriggerChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;
        if (!RandomSolver.CheckSuccess(diseaseTriggerChance))
            return false;

        float diseaseChance = 1 - target.Character.GetSingleAttribute(AttributeType.Disease).ModifiedValue;

        if (RandomSolver.CheckSuccess(diseaseChance))
        {
            var hero = (Hero)target.Character;
            if (IsRandom == false && Disease != null)
            {
                if (hero.AddQuirk(Disease))
                    return true;
            }
            else
            {
                hero.AddRandomDisease();
                return true;
            }
        }
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null || target.Character is Hero == false)
            return false;

        float diseaseTriggerChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;
        if (!RandomSolver.CheckSuccess(diseaseTriggerChance))
            return false;

        float diseaseChance = 1 - target.Character.GetSingleAttribute(AttributeType.Disease).ModifiedValue;

        if (RandomSolver.CheckSuccess(diseaseChance))
        {
            var hero = (Hero)target.Character;
            if (IsRandom == false && Disease != null)
            {
                if (hero.AddQuirk(Disease))
                {
                    target.SetHalo("disease");
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Disease,
                        LocalizationManager.GetString("str_quirk_name_" + Disease.Id));
                    return true;
                }
                return false;
            }
            else
            {
                var disease = hero.AddRandomDisease();
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Disease,
                    LocalizationManager.GetString("str_quirk_name_" + disease.Id));
                target.SetHalo("disease");
                return true;
            }
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DiseaseResist);
            return false;
        }
    }
}