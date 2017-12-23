using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public delegate void HeirloomArrowEvent(int exchangeIndex);

public class HeirloomExchangePanel : MonoBehaviour
{
    [SerializeField]
    private Image fromHeirloom;
    [SerializeField]
    private Text fromAmount;
    [SerializeField]
    private Animator exchangeAnimator;
    [SerializeField]
    private List<Image> toHeirloomArrows;
    [SerializeField]
    private List<ExchangeEntry> exchangeEntries;

    private string CurrentHeirloom { get; set; }
    private int CurrentAmount { get; set; }

    private int typeCount = 3;

    private void Start()
    {
        for(int i = 0; i < exchangeEntries.Count; i++)
        {
            exchangeEntries[i].ConnectArrow = UpdateExchangeArrows;
            exchangeEntries[i].DisconnectArrow = RemoveExchangeArrows;
            exchangeEntries[i].ExchangeIndex = i;
        }

        InitializeExchanges();
    }

    public void CurrencyUpdated(string currency)
    {
        if(exchangeAnimator.GetBool("IsOpened") && currency != "gold")
            UpdateExchanges();
    }

    #region Exchange Panel Actions

    public void ExchangeSwitched()
    {
        exchangeAnimator.SetBool("IsOpened", !exchangeAnimator.GetBool("IsOpened"));
        if (exchangeAnimator.GetBool("IsOpened"))
            DarkestSoundManager.PlayOneShot("event:/ui/town/heirloom_exchange_open");
        else
            DarkestSoundManager.PlayOneShot("event:/ui/town/heirloom_exchange_close");
    }

    public void UpTypeButtonClicked()
    {
        int currentIndex = DarkestDungeonManager.Data.HeirloomExchanges. FindIndex(entry => entry.FromType == CurrentHeirloom);
        int nextIndex = (currentIndex + typeCount) % DarkestDungeonManager.Data.HeirloomExchanges.Count;

        CurrentHeirloom = DarkestDungeonManager.Data.HeirloomExchanges[nextIndex].FromType;
        fromHeirloom.sprite = DarkestDungeonManager.Data.Sprites[CurrentHeirloom];

        UpdateExchanges(true);
    }

    public void DownTypeButtonClicked()
    {
        int currentIndex = DarkestDungeonManager.Data.HeirloomExchanges.FindIndex(entry => entry.FromType == CurrentHeirloom);
        int nextIndex = currentIndex - typeCount;

        if (nextIndex < 0)
            nextIndex = DarkestDungeonManager.Data.HeirloomExchanges.Count - 1;

        CurrentHeirloom = DarkestDungeonManager.Data.HeirloomExchanges[nextIndex].FromType;
        fromHeirloom.sprite = DarkestDungeonManager.Data.Sprites[CurrentHeirloom];

        UpdateExchanges(true);
    }

    public void IncAmountButtonClicked()
    {
        if (CurrentAmount < DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom])
        {
            CurrentAmount++;
            fromAmount.text = CurrentAmount.ToString();
            UpdateExchanges();
        }
    }

    public void DecAmountButtonClicked()
    {
        int minPossible = DarkestDungeonManager.Data.HeirloomExchanges.
            FindAll(ex => ex.FromType == CurrentHeirloom).
            Min(currentEx => currentEx.FromAmount);

        if (CurrentAmount > minPossible)
        {
            CurrentAmount--;
            fromAmount.text = CurrentAmount.ToString();
            UpdateExchanges();
        }
    }

    #endregion

    private void InitializeExchanges()
    {
        CurrentHeirloom = "bust";
        fromHeirloom.sprite = DarkestDungeonManager.Data.Sprites[CurrentHeirloom];
        UpdateExchanges(true);
    }

    private void UpdateExchanges(bool trySetMinimum = false)
    {
        int minPossible = DarkestDungeonManager.Data.HeirloomExchanges.
            FindAll(ex => ex.FromType == CurrentHeirloom).
            Min(currentEx => currentEx.FromAmount);

        if (trySetMinimum && DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom] >= minPossible)
            CurrentAmount = minPossible;

        if (CurrentAmount > DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom])
            CurrentAmount = DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom];

        fromAmount.text = CurrentAmount.ToString();

        var possibleExchanges = DarkestDungeonManager.Data.HeirloomExchanges.FindAll(ex => ex.FromType == CurrentHeirloom);
        int availableSlots = Mathf.Min(possibleExchanges.Count, exchangeEntries.Count);

        for (int i = 0; i < availableSlots; i++)
            exchangeEntries[i].UpdateExchange(possibleExchanges[i], CurrentAmount);
    }

    private void UpdateExchangeArrows(int exchangeIndex)
    {
        for (int i = 0; i < toHeirloomArrows.Count; i++)
            if (i != exchangeIndex)
                toHeirloomArrows[i].enabled = false;
            else
                toHeirloomArrows[i].enabled = true;
    }

    private void RemoveExchangeArrows(int exchangeIndex)
    {
        for (int i = 0; i < toHeirloomArrows.Count; i++)
            toHeirloomArrows[i].enabled = false;
    }
}