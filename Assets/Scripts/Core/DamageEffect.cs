using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Effects/Damage")]
public class DamageEffect : CardEffect
{
    public int amount = 3;

    public override bool CanTarget(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        if (target.IsLeader)
        {
            // リーダーは、中央後列にカードがいる間は狙えない
            return true;
        }

        // 魔法は縦ライン守りを無視して、カードがいればどのマスでも狙える
        return opponent.Board.GetCard(target.Slot) != null;
    }

    public override void Apply(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        if (target.IsLeader)
        {
            opponent.TakeDamage(amount);
            return;
        }

        CardInstance card = opponent.Board.GetCard(target.Slot);
        card.TakeDamage(amount);
        Combat.ResolveDeath(opponent, target.Slot);
    }
    public override int AiScore(PlayerState caster, PlayerState opponent, out EffectTarget target)
    {
        target = EffectTarget.None;

        // 倒せるカードのうち、いちばんコストが高いものを狙う
        int bestSlot = -1;
        int bestCost = -1;
        for (int i = 0; i < BoardState.SlotCount; i++)
        {
            CardInstance card = opponent.Board.GetCard(i);
            if (card != null && card.CurrentHealth <= amount && card.Data.cost > bestCost)
            {
                bestSlot = i;
                bestCost = card.Data.cost;
            }
        }

        if (bestSlot >= 0)
        {
            target = new EffectTarget(false, bestSlot);
            return 2 + bestCost;
        }

    // リーダーを倒せるなら、リーダーを狙う
    int armor = opponent.Armor != null ? opponent.Armor.CurrentHealth : 0;
    if (opponent.Health + armor <= amount)

    {
        target = new EffectTarget(true, 0);
        return 100;
    }

        return 0;
    }
}