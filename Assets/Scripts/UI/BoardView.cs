using System;
using UnityEngine;
using UnityEngine.UI;

// マスに付ける、操作のヒント
public enum SlotHint
{
    None,
    Placeable,   // カードを出せる空きマス
    Attackable,  // 攻撃・魔法の対象にできる相手
    Blocked      // 守られていて、攻撃できない相手
}

public class BoardView : MonoBehaviour
{
    [SerializeField] private Transform[] slots;
    [SerializeField] private CardView cardPrefab;

    [Header("マスの見た目")]
    [SerializeField] private Color normalOutline = new Color(0.56f, 0.72f, 0.85f, 0.63f);
    [SerializeField] private Color guardOutline = new Color(0.4f, 0.8f, 1f, 1f); // 中央後列の縁(リーダーを守るマス)
    [SerializeField] private Sprite guardSlotSprite; // 中央後列のマスに使う絵(空なら変えない)

    [Header("操作のヒント")]
    [SerializeField] private Color placeableOutline = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color attackableOutline = new Color(1f, 0.3f, 0.3f, 1f);

    public event Action<int> SlotClicked;

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            slots[i].GetComponent<Button>().onClick.AddListener(() => SlotClicked?.Invoke(index));

            if (i == BoardState.CenterBackSlot && guardSlotSprite != null)
            {
                Image slotImage = slots[i].GetComponent<Image>();
                if (slotImage != null) slotImage.sprite = guardSlotSprite;
            }

            ApplyHint(i, SlotHint.None);
        }
    }

    private Color BaseOutline(int slot)
    {
        return slot == BoardState.CenterBackSlot ? guardOutline : normalOutline;
    }

    // マスの縁の色で、ヒントを出す
    private void ApplyHint(int slot, SlotHint hint)
    {
        Outline outline = slots[slot].GetComponent<Outline>();
        Transform hintObject = slots[slot].Find("Hint");
        Image hintImage = hintObject != null ? hintObject.GetComponent<Image>() : null;

        switch (hint)
        {
            case SlotHint.Placeable:
                SetHint(outline, hintImage, placeableOutline, 4f);
                break;
            case SlotHint.Attackable:
                SetHint(outline, hintImage, attackableOutline, 4f);
                break;
            default:
                if (outline != null)
                {
                    outline.effectColor = BaseOutline(slot);
                    outline.effectDistance = new Vector2(2f, -2f);
                }
                if (hintImage != null) hintImage.gameObject.SetActive(false);
                break;
        }
    }

    private void SetHint(Outline outline, Image hintImage, Color color, float distance)
    {
        if (outline != null)
        {
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
        }
        if (hintImage != null)
        {
            hintImage.gameObject.SetActive(true);
            hintImage.color = new Color(color.r, color.g, color.b, 0.25f);
        }
    }

    public void Refresh(BoardState state, int selectedSlot = -1, Func<int, SlotHint> hintFor = null)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SlotHint hint = hintFor != null ? hintFor(i) : SlotHint.None;
            ApplyHint(i, hint);

            foreach (Transform child in slots[i])
            {
                if (child.name == "Hint") continue; // ヒントの枠は消さない
                Destroy(child.gameObject);
            }

            CardInstance card = state.GetCard(i);
            if (card == null) continue;

            CardView view = Instantiate(cardPrefab, slots[i]);
            view.SetCard(card);
            view.SetCompact(true);

            // 盤面のカードのボタンは切って、クリックをマス側で受ける
            view.GetComponent<Button>().enabled = false;

            // 攻撃役に選んだカードは色を変える
            if (i == selectedSlot)
            {
                view.SetHighlight(true);
            }

            // 守られていて攻撃できない相手は、暗くする
            if (hint == SlotHint.Blocked)
            {
                view.SetDim(true);
            }
        }
    }
}