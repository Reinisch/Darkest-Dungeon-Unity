using UnityEngine;
using UnityEngine.UI;

public delegate void EstateCurrencyChanged(string currency);

public class EstateCurrencyPanel : MonoBehaviour
{
    [SerializeField]
    private Text goldAmount;
    [SerializeField]
    private Text bustsAmount;
    [SerializeField]
    private Text portraitsAmount;
    [SerializeField]
    private Text deedsAmount;
    [SerializeField]
    private Text crestsAmount;
    [SerializeField]
    private SkeletonAnimation goldPile;

    public event EstateCurrencyChanged EventCurrencyIncreased;
    public event EstateCurrencyChanged EventCurrencyDecreased;

    public void CurrencyIncreased(string currency)
    {
        if (currency == "gold")
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/ui/town/buy_free");
        }

        if (EventCurrencyIncreased != null)
            EventCurrencyIncreased(currency);
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
        if (EventCurrencyDecreased != null)
            EventCurrencyDecreased(currency);
    }

    public void UpdateCurrency()
    {
        var campaign = DarkestDungeonManager.Campaign;
        goldAmount.text = campaign.Estate.Currencies["gold"].ToString();
        bustsAmount.text = campaign.Estate.Currencies["bust"].ToString();
        portraitsAmount.text = campaign.Estate.Currencies["portrait"].ToString();
        deedsAmount.text = campaign.Estate.Currencies["deed"].ToString();
        crestsAmount.text = campaign.Estate.Currencies["crest"].ToString();
    }
}