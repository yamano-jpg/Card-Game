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

    public Sprite leaderPortrait; // リーダー表示に使う絵
    public Vector2 leaderPortraitSize = new Vector2(380f, 510f);   // 絵の大きさ
    public Vector2 leaderPortraitOffset = new Vector2(0f, -5f);    // 絵の位置(窓の上端から)

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