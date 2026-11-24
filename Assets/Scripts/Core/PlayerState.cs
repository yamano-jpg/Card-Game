using System.Collections.Generic;

public class PlayerState
{
    public const int MaxManaLimit = 10;
    public const int StartHealth = 20;
    public const int MaxHandSize = 9;

    public BoardState Board { get; } = new BoardState();
    public List<CardInstance> Hand { get; } = new List<CardInstance>();
    public Deck Deck { get; private set; }
    public PlayerState Opponent { get; set; }

    public CardInstance Weapon { get; private set; }
    public CardInstance Armor { get; private set; }
    public bool LeaderCanAttack { get; private set; }

    public int Health { get; private set; } = StartHealth;
    public int MaxMana { get; private set; }
    public int FatigueCount { get; private set; }           // デッキ切れでカードを引こうとした回数
    public event System.Action<int> Fatigued;               // デッキ切れのダメージを受けたとき(ダメージ量を渡す)
    // 中央後列のカードが倒されて、リーダーがダメージを受けたときに知らせる(画面の演出用)
    public event System.Action<CardInstance, int> CenterBackFell;
    // リーダー(または防具)が、ダメージを受けたときに知らせる(画面の演出用)
    public event System.Action<int> Damaged;

    public void RaiseCenterBackFell(CardInstance card, int damage)
    {
        CenterBackFell?.Invoke(card, damage);
    }
    public int Mana { get; private set; }

    public void SetDeck(List<CardData> cards)
    {
        Deck = new Deck(cards);
    }

    public void DrawCard()
    {
        CardData data = Deck.Draw();

        // デッキが空: 引けない代わりに、自分のリーダーがダメージを受ける(1, 2, 3…と増える)
        if (data == null)
        {
            FatigueCount++;
            TakeDamage(FatigueCount);
            Fatigued?.Invoke(FatigueCount);
            return;
        }

        if (Hand.Count >= MaxHandSize) return;
        Hand.Add(new CardInstance(data));
    }

    public void StartTurn()
    {
        if (MaxMana < MaxManaLimit) MaxMana++;
        Mana = MaxMana;

        for (int i = 0; i < BoardState.SlotCount; i++)
        {
            CardInstance card = Board.GetCard(i);
            if (card != null) card.CanAttack = true;
        }

        LeaderCanAttack = Weapon != null;
    }

    public bool CanPay(int cost)
    {
        return Mana >= cost;
    }

    public void Pay(int cost)
    {
        Mana -= cost;
    }

    public void Equip(CardInstance card)
    {
        if (card.Data.type == CardType.Weapon)
        {
            Weapon = card;
            LeaderCanAttack = true; // 装備したターンから攻撃できる
        }
        else if (card.Data.type == CardType.Armor)
        {
            Armor = card;
        }
    }

    public void BreakWeapon()
    {
        Weapon = null;
        LeaderCanAttack = false;
    }

    public void LeaderUsedAttack()
    {
        LeaderCanAttack = false;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        int total = amount;

        if (Armor != null)
        {
            int absorbed = System.Math.Min(amount, Armor.CurrentHealth);
            Armor.TakeDamage(absorbed);
            amount -= absorbed;
            if (Armor.IsDead) Armor = null;
        }

        Health -= amount;
        if (Health < 0) Health = 0;

        Damaged?.Invoke(total);
    }
}