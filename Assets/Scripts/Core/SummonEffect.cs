using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Effects/Summon")]
public class SummonEffect : CardEffect
{
    public CardData token; // 出すモンスター
    public int count = 1;  // 出す数

    public override bool NeedsTarget => false;

    public override bool CanTarget(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        return true;
    }

    public override void Apply(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        for (int i = 0; i < count; i++)
        {
            // 空いているマスが無ければ、そこで止まる
            if (!caster.Board.TryPlaceFirstEmpty(new CardInstance(token))) break;
        }
    }
    public override int AiScore(PlayerState caster, PlayerState opponent, out EffectTarget target)
    {
        target = EffectTarget.None;
        return caster.Board.HasEmptySlot() ? 2 : 0;
    }
}