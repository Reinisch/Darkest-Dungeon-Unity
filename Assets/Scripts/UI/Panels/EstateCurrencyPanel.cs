using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void EstateCurrencyChanged(string currency);

public class EstateCurrencyPanel : MonoBehaviour
{
    public Text goldAmount;
    public Text bustsAmount;
    public Text portraitsAmount;
    public Text deedsAmount;
    public Text crestsAmount;

    public SkeletonAnimation goldPile;

    public event EstateCurrencyChanged onCurrencyIncreased;
    public event EstateCurrencyChanged onCurrencyDecreased;

    public void CurrencyIncreased(string currency)
    {
        if (currency == "gold")
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/ui/town/buy_free");
        }

        if (onCurrencyIncreased != null)
            onCurrencyIncreased(currency);
    }
    public void CurrencyDecreased(string currency)
    {
        if (currency == "gold")
        {
            goldPile.state.ClearTracks();
            goldPile.state.SetAnimation(0, "spend", false);
            goldPile.state.AddAnimation(0, "idle", true, 0);
            FMODUnity.RuntimeManager.PlayOneShot("event:/ui/town/buy");
        }
        if (onCurrencyDecreased != null)
            onCurrencyDecreased(currency);
    }

    public void UpdateCurrency()
    {
        var campaign = DarkestDungeonManager.Campaign;
        goldAmount.text = campaign.Estate.Currencies["gold"].amount.ToString();
        bustsAmount.text = campaign.Estate.Currencies["bust"].amount.ToString();
        portraitsAmount.text = campaign.Estate.Currencies["portrait"].amount.ToString();
        deedsAmount.text = campaign.Estate.Currencies["deed"].amount.ToString();
        crestsAmount.text = campaign.Estate.Currencies["crest"].amount.ToString();
    }
}