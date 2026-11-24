using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
    private const int LeaderAttacker = 6;

    [SerializeField] private BoardView boardView;
    [SerializeField] private BoardView enemyBoardView;
    [SerializeField] private HandView handView;
    [SerializeField] private DeckData playerDeckData;
    [SerializeField] private DeckData enemyDeckData;
    [SerializeField] private TMP_Text manaText;       // 自分のマナ
    [SerializeField] private TMP_Text enemyManaText;  // 相手のマナ
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button playerLeaderButton;
    [SerializeField] private Button enemyLeaderButton;
    [SerializeField] private Button backToHomeButton;
    [SerializeField] private CardView detailCard;
    [SerializeField] private Button backgroundButton;
    [SerializeField] private GameObject inputBlocker;       // 相手のターンの間、操作を止める見えない板
    [SerializeField] private float enemyStepDelay = 0.8f;   // 相手の行動と行動の間の、待ち時間(秒)

    [Header("効果音")]
    [SerializeField] private AudioClip selectSe;
    [SerializeField] private AudioClip playUnitSe;
    [SerializeField] private AudioClip equipSe;
    [SerializeField] private AudioClip spellSe;
    [SerializeField] private AudioClip attackSe;
    [SerializeField] private AudioClip endTurnSe;
    [SerializeField] private AudioClip winSe;
    [SerializeField] private AudioClip loseSe;
    [SerializeField] private TMP_Text messageText; // 「マナが足りません」などを出す
    [Header("リーダー表示")]
    [SerializeField] private Image playerPortrait;
    [SerializeField] private Image enemyPortrait;
    [SerializeField] private Image playerLeaderFrame;   // 攻撃役に選んだとき、色が変わる枠
    [SerializeField] private TMP_Text playerHealthNumber;
    [SerializeField] private TMP_Text enemyHealthNumber;
    [SerializeField] private GameObject playerArmorBadge;
    [SerializeField] private TMP_Text playerArmorNumber;
    [SerializeField] private GameObject enemyArmorBadge;
    [SerializeField] private TMP_Text enemyArmorNumber;
    [SerializeField] private GameObject playerWeaponBadge;
    [SerializeField] private TMP_Text playerWeaponNumber;
    [SerializeField] private GameObject enemyWeaponBadge;
    [SerializeField] private TMP_Text enemyWeaponNumber;
    [Header("やられ演出")]
    [SerializeField] private LeaderHitEffect hitEffect;
    [SerializeField] private RectTransform playerLeaderRect;
    [SerializeField] private RectTransform enemyLeaderRect;
    [SerializeField] private Image enemyLeaderFrame;
    [SerializeField] private RectTransform playerCenterBackSlot; // 自分の中央後列のマス
    [SerializeField] private RectTransform enemyCenterBackSlot;  // 相手の中央後列のマス

    private PlayerState player;
    private PlayerState enemy;
    private int selectedHandIndex = -1;
    private int selectedAttackerSlot = -1;
    private CardInstance inspectedCard; // 盤面のカードを、クリックして、詳細を見ているもの
    private CardInstance enemyActionCard; // 相手が、いま使ったカード(詳細表示に出す)
    private bool isGameOver;
    private Coroutine messageRoutine;

    private void Start()
    {
        player = new PlayerState();
        enemy = new PlayerState();
        player.Opponent = enemy;
        enemy.Opponent = player;
        player.Fatigued += damage => ShowMessage("デッキ切れ！ 自分のリーダーに" + damage + "ダメージ");
        enemy.Fatigued += damage => ShowMessage("デッキ切れ！ 相手のリーダーに" + damage + "ダメージ");
        player.CenterBackFell += (card, damage) => PlayCenterBackFell(playerCenterBackSlot, playerLeaderRect, playerLeaderFrame, damage);
        enemy.CenterBackFell += (card, damage) => PlayCenterBackFell(enemyCenterBackSlot, enemyLeaderRect, enemyLeaderFrame, damage);
        player.Damaged += damage => PlayDamaged(playerLeaderRect, playerLeaderFrame, damage);
        enemy.Damaged += damage => PlayDamaged(enemyLeaderRect, enemyLeaderFrame, damage);
        resultText.text = "";
        if (messageText != null) messageText.gameObject.SetActive(false);

        // 戦闘中は、ボタンを押したときの自動のクリック音を止める
        if (AudioManager.Instance != null) AudioManager.Instance.SuppressAutoClick = true;

        backToHomeButton.gameObject.SetActive(false);
        backToHomeButton.onClick.AddListener(OnBackToHome);

        DeckData playerDeck = GameSession.PlayerDeck != null ? GameSession.PlayerDeck : playerDeckData;
        DeckData enemyDeck = GameSession.EnemyDeck != null ? GameSession.EnemyDeck : enemyDeckData;
        ApplyPortrait(playerPortrait, playerDeck);
        ApplyPortrait(enemyPortrait, enemyDeck);
        player.SetDeck(playerDeck.ToCardList());
        enemy.SetDeck(enemyDeck.ToCardList());

        for (int i = 0; i < 4; i++)
        {
            player.DrawCard();
            enemy.DrawCard();
        }

        handView.CardClicked += OnHandCardClicked;
        boardView.SlotClicked += OnMySlotClicked;
        enemyBoardView.SlotClicked += OnEnemySlotClicked;
        endTurnButton.onClick.AddListener(OnEndTurn);
        backgroundButton.onClick.AddListener(OnBackgroundClicked);
        inputBlocker.SetActive(false);
        playerLeaderButton.onClick.AddListener(OnMyLeaderClicked);
        enemyLeaderButton.onClick.AddListener(OnEnemyLeaderClicked);

        player.StartTurn();
        RefreshViews();
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SuppressAutoClick = false;
    }

    private void PlayCenterBackFell(RectTransform slot, RectTransform leader, Image frame, int damage)
    {
        if (hitEffect == null || slot == null || leader == null) return;
        hitEffect.Play(slot.position, leader, frame, damage);
    }

    private void PlayDamaged(RectTransform leader, Image frame, int damage)
    {
        if (hitEffect == null || leader == null) return;
        hitEffect.PlayDamage(leader, frame, damage);
    }

    // 画面に、短いメッセージを、1.5秒だけ出す
    private void ShowMessage(string text)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text));
    }

    private IEnumerator MessageRoutine(string text)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        messageText.gameObject.SetActive(false);
        messageRoutine = null;
    }

    private void PlaySe(AudioClip clip)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySe(clip);
    }

    private void PlayJingle(AudioClip clip)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayJingle(clip);
    }

    // 何もない所をクリックしたら、選択をすべて解除する
    private void OnBackgroundClicked()
    {
        if (isGameOver) return;

        selectedHandIndex = -1;
        selectedAttackerSlot = -1;
        inspectedCard = null;
        RefreshViews();
    }

    private void OnBackToHome()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuClick();
        SceneManager.LoadScene(SceneNames.Home);
    }

    private void OnHandCardClicked(int index)
    {
        if (isGameOver) return;

        // 選んでいる、対象を選ばない魔法を、もう一度クリックしたら、使う
        if (index == selectedHandIndex && TryCastTargetlessSpell()) return;

        selectedHandIndex = index;
        selectedAttackerSlot = -1;
        inspectedCard = null;
        PlaySe(selectSe);
        RefreshViews();
    }

    private void OnMySlotClicked(int slotIndex)
    {
        if (isGameOver) return;

        CardInstance clicked = player.Board.GetCard(slotIndex);
        bool slotIsEmpty = clicked == null;

        if (selectedHandIndex >= 0 && slotIsEmpty)
        {
            PlayUnit(slotIndex);
            return;
        }

        // 盤面のカードをクリックしたら、詳細を出す
        if (clicked != null)
        {
            inspectedCard = clicked;
            selectedHandIndex = -1;
        }

        // 攻撃できるカードなら、攻撃役としても選ぶ
        if (Combat.CanAttackWith(player, slotIndex))
        {
            selectedAttackerSlot = slotIndex;
            PlaySe(selectSe);
        }

        if (clicked != null) RefreshViews();
    }

    private void PlayUnit(int slotIndex)
    {
        CardInstance card = player.Hand[selectedHandIndex];

        if (card.Data.type != CardType.Unit)
        {
            ShowMessage("このカードは盤面に出せません(魔法は相手に、武器・防具は自分のリーダーに使います)");
            return;
        }

        if (!player.CanPay(card.Data.cost))
        {
            ShowMessage("マナが足りません");
            return;
        }

        player.Board.Place(slotIndex, card);
        player.Pay(card.Data.cost);
        player.Hand.RemoveAt(selectedHandIndex);
        selectedHandIndex = -1;
        Combat.TriggerOnPlay(player, card);
        PlaySe(playUnitSe);
        RefreshViews();
    }

    private void OnMyLeaderClicked()
    {
        if (isGameOver) return;

        if (selectedHandIndex < 0)
        {
            if (Combat.CanLeaderAttack(player))
            {
                selectedAttackerSlot = LeaderAttacker;
                PlaySe(selectSe);
                RefreshViews();
            }
            return;
        }

        CardInstance card = player.Hand[selectedHandIndex];
        bool isEquipment = card.Data.type == CardType.Weapon || card.Data.type == CardType.Armor;
        if (!isEquipment) return;

        if (!player.CanPay(card.Data.cost))
        {
            ShowMessage("マナが足りません");
            return;
        }

        player.Pay(card.Data.cost);
        player.Equip(card);
        player.Hand.RemoveAt(selectedHandIndex);
        selectedHandIndex = -1;
        PlaySe(equipSe);
        RefreshViews();
    }

        // 対象を選ばない魔法を使う。使える魔法を選んでいなければ false を返す
    private bool TryCastTargetlessSpell()
    {
        if (selectedHandIndex < 0) return false;

        CardInstance card = player.Hand[selectedHandIndex];
        if (card.Data.type != CardType.Spell) return false;

        CardEffect effect = card.Data.effect;
        if (effect == null || effect.NeedsTarget) return false;

        if (!player.CanPay(card.Data.cost))
        {
            ShowMessage("マナが足りません");
            return true;
        }

        player.Pay(card.Data.cost);
        effect.Apply(player, enemy, EffectTarget.None);
        player.Hand.RemoveAt(selectedHandIndex);
        selectedHandIndex = -1;
        PlaySe(spellSe);
        RefreshViews();
        return true;
    }

    private bool TryCastSpell(EffectTarget target)
    {
        if (selectedHandIndex < 0) return false;

        CardInstance card = player.Hand[selectedHandIndex];
        if (card.Data.type != CardType.Spell) return false;

        CardEffect effect = card.Data.effect;
        if (effect != null && !effect.NeedsTarget) return false; // 対象を選ばない魔法は、ここでは使わない
        if (effect == null)
        {
            ShowMessage("この魔法には、効果が設定されていません");
            return true;
        }

        if (!player.CanPay(card.Data.cost))
        {
            ShowMessage("マナが足りません");
            return true;
        }

        if (!effect.CanTarget(player, enemy, target))
        {
            ShowMessage("その対象には使えません");
            return true;
        }

        player.Pay(card.Data.cost);
        effect.Apply(player, enemy, target);
        player.Hand.RemoveAt(selectedHandIndex);
        selectedHandIndex = -1;
        PlaySe(spellSe);
        RefreshViews();
        return true;
    }

    private void OnEnemySlotClicked(int slotIndex)
    {
        if (isGameOver) return;

        if (TryCastSpell(new EffectTarget(false, slotIndex))) return;

        // 攻撃役を選んでいなければ、相手のカードの詳細を出す
        if (selectedAttackerSlot < 0)
        {
            CardInstance clicked = enemy.Board.GetCard(slotIndex);
            if (clicked != null)
            {
                inspectedCard = clicked;
                selectedHandIndex = -1;
                RefreshViews();
            }
            return;
        }

        if (!Combat.CanTargetCard(enemy, slotIndex))
        {
            ShowMessage("そのカードは攻撃できません(守られているか、カードがいません)");
            return;
        }

        if (selectedAttackerSlot == LeaderAttacker)
        {
            Combat.LeaderAttackCard(player, enemy, slotIndex);
        }
        else
        {
            Combat.AttackCard(player, selectedAttackerSlot, enemy, slotIndex);
        }

        selectedAttackerSlot = -1;
        PlaySe(attackSe);
        RefreshViews();
    }

    private void OnEnemyLeaderClicked()
    {
        if (isGameOver) return;

        if (TryCastSpell(new EffectTarget(true, 0))) return;

        if (selectedAttackerSlot < 0) return;

        if (!Combat.CanTargetLeader(enemy))
        {
            ShowMessage("リーダーは守られています");
            return;
        }

        if (selectedAttackerSlot == LeaderAttacker)
        {
            Combat.LeaderAttackLeader(player, enemy);
        }
        else
        {
            Combat.AttackLeader(player, selectedAttackerSlot, enemy);
        }

        selectedAttackerSlot = -1;
        PlaySe(attackSe);
        RefreshViews();
    }

    private void OnEndTurn()
    {
        if (isGameOver) return;
        if (inputBlocker.activeSelf) return; // 相手のターンの間は、押せない

        PlaySe(endTurnSe);

        selectedHandIndex = -1;
        selectedAttackerSlot = -1;
        inspectedCard = null;

        StartCoroutine(EnemyTurnRoutine());
    }

    // 相手のターン: 行動を、1つずつ、間を空けて見せる
    private IEnumerator EnemyTurnRoutine()
    {
        inputBlocker.SetActive(true); // 相手のターンの間は、操作を受け付けない
        RefreshViews();
        yield return new WaitForSeconds(enemyStepDelay);

        enemy.StartTurn();
        enemy.DrawCard();
        RefreshViews();
        yield return new WaitForSeconds(enemyStepDelay);

        int safety = 0;
        while (!isGameOver && safety < 50)
        {
            AiActionType action = SimpleAI.TakeNextAction(enemy, player);
            if (action == AiActionType.None) break;

            safety++;
            switch (action)
            {
                case AiActionType.PlayCard: PlaySe(playUnitSe); break;
                case AiActionType.Equip: PlaySe(equipSe); break;
                case AiActionType.CastSpell: PlaySe(spellSe); break;
                case AiActionType.Attack:
                case AiActionType.LeaderAttack: PlaySe(attackSe); break;
            }

           enemyActionCard = SimpleAI.LastActionCard;
            RefreshViews();
            yield return new WaitForSeconds(enemyStepDelay);
        }

        inputBlocker.SetActive(false);
        enemyActionCard = null;
        if (isGameOver) yield break;

        player.StartTurn();
        player.DrawCard();
        RefreshViews();
    }

    // デッキに設定された絵・大きさ・位置を、リーダーの表示に反映する
    private void ApplyPortrait(Image portrait, DeckData deck)
    {
        if (portrait == null || deck.leaderPortrait == null) return;

        portrait.sprite = deck.leaderPortrait;
        RectTransform rect = portrait.rectTransform;
        rect.sizeDelta = deck.leaderPortraitSize;
        rect.anchoredPosition = deck.leaderPortraitOffset;
    }

    private void UpdateLeaderView(PlayerState p, TMP_Text healthNumber, GameObject armorBadge, TMP_Text armorNumber, GameObject weaponBadge, TMP_Text weaponNumber)
    {
        healthNumber.text = p.Health.ToString();

        bool hasArmor = p.Armor != null;
        armorBadge.SetActive(hasArmor);
        if (hasArmor) armorNumber.text = p.Armor.CurrentHealth.ToString();

        bool hasWeapon = p.Weapon != null;
        weaponBadge.SetActive(hasWeapon);
        if (hasWeapon) weaponNumber.text = p.Weapon.Data.attack + "/" + p.Weapon.CurrentHealth;
    }

    // 手札のキャラを選んでいる間、出せる空きマスを光らせる
    private SlotHint PlayerBoardHint(int slot)
    {
        if (selectedHandIndex < 0 || selectedHandIndex >= player.Hand.Count) return SlotHint.None;

        CardInstance card = player.Hand[selectedHandIndex];
        bool canPlace = card.Data.type == CardType.Unit
                        && player.Board.GetCard(slot) == null
                        && player.CanPay(card.Data.cost);
        return canPlace ? SlotHint.Placeable : SlotHint.None;
    }

    // 攻撃役か、対象を選ぶ魔法を選んでいる間、狙える相手を赤く、狙えない相手を暗くする
    private SlotHint EnemyBoardHint(int slot)
    {
        if (enemy.Board.GetCard(slot) == null) return SlotHint.None;

        if (selectedAttackerSlot >= 0)
        {
            return Combat.CanTargetCard(enemy, slot) ? SlotHint.Attackable : SlotHint.Blocked;
        }

        if (SelectedSpellNeedsTarget()) return SlotHint.Attackable;
        return SlotHint.None;
    }

    private bool SelectedSpellNeedsTarget()
    {
        if (selectedHandIndex < 0 || selectedHandIndex >= player.Hand.Count) return false;

        CardData data = player.Hand[selectedHandIndex].Data;
        return data.type == CardType.Spell && data.effect != null && data.effect.NeedsTarget;
    }

    // このカードが、いま、どちらかの盤面にいるか
    private bool IsOnBoard(CardInstance card)
    {
        for (int i = 0; i < BoardState.SlotCount; i++)
        {
            if (player.Board.GetCard(i) == card || enemy.Board.GetCard(i) == card) return true;
        }
        return false;
    }

    private void RefreshViews()
    {
        boardView.Refresh(player.Board, selectedAttackerSlot, PlayerBoardHint);
        enemyBoardView.Refresh(enemy.Board, -1, EnemyBoardHint);
        handView.Refresh(player.Hand, selectedHandIndex);
        // 倒されて、盤面から消えたカードの詳細は、消す
        if (inspectedCard != null && !IsOnBoard(inspectedCard)) inspectedCard = null;

        // 手札を選んでいる間は、手札の詳細。そうでなければ、盤面のカードの詳細
        if (selectedHandIndex >= 0)
        {
            detailCard.gameObject.SetActive(true);
            detailCard.SetData(player.Hand[selectedHandIndex].Data);
            detailCard.SetCompact(false);
        }
        else if (inspectedCard != null)
        {
            detailCard.gameObject.SetActive(true);
            detailCard.SetCard(inspectedCard);
            detailCard.SetCompact(false);
        }
        else if (enemyActionCard != null)
        {
            detailCard.gameObject.SetActive(true);
            if (IsOnBoard(enemyActionCard)) detailCard.SetCard(enemyActionCard);
            else detailCard.SetData(enemyActionCard.Data); // 魔法・装備・倒されたカード
            detailCard.SetCompact(false);
        }
        else
        {
            detailCard.gameObject.SetActive(false);
        }
        manaText.text = player.Mana + " / " + player.MaxMana;
        if (enemyManaText != null) enemyManaText.text = enemy.Mana + " / " + enemy.MaxMana;

        UpdateLeaderView(player, playerHealthNumber, playerArmorBadge, playerArmorNumber, playerWeaponBadge, playerWeaponNumber);
        UpdateLeaderView(enemy, enemyHealthNumber, enemyArmorBadge, enemyArmorNumber, enemyWeaponBadge, enemyWeaponNumber);
        if (playerLeaderFrame != null)
        {
            playerLeaderFrame.color = selectedAttackerSlot == LeaderAttacker ? new Color(1f, 0.85f, 0.3f) : Color.white;
        }
        UpdateResult();
    }

    private void UpdateResult()
    {
        if (isGameOver) return;

        if (enemy.Health <= 0)
        {
            isGameOver = true;
            resultText.text = "勝利";
            PlayJingle(winSe);
        }
        else if (player.Health <= 0)
        {
            isGameOver = true;
            resultText.text = "敗北";
            PlayJingle(loseSe);
        }

        if (isGameOver) backToHomeButton.gameObject.SetActive(true);
    }
}