using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Effects/Composite")]
public class CompositeEffect : CardEffect
{
    public CardEffect[] effects;

    public override bool NeedsTarget
    {
        get
        {
            foreach (CardEffect effect in effects)
            {
                if (effect != null && effect.NeedsTarget) return true;
            }
            return false;
        }
    }

    public override bool CanTarget(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        foreach (CardEffect effect in effects)
        {
            if (effect != null && effect.NeedsTarget && !effect.CanTarget(caster, opponent, target)) return false;
        }
        return true;
    }

    public override void Apply(PlayerState caster, PlayerState opponent, EffectTarget target)
    {
        foreach (CardEffect effect in effects)
        {
            if (effect != null) effect.Apply(caster, opponent, target);
        }
    }
    public override int AiScore(PlayerState caster, PlayerState opponent, out EffectTarget target)
    {
        target = EffectTarget.None;

        int total = 0;
        bool targetFound = false;
        foreach (CardEffect effect in effects)
        {
            if (effect == null) continue;

            int score = effect.AiScore(caster, opponent, out EffectTarget t);
            if (effect.NeedsTarget)
            {
                if (score <= 0) return 0; // 対象が見つからないなら、使わない
                if (!targetFound)
                {
                    target = t;
                    targetFound = true;
                }
            }
            total += score;
        }
        return total;
    }
}