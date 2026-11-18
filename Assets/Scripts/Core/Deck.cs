using System;
using System.Collections.Generic;

public class Deck
{
    private readonly List<CardData> cards;

    public int Count => cards.Count;

    public Deck(List<CardData> source)
    {
        cards = new List<CardData>(source);
        Shuffle();
    }

    private void Shuffle()
    {
        Random random = new Random();
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public CardData Draw()
    {
        if (cards.Count == 0) return null;
        CardData top = cards[cards.Count - 1];
        cards.RemoveAt(cards.Count - 1);
        return top;
    }
}