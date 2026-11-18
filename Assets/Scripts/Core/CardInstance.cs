public class CardInstance
{
    public CardData Data { get; }
    public int CurrentHealth { get; private set; }
    public bool CanAttack { get; set; }

    // 体力が0以下なら、倒された
    public bool IsDead => CurrentHealth <= 0;

    public CardInstance(CardData data)
    {
        Data = data;
        CurrentHealth = data.health;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
    }
}