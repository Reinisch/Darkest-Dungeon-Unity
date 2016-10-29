using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class ExchangeEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image entryFrame;
    public Image toHeirloom;
    public Text toAmount;
    public Button confirmButton;

    public HeirloomExchange CurrentExchange { get; set; }
    public int ExchangeIndex { get; set; }
    public int CurrentAmount { get; set; }
    public HeirloomArrowEvent ConnectArrow { get; set; }
    public HeirloomArrowEvent DisconnectArrow { get; set; }

    public void ConfirmButtonClicked()
    {
        int fromAmount = CurrentAmount / CurrentExchange.ToAmount * CurrentExchange.FromAmount;
        if(DarkestDungeonManager.Campaign.Estate.Currencies[CurrentExchange.FromType].amount >= fromAmount)
        {
            DarkestDungeonManager.Campaign.Estate.Currencies[CurrentExchange.FromType].amount -= fromAmount;
            DarkestDungeonManager.Campaign.Estate.Currencies[CurrentExchange.ToType].amount += CurrentAmount;
            DarkestSoundManager.PlayOneShot("event:/ui/town/heirloom_exchange_confirm");
            EstateSceneManager.Instanse.currencyPanel.UpdateCurrency();
        }
    }

    public void UpdateExchange(HeirloomExchange exchange, int fromAmount)
    {
        CurrentExchange = exchange;

        toHeirloom.sprite = DarkestDungeonManager.Data.Sprites[exchange.ToType];
        CurrentAmount = (fromAmount / exchange.FromAmount) * exchange.ToAmount;

        toAmount.text = CurrentAmount.ToString();
        if (CurrentAmount == 0)
            confirmButton.gameObject.SetActive(false);
        else
            confirmButton.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        entryFrame.enabled = true;
        ConnectArrow(ExchangeIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        entryFrame.enabled = false;
        DisconnectArrow(ExchangeIndex);
    }
}
