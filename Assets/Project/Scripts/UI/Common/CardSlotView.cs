using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotView : MonoBehaviour
{
    [SerializeField] Card card;
    [SerializeField] Button cardButton;
    [SerializeField] GameObject priceRow;
    [SerializeField] TMP_Text priceText;
    [SerializeField] Button actionButton;
    [SerializeField] TMP_Text actionLabel;
    [SerializeField] TMP_Text countText;

    public string Id { get; private set; }
    public event Action<CardSlotView> Clicked;
    public event Action<CardSlotView> ActionClicked;

    /* Shows a single creature card using Piotrs card visuals, with extras on top
     * The shop/deck and collection panels spawn this prefab and call Show() to fill it in
     */
    void Awake()
    {
        if (cardButton != null) cardButton.onClick.AddListener(() => Clicked?.Invoke(this));
        if (actionButton != null) actionButton.onClick.AddListener(() => ActionClicked?.Invoke(this));
    }

    public void Show(CreatureCatalog.Entry entry, int price = -1, string action = "", int count = 0)
    {
        Id = entry.Id;
        card.SetCardData(entry.Creature);

        if (priceRow != null) priceRow.SetActive(price >= 0);
        if (priceText != null) priceText.text = price.ToString();

        if (actionButton != null) actionButton.gameObject.SetActive(action != "");
        if (actionLabel != null) actionLabel.text = action;

        if (countText != null)
        {
            countText.gameObject.SetActive(count > 1);
            countText.text = $"x{count}";
        }
    }

    public void SetInteractable(bool on)
    {
        if (cardButton != null) cardButton.interactable = on;
        if (actionButton != null) actionButton.interactable = on;
    }
}
