using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ben_P/Creature Catalog")]
public class CreatureCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public CreatureData Creature;
        public Sprite CardArt;
        public int ShopPrice = 150;
        public string Id => Creature.name;
    }

    public List<Entry> Entries = new();
    public Entry Get(string id) => Entries.Find(e => e.Creature != null && e.Id == id);
}