using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public delegate void HeirloomArrowEvent(int exchangeIndex);

public class HeirloomExchangePanel : MonoBehaviour
{
    public Text title;

    public Image fromHeirloom;
    public Text fromAmount;

    public Button incAmountButton;
    public Button decAmountButton;

    public Animator exchangeAnimator;

    public List<Image> toHeirloomArrows;
    public List<ExchangeEntry> exchangeEntries;

    public string CurrentHeirloom { get; set; }
    public int CurrentAmount { get; set; }
    public bool IsOpened
    {
        get
        {
            return exchangeAnimator.GetBool("IsOpened");
        }
    }

    private int typeCount = 3;

    void UpdateExchangeArrows(int exchangeIndex)
    {
        for (int i = 0; i < toHeirloomArrows.Count; i++)
            if (i != exchangeIndex)
                toHeirloomArrows[i].enabled = false;
            else
                toHeirloomArrows[i].enabled = true;
    }
    void RemoveExchangeArrows(int exchangeIndex)
    {
        for (int i = 0; i < toHeirloomArrows.Count; i++)
                toHeirloomArrows[i].enabled = false;
    }

    void Start()
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

    public void InitializeExchanges()
    {
        CurrentHeirloom = "bust";
        fromHeirloom.sprite = DarkestDungeonManager.Data.Sprites[CurrentHeirloom];
        UpdateExchanges(true);
    }
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
        if (CurrentAmount < DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom].amount)
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

    public void UpdateExchanges(bool trySetMinimum = false)
    {
        int minPossible = DarkestDungeonManager.Data.HeirloomExchanges.
            FindAll(ex => ex.FromType == CurrentHeirloom).
            Min(currentEx => currentEx.FromAmount);

        if (trySetMinimum && DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom].amount >= minPossible)
            CurrentAmount = minPossible;

        if (CurrentAmount > DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom].amount)
            CurrentAmount = DarkestDungeonManager.Campaign.Estate.Currencies[CurrentHeirloom].amount;

        fromAmount.text = CurrentAmount.ToString();

        var possibleExchanges = DarkestDungeonManager.Data.HeirloomExchanges.FindAll(ex => ex.FromType == CurrentHeirloom);
        int availableSlots = Mathf.Min(possibleExchanges.Count, exchangeEntries.Count);

        for (int i = 0; i < availableSlots; i++)
            exchangeEntries[i].UpdateExchange(possibleExchanges[i], CurrentAmount);
    }
}