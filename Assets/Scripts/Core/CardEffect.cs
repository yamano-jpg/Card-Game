using UnityEngine;

// 狙う先。リーダーか、盤面のどのマスか
public readonly struct EffectTarget
{
    // 狙う先を選ばない効果に使う
    public static readonly EffectTarget None = new EffectTarget(false, -1);

    public readonly bool IsLeader;
    public readonly int Slot;

    public EffectTarget(bool isLeader, int slot)
    {
        IsLeader = isLeader;
        Slot = slot;
    }
}

// すべての効果の、ひな形
public abstract class CardEffect : ScriptableObject
{
    // 狙う先を選ぶ効果か(false なら、選ばずに発動する)
    public virtual bool NeedsTarget => true;

    public abstract bool CanTarget(PlayerState caster, PlayerState opponent, EffectTarget target);
    public abstract void Apply(PlayerState caster, PlayerState opponent, EffectTarget target);
    // CPUが使うときの、おすすめ度(0なら、使わない)。対象を選ぶ効果は、使う対象も返す
    public virtual int AiScore(PlayerState caster, PlayerState opponent, out EffectTarget target)
    {
        target = EffectTarget.None;
        return 0;
    }
}