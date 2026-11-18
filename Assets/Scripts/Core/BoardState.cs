public class BoardState
{
    public const int SlotCount = 6;
    public const int Columns = 3;
    public const int CenterBackSlot = 4;

    private readonly CardInstance[] slots = new CardInstance[SlotCount];

    public CardInstance GetCard(int index)
    {
        return slots[index];
    }

    public bool Place(int index, CardInstance card)
    {
        if (index < 0 || index >= SlotCount) return false;
        if (slots[index] != null) return false;
        slots[index] = card;
        return true;
    }

    public void Remove(int index)
    {
        slots[index] = null;
    }

    // 後列のカードは、同じ縦列の前列にカードがいると守られる
    public bool IsProtected(int index)
    {
        bool isBackRow = index >= Columns;
        if (!isBackRow) return false;
        return slots[index - Columns] != null;
    }

    public bool CanBeAttacked(int index)
    {
        return slots[index] != null && !IsProtected(index);
    }

    public bool IsLeaderProtected()
    {
        return slots[CenterBackSlot] != null;
    }

    // 前列のマスの、すぐ後ろ(同じ縦列の後列)のマス番号。前列でなければ -1
    public int GetBehindSlot(int slot)
    {
        return slot < Columns ? slot + Columns : -1;
    }

    public bool HasEmptySlot()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == null) return true;
        }
        return false;
    }

    // 召喚するときの、置く場所の優先順(前列の中央 → 後列の中央 → そのほか)
    private static readonly int[] SummonOrder = { 1, 4, 0, 2, 3, 5 };

    public bool TryPlaceFirstEmpty(CardInstance card)
    {
        foreach (int index in SummonOrder)
        {
            if (Place(index, card)) return true;
        }
        return false;
    }
}