public enum AiActionType
{
    None,        // 行動しなかった
    PlayCard,    // キャラを出した
    Equip,       // 武器・防具を装備した
    CastSpell,   // 魔法を使った
    Attack,      // キャラで攻撃した
    LeaderAttack // リーダーが武器で攻撃した
}

public static class SimpleAI
{
    private static readonly int[] PlaceOrder = { 1, 4, 0, 2, 3, 5 };

    // 直前の行動で使った(動かした)カード。画面に見せるために使う
    public static CardInstance LastActionCard { get; private set; }

    // 1ターン分の行動を、まとめて行う(画面の演出がいらないとき用)
    public static void TakeTurn(PlayerState me, PlayerState opponent)
    {
        int safety = 0;
        while (TakeNextAction(me, opponent) != AiActionType.None && safety < 50)
        {
            safety++;
        }
    }

    // 行動を1つだけ行う。何をしたかを返す。もうやることがなければ None
    public static AiActionType TakeNextAction(PlayerState me, PlayerState opponent)
    {
        LastActionCard = null;

        if (TryEquip(me)) return AiActionType.Equip;
        if (TryCastSpell(me, opponent)) return AiActionType.CastSpell;
        if (TryPlayUnit(me)) return AiActionType.PlayCard;
        if (TryAttackOnce(me, opponent)) return AiActionType.Attack;
        if (TryLeaderAttack(me, opponent)) return AiActionType.LeaderAttack;
        return AiActionType.None;
    }

    private static bool TryEquip(PlayerState me)
    {
        for (int h = me.Hand.Count - 1; h >= 0; h--)
        {
            CardInstance card = me.Hand[h];
            CardType type = card.Data.type;
            if (type != CardType.Weapon && type != CardType.Armor) continue;
            if (type == CardType.Weapon && me.Weapon != null) continue; // すでに武器があれば、置き換えない
            if (type == CardType.Armor && me.Armor != null) continue;   // すでに防具があれば、置き換えない
            if (!me.CanPay(card.Data.cost)) continue;

            me.Pay(card.Data.cost);
            me.Equip(card);
            me.Hand.RemoveAt(h);
            LastActionCard = card;
            return true;
        }
        return false;
    }

    // 手札の魔法の中で、いちばんおすすめ度が高いものを使う
    private static bool TryCastSpell(PlayerState me, PlayerState opponent)
    {
        int bestIndex = -1;
        int bestScore = 0;
        EffectTarget bestTarget = EffectTarget.None;

        for (int h = 0; h < me.Hand.Count; h++)
        {
            CardInstance card = me.Hand[h];
            if (card.Data.type != CardType.Spell) continue;
            if (!me.CanPay(card.Data.cost)) continue;

            CardEffect effect = card.Data.effect;
            if (effect == null) continue;

            int score = effect.AiScore(me, opponent, out EffectTarget target);
            if (score <= bestScore) continue;
            if (effect.NeedsTarget && !effect.CanTarget(me, opponent, target)) continue;

            bestIndex = h;
            bestScore = score;
            bestTarget = target;
        }

        if (bestIndex < 0) return false;

        CardInstance spell = me.Hand[bestIndex];
        me.Pay(spell.Data.cost);
        me.Hand.RemoveAt(bestIndex);
        spell.Data.effect.Apply(me, opponent, bestTarget);
        LastActionCard = spell;
        return true;
    }

    private static bool TryPlayUnit(PlayerState me)
    {
        for (int h = me.Hand.Count - 1; h >= 0; h--)
        {
            CardInstance card = me.Hand[h];
            if (card.Data.type != CardType.Unit) continue;
            if (!me.CanPay(card.Data.cost)) continue;

            foreach (int slot in PlaceOrder)
            {
                if (me.Board.Place(slot, card))
                {
                    me.Pay(card.Data.cost);
                    me.Hand.RemoveAt(h);
                    Combat.TriggerOnPlay(me, card);
                    LastActionCard = card;
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TryAttackOnce(PlayerState me, PlayerState opponent)
    {
        for (int slot = 0; slot < BoardState.SlotCount; slot++)
        {
            if (!Combat.CanAttackWith(me, slot)) continue;

            // リーダーを狙えるなら、リーダーを攻撃する
            if (Combat.CanTargetLeader(opponent))
            {
                LastActionCard = me.Board.GetCard(slot); // 攻撃の前に覚える
                Combat.AttackLeader(me, slot, opponent);
                return true;
            }

            // 狙えないなら、攻撃できるカードを攻撃する
            for (int target = 0; target < BoardState.SlotCount; target++)
            {
                if (Combat.CanTargetCard(opponent, target))
                {
                    LastActionCard = me.Board.GetCard(slot); // 攻撃の前に覚える
                    Combat.AttackCard(me, slot, opponent, target);
                    return true;
                }
            }

            // 攻撃できる相手がいなければ、このカードは、今回は攻撃しない
            me.Board.GetCard(slot).CanAttack = false;
        }

        return false;
    }

    // リーダーの武器での攻撃
    private static bool TryLeaderAttack(PlayerState me, PlayerState opponent)
    {
        if (!Combat.CanLeaderAttack(me)) return false;

        // リーダーを狙えるなら、リーダーを攻撃する
        if (Combat.CanTargetLeader(opponent))
        {
            LastActionCard = me.Weapon; // 武器が壊れる前に覚える
            Combat.LeaderAttackLeader(me, opponent);
            return true;
        }

        // 狙えないときは、武器で倒せるカードだけを攻撃する
        int weaponAttack = me.Weapon.Data.attack;
        for (int target = 0; target < BoardState.SlotCount; target++)
        {
            if (!Combat.CanTargetCard(opponent, target)) continue;

            CardInstance card = opponent.Board.GetCard(target);
            if (card.CurrentHealth <= weaponAttack)
            {
                LastActionCard = me.Weapon; // 武器が壊れる前に覚える
                Combat.LeaderAttackCard(me, opponent, target);
                return true;
            }
        }

        me.LeaderUsedAttack(); // 攻撃しないことにして、今回は、もう考えない
        return false;
    }
}