using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const float ItemIconSize = 200f;   // 魔法・装備のアイコンの大きさ
    private const float HoverScale = 1.5f;     // カーソルを合わせたとき、大きくなる倍率
    private const float SelectedScale = 1.15f; // 手札で選んでいるカードの倍率
    private static readonly Color HighlightColor = new Color(1f, 0.85f, 0.3f);

    [Header("枠")]
    [SerializeField] private Image highlightImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite heroFrame;
    [SerializeField] private Sprite demonFrame;
    [SerializeField] private Sprite commonFrame;

    [Header("中身")]
    [SerializeField] private Image illustrationImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private GameObject attackBadge;
    [SerializeField] private GameObject healthBadge;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text healthText;

    private Vector2 defaultArtSize;
    private Vector2 defaultArtPos;
    private Vector3 baseScale;
    private bool hoverEnabled;
    private bool hovering;
    private bool selected;
    private bool pulsing;

    private void Awake()
    {
        RectTransform artRect = illustrationImage.rectTransform;
        defaultArtSize = artRect.sizeDelta;
        defaultArtPos = artRect.anchoredPosition;

        baseScale = transform.localScale;
    }

    // 選ばれているカードの縁を、ゆっくり脈打たせる
    private void Update()
    {
        if (!pulsing || highlightImage == null) return;

        Color c = HighlightColor;
        c.a = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 6f);
        highlightImage.color = c;
    }

    // 手札用: 普段は小さく、カーソルを合わせると大きくなって、前に出る
    public void EnableHover()
    {
        hoverEnabled = true;
        ((RectTransform)transform).pivot = new Vector2(0.5f, 0f); // 下の端を基準に、上へ大きくなる
        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hoverEnabled) return;
        hovering = true;
        RefreshVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!hoverEnabled) return;
        hovering = false;
        RefreshVisual();
    }

    // 手札のカードの、大きさと前後の見え方を決める
    private void RefreshVisual()
    {
        if (!hoverEnabled) return;

        float scale = hovering ? HoverScale : (selected ? SelectedScale : 1f);
        transform.localScale = baseScale * scale;
        SetCompact(!hovering);
        BringToFront(hovering || selected);
    }

    private void BringToFront(bool front)
    {
        Canvas c = GetComponent<Canvas>();
        if (front)
        {
            if (c == null) c = gameObject.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 10;
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        }
        else if (c != null)
        {
            c.overrideSorting = false;
        }
    }

    public void SetData(CardData data)
    {
        Show(data, data.health);
    }

    // 盤面のカード用: いまの体力を表示する
    public void SetCard(CardInstance card)
    {
        Show(card.Data, card.CurrentHealth);
    }

    // 小さいカード用: 種類と説明文を隠す
    public void SetCompact(bool compact)
    {
        typeText.gameObject.SetActive(!compact);
        effectText.gameObject.SetActive(!compact);
    }

    // 選ばれているカードの強調
    public void SetHighlight(bool on)
    {
        selected = on;
        pulsing = on;
        highlightImage.enabled = on;
        if (on) highlightImage.color = HighlightColor;
        RefreshVisual();
    }

    // 攻撃できない相手などを、暗くする
    public void SetDim(bool dim)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = dim ? 0.45f : 1f;
    }

    private void Show(CardData data, int health)
    {
        nameText.text = data.cardName;
        costText.text = data.cost.ToString();
        typeText.text = FactionName(data.faction) + " / " + TypeName(data.type);
        effectText.text = BuildEffectText(data);

        Sprite frame = FrameSprite(data.faction);
        if (frame != null) frameImage.sprite = frame;

        illustrationImage.sprite = data.illustration;
        illustrationImage.enabled = data.illustration != null;

        // イラストの大きさと位置
        RectTransform artRect = illustrationImage.rectTransform;
        if (data.type == CardType.Unit)
        {
            // キャラは、プレハブに設定した、元の大きさと位置のまま
            artRect.sizeDelta = defaultArtSize * data.illustrationScale;
            artRect.anchoredPosition = defaultArtPos + data.illustrationOffset;
        }
        else
        {
            // 魔法・装備のアイコンは、大きく見せる
            float size = ItemIconSize * data.illustrationScale;
            artRect.sizeDelta = new Vector2(size, size);
            artRect.anchoredPosition = defaultArtPos + data.illustrationOffset;
        }

        // 魔法には攻撃力も体力もない。防具には攻撃力がない
        bool hasAttack = data.type == CardType.Unit || data.type == CardType.Weapon;
        bool hasHealth = data.type != CardType.Spell;
        attackBadge.SetActive(hasAttack);
        healthBadge.SetActive(hasHealth);
        attackText.text = data.attack.ToString();
        healthText.text = health.ToString();
    }

    private string BuildEffectText(CardData data)
    {
        string text = data.effectText;
        if (data.pierce) text = "波及: 前列を攻撃したとき、後ろにも半分のダメージ\n" + text;
        return text;
    }

    private Sprite FrameSprite(Faction faction)
    {
        switch (faction)
        {
            case Faction.Hero: return heroFrame;
            case Faction.Demon: return demonFrame;
            default: return commonFrame;
        }
    }

    private string FactionName(Faction faction)
    {
        switch (faction)
        {
            case Faction.Hero: return "勇者";
            case Faction.Demon: return "魔王";
            default: return "共通";
        }
    }

    private string TypeName(CardType type)
    {
        switch (type)
        {
            case CardType.Spell: return "魔法";
            case CardType.Weapon: return "武器";
            case CardType.Armor: return "防具";
            default: return "キャラクター";
        }
    }
}