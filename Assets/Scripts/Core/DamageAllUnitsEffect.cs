using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Effects/Damage All Units")]
public class DamageAllUnitsEffect : CardEffect
{
    public int amount = 2;

    public override bool NeedsTarget => false;

    public override bool CanTarget(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        return true;
    }

    public override void Apply(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        // 先に全員にダメージを与えてから、倒れたものを処理する
        for (int i = 0; i < BoardState.SlotCount; i++)
        {
            CardInstance card = opponent.Board.GetCard(i);
            if (card != null) card.TakeDamage(amount);
        }

        for (int i = 0; i < BoardState.SlotCount; i++)
        {
            Combat.ResolveDeath(opponent, i);
        }
    }
    public override int AiScore(PlayerState caster, PlayerState opponent, out EffectTarget target)
    {
        target = EffectTarget.None;

        int hits = 0;
        int kills = 0;
        for (int i = 0; i < BoardState.SlotCount; i++)
        {
            CardInstance card = opponent.Board.GetCard(i);
            if (card == null) continue;

            hits++;
            if (card.CurrentHealth <= amount) kills++;
        }

        // 2体以上に当たるときだけ使う
        return hits >= 2 ? kills * 3 + hits : 0;
    }
}