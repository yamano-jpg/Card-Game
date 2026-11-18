using System;
using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [SerializeField] private Transform[] slots;
    [SerializeField] private CardView cardPrefab;

    public event Action<int> SlotClicked;

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            slots[i].GetComponent<Button>().onClick.AddListener(() => SlotClicked?.Invoke(index));
        }
    }

    public void Refresh(BoardState state, int selectedSlot = -1)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            foreach (Transform child in slots[i])
            {
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
        }
    }
}