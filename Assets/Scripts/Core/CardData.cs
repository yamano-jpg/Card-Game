using UnityEngine;

public enum Faction
{
    Common, // 共通
    Hero,   // 勇者
    Demon   // 魔王
}

public enum CardType
{
    Unit,   // キャラクター
    Spell,  // 魔法
    Weapon, // 武器
    Armor   // 防具
}

[CreateAssetMenu(menuName = "CardGame/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Faction faction;
    public CardType type;
    public int cost;
    public int attack;
    public int health; // 武器・防具では「耐久」として使う
    public Sprite illustration;
    [TextArea] public string effectText; // カードに出す、効果の説明
    public CardEffect effect;
    public CardEffect onPlayEffect;  // 登場時の効果
    public CardEffect onDeathEffect; // 撃破時の効果
    public CardEffect onAttackEffect; // 武器で攻撃したときの効果
    public bool pierce;
    public bool noCounter; // 武器で攻撃したとき、相手のカードの反撃を受けない
    [Header("イラストの見せ方")]
    public float illustrationScale = 1f; // 大きさの倍率
    public Vector2 illustrationOffset;   // 位置のずれ
}