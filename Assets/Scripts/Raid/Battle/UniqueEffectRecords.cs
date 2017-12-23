public class CaptureRecord
{
    public FormationUnit PrisonerUnit { get; set; }
    public FormationUnit CaptorUnit { get; set; }
    public bool RemoveFromParty { get; set; }
    public FullCaptor Component { get { return CaptorUnit.Character.FullCaptor; } }

    public CaptureRecord()
    {
    }

    public CaptureRecord(FormationUnit prisoner, FormationUnit captor, bool removeFromParty)
    {
        PrisonerUnit = prisoner;
        CaptorUnit = captor;
        RemoveFromParty = removeFromParty;
    }

    public override int GetHashCode()
    {
        return PrisonerUnit.CombatInfo.CombatId * 100000 + CaptorUnit.CombatInfo.CombatId * 1000 + (RemoveFromParty ? 1 : 0);
    }

    public int GetHashPrisonerId(int hashCode)
    {
        return hashCode / 100000;
    }

    public int GetHashCaptorId(int hashCode)
    {
        return hashCode % 100000 / 1000;
    }

    public bool GetHashRemoveFromParty(int hashCode)
    {
        return hashCode % 2 == 1;
    }
}

public class ControlRecord
{
    public FormationUnit PrisonerUnit { get; set; }
    public FormationUnit ControllUnit { get; set; }
    public int DurationLeft { get; set; }
    public Controller ControllComponent
    {
        get
        {
            return ControllUnit.Character.ControllerCaptor;
        }
    }

    public ControlRecord()
    {
    }

    public ControlRecord(FormationUnit prisoner, FormationUnit controller, int duration)
    {
        PrisonerUnit = prisoner;
        ControllUnit = controller;
        DurationLeft = duration;
    }

    public override int GetHashCode()
    {
        return PrisonerUnit.CombatInfo.CombatId * 100000 + ControllUnit.CombatInfo.CombatId * 1000 + DurationLeft;
    }

    public int GetHashPrisonerId(int hashCode)
    {
        return hashCode / 100000;
    }

    public int GetHashControlId(int hashCode)
    {
        return hashCode % 100000 / 1000;
    }

    public int GetHashDurationLeft(int hashCode)
    {
        return hashCode % 1000;
    }
}

public class CompanionRecord
{
    public FormationUnit TargetUnit { get; set; }
    public FormationUnit CompanionUnit { get; set; }
    public Companion CompanionComponent
    {
        get
        {
            return TargetUnit.Character.Companion;
        }
    }

    public CompanionRecord()
    {
    }

    public CompanionRecord(FormationUnit target, FormationUnit companion)
    {
        TargetUnit = target;
        CompanionUnit = companion;
    }

    public override int GetHashCode()
    {
        return TargetUnit.CombatInfo.CombatId * 1000 + CompanionUnit.CombatInfo.CombatId;
    }

    public int GetHashTargetId(int hashCode)
    {
        return hashCode / 1000;
    }

    public int GetHashCompanionId(int hashCode)
    {
        return hashCode % 1000;
    }
}