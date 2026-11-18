using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;

    public event Action<int> CardClicked;

    public void Refresh(List<CardInstance> hand, int selectedIndex)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < hand.Count; i++)
        {
            CardView view = Instantiate(cardPrefab, transform);
            view.SetData(hand[i].Data);
            view.EnableHover();

            // 選ばれているカードは色を変える
            if (i == selectedIndex)
            {
                view.SetHighlight(true);
            }

            // ループ変数 i をそのまま使うと全部同じ番号になるので、コピーを使う
            int index = i;
            view.GetComponent<Button>().onClick.AddListener(() => CardClicked?.Invoke(index));
        }
    }
}