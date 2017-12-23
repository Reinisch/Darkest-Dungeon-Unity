using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PartyCompositionPanel : MonoBehaviour
{
    [SerializeField]
    private Text partyName;
    [SerializeField]
    private Animator partyAnimator;
    [SerializeField]
    private SkeletonAnimation compAnimation;

    public void UpdateComposition(List<string> partyClasses)
    {
        var compEntry = DarkestDungeonManager.Data.PartyNames.Find(entry =>
        {
            if (entry.ClassIds.Count != partyClasses.Count)
                return false;
            for (int i = 0; i < entry.ClassIds.Count; i++)
                if (entry.ClassIds[i] != partyClasses[i])
                    return false;
            return true;
        });

        if(compEntry != null)
        {
            partyName.text = LocalizationManager.GetString("party_name_" + compEntry.Id);
            partyAnimator.SetBool("IsValid", true);
            DarkestSoundManager.PlayOneShot("event:/ui/town/party_comp");
        }
        else
            partyAnimator.SetBool("IsValid", false);
    }

    public void ResetAnimation()
    {
        if (compAnimation.Skeleton == null)
            compAnimation.Reset();

        compAnimation.state.SetAnimation(0, "combo", false).Time = 0;
        compAnimation.Update(0);
        compAnimation.state.SetAnimation(0, "combo", false).Time = 0;
    }
}
