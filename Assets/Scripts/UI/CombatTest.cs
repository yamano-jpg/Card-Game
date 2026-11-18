using UnityEngine;

public class CombatTest : MonoBehaviour
{
    [SerializeField] private CardData cardData;

    private void Start()
    {
        PlayerState me = new PlayerState();
        PlayerState enemy = new PlayerState();

        me.Board.Place(0, new CardInstance(cardData));
        enemy.Board.Place(1, new CardInstance(cardData));
        enemy.Board.Place(4, new CardInstance(cardData));

        Debug.Log("出したターン: " + Combat.CanAttackWith(me, 0));
        me.StartTurn();
        Debug.Log("次のターン: " + Combat.CanAttackWith(me, 0));

        Debug.Log("4番を狙える: " + Combat.CanTargetCard(enemy, 4));
        Debug.Log("リーダーを狙える: " + Combat.CanTargetLeader(enemy));

        Combat.AttackCard(me, 0, enemy, 1);
        Debug.Log("相手の1番の体力: " + enemy.Board.GetCard(1).CurrentHealth);
        Debug.Log("自分の0番の体力: " + me.Board.GetCard(0).CurrentHealth);
    }
}