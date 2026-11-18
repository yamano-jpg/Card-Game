using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Deck Data")]
public class DeckData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public CardData card;
        public int count = 1;
    }

    public List<Entry> entries = new List<Entry>();

    public List<CardData> ToCardList()
    {
        List<CardData> list = new List<CardData>();
        foreach (Entry entry in entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                list.Add(entry.card);
            }
        }
        return list;
    }
}