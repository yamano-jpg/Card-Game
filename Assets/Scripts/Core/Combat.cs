public static class Combat
{
    public static bool CanAttackWith(PlayerState attacker, int slot)
    {
        CardInstance card = attacker.Board.GetCard(slot);
        return card != null && card.CanAttack;
    }

    public static bool CanLeaderAttack(PlayerState attacker)
    {
        return attacker.Weapon != null && attacker.LeaderCanAttack;
    }

    public static bool CanTargetCard(PlayerState defender, int slot)
    {
        return defender.Board.CanBeAttacked(slot);
    }

    public static bool CanTargetLeader(PlayerState defender)
    {
        return !defender.Board.IsLeaderProtected();
    }

    public static void AttackCard(PlayerState attacker, int attackerSlot, PlayerState defender, int targetSlot)
    {
        CardInstance a = attacker.Board.GetCard(attackerSlot);
        CardInstance d = defender.Board.GetCard(targetSlot);

        d.TakeDamage(a.Data.attack);
        a.TakeDamage(d.Data.attack);
        a.CanAttack = false;

        // 貫通: 同じ縦列の後ろのカードにも、攻撃力の半分(切り上げ)のダメージ
        int behindSlot = defender.Board.GetBehindSlot(targetSlot);
        if (a.Data.pierce && behindSlot >= 0)
        {
            CardInstance behind = defender.Board.GetCard(behindSlot);
            if (behind != null)
            {
                behind.TakeDamage((a.Data.attack + 1) / 2);
            }
        }

        ResolveDeath(attacker, attackerSlot);
        ResolveDeath(defender, targetSlot);
        if (behindSlot >= 0) ResolveDeath(defender, behindSlot);
    }

    public static void AttackLeader(PlayerState attacker, int attackerSlot, PlayerState defender)
    {
        CardInstance a = attacker.Board.GetCard(attackerSlot);
        defender.TakeDamage(a.Data.attack);
        a.CanAttack = false;

        // 守る側に武器があれば、攻撃力分を反撃して、耐久が1減る
        CardInstance weapon = defender.Weapon;
        if (weapon != null)
        {
            a.TakeDamage(weapon.Data.attack);
            weapon.TakeDamage(1);
            if (weapon.IsDead) defender.BreakWeapon();
            ResolveDeath(attacker, attackerSlot);
        }
    }

    // リーダーが武器で、相手のカードを攻撃する
    public static void LeaderAttackCard(PlayerState attacker, PlayerState defender, int targetSlot)
    {
        CardInstance weapon = attacker.Weapon; // 壊れる前に覚えておく
        CardInstance d = defender.Board.GetCard(targetSlot);

        d.TakeDamage(weapon.Data.attack);
        if (!weapon.Data.noCounter)
        {
            attacker.TakeDamage(d.Data.attack); // 相手のカードからの反撃を受ける
        }
        UseWeapon(attacker);

        ResolveDeath(defender, targetSlot);

        TriggerOnWeaponAttack(attacker, weapon); // 攻撃時の効果
    }
    // リーダーが武器で、相手のリーダーを攻撃する
    public static void LeaderAttackLeader(PlayerState attacker, PlayerState defender)
    {
        CardInstance weapon = attacker.Weapon; // 壊れる前に覚えておく

        defender.TakeDamage(weapon.Data.attack);
        UseWeapon(attacker);

        TriggerOnWeaponAttack(attacker, weapon); // 攻撃時の効果
    }
    private static void UseWeapon(PlayerState owner)
    {
        owner.LeaderUsedAttack();
        owner.Weapon.TakeDamage(1);
        if (owner.Weapon.IsDead) owner.BreakWeapon();
    }

    // キャラを出したときの効果(登場時)
    public static void TriggerOnPlay(PlayerState owner, CardInstance card)
    {
        CardEffect effect = card.Data.onPlayEffect;
        if (effect != null && !effect.NeedsTarget) effect.Apply(owner, owner.Opponent, EffectTarget.None);
    }

    public static void ResolveDeath(PlayerState owner, int slot)
    {
        CardInstance card = owner.Board.GetCard(slot);
        if (card == null || !card.IsDead) return;

        owner.Board.Remove(slot);

        if (slot == BoardState.CenterBackSlot)
        {
            int damage = LeaderDamageByCost(card.Data.cost);
            owner.TakeDamage(damage);
            owner.RaiseCenterBackFell(card, damage);
        }

        // 倒されたときの効果(撃破時)
        CardEffect effect = card.Data.onDeathEffect;
        if (effect != null && !effect.NeedsTarget) effect.Apply(owner, owner.Opponent, EffectTarget.None);
    }

    private static int LeaderDamageByCost(int cost)
    {
        if (cost <= 3) return 1;
        if (cost <= 6) return 2;
        return 3;
    }
    // 武器で攻撃したときの効果を使う
    private static void TriggerOnWeaponAttack(PlayerState me, CardInstance weapon)
    {
        if (weapon == null || weapon.Data.onAttackEffect == null) return;
        weapon.Data.onAttackEffect.Apply(me, me.Opponent, EffectTarget.None);
    }
}