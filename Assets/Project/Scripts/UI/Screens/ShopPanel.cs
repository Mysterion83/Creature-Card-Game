using UnityEngine;

public class ShopPanel : MonoBehaviour
{
    [SerializeField] CardSlotView slotPrefab;
    [SerializeField] Transform grid;

    /*
    Controls the shop panel. It shows the cards in todays shop stock with their prices
    and greys out any you cant afford
    Clicking a card spends the coins, adds it to your collection and removes it from the shop and saves    
    */

    void OnEnable()
    {
        ProfileManager.OnProfileChanged += Refresh;
        Refresh();
    }

    void OnDisable() => ProfileManager.OnProfileChanged -= Refresh;

    void Refresh()
    {
        foreach (Transform child in grid) Destroy(child.gameObject);

        var pm = ProfileManager.Instance;
        if (pm == null || pm.Profile == null) return;

        foreach (string id in pm.Profile.ShopStock)
        {
            var entry = pm.Catalog.Get(id);
            if (entry == null) continue;

            int price = PriceOf(entry);
            CardSlotView slot = Instantiate(slotPrefab, grid);
            slot.Show(entry, price);
            slot.SetInteractable(pm.Profile.Coins >= price);
            slot.Clicked += Buy;
        }
    }

    void Buy(CardSlotView slot)
    {
        var pm = ProfileManager.Instance;
        var entry = pm.Catalog.Get(slot.Id);
        if (entry == null || !pm.TrySpend(PriceOf(entry))) return;

        pm.Profile.Collection.Add(slot.Id);
        pm.Profile.ShopStock.Remove(slot.Id);
        pm.Save();
        Refresh();
    }
    
    public static int PriceOf(CreatureCatalog.Entry entry) =>
        entry.Creature.PurchaseCost > 0 ? Mathf.RoundToInt(entry.Creature.PurchaseCost) : entry.ShopPrice;
}
