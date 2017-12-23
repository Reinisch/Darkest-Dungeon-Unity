using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExchangeEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image entryFrame;
    [SerializeField]
    private Image toHeirloom;
    [SerializeField]
    private Text toAmount;
    [SerializeField]
    private Button confirmButton;

    public int ExchangeIndex { private get; set; }
    public HeirloomArrowEvent ConnectArrow { private get; set; }
    public HeirloomArrowEvent DisconnectArrow { private get; set; }

    private HeirloomExchange CurrentExchange { get; set; }
    private int CurrentAmount { get; set; }

    public void ConfirmButtonClicked()
    {
        int fromAmount = CurrentAmount / CurrentExchange.ToAmount * CurrentExchange.FromAmount;
        if(DarkestDungeonManager.Campaign.Estate.Currencies[CurrentExchange.FromType] >= fromAmount)
        {
            DarkestDungeonManager.Campaign.Estate.Currencies[CurrentExchange.FromType] -= fromAmount;
            DarkestDungeonManager.Campaign.Estate.Currencies[CurrentExchange.ToType] += CurrentAmount;
            DarkestSoundManager.PlayOneShot("event:/ui/town/heirloom_exchange_confirm");
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_click");
            EstateSceneManager.Instanse.OnHeirloomExchange();
            EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
        }
    }

    public void UpdateExchange(HeirloomExchange exchange, int fromAmount)
    {
        CurrentExchange = exchange;

        toHeirloom.sprite = DarkestDungeonManager.Data.Sprites[exchange.ToType];
        CurrentAmount = (fromAmount / exchange.FromAmount) * exchange.ToAmount;

        toAmount.text = CurrentAmount.ToString();
        confirmButton.gameObject.SetActive(CurrentAmount != 0);
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
