using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Effects/Draw Cards")]
public class DrawCardsEffect : CardEffect
{
    public int count = 1;

    public override bool NeedsTarget => false;

    public override bool CanTarget(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        return true;
    }

    public override void Apply(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        for (int i = 0; i < count; i++)
        {
            caster.DrawCard();
        }
    }
    public override int AiScore(PlayerState caster, PlayerState opponent, out EffectTarget target)
    {
        target = EffectTarget.None;
        return (caster.Deck.Count > 0 && caster.Hand.Count <= 5) ? 1 : 0;
    }
}